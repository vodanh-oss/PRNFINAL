using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartStudyAssistant.Core.Models;
using SmartStudyAssistant.Core.Services.Ai;
using SmartStudyAssistant.Core.Services.Database;

namespace SmartStudyAssistant.Wpf.ViewModels;

public partial class FlashcardGroupItem : ObservableObject
{
    [ObservableProperty]
    private string _groupName = string.Empty;

    [ObservableProperty]
    private int _cardCount;

    [ObservableProperty]
    private bool _isSelected;

    public string DisplayText => $"{GroupName} ({CardCount})";
}

public partial class FlashcardsViewModel : ObservableObject
{
    private readonly IDatabaseService _databaseService;
    private readonly AiServiceCoordinator _aiCoordinator;
    private List<Flashcard> _allCards = new();

    [ObservableProperty]
    private ObservableCollection<FlashcardGroupItem> _groupItems = new();

    [ObservableProperty]
    private string _selectedGroupName = "Tất cả";

    [ObservableProperty]
    private ObservableCollection<Flashcard> _flashcards = new();

    [ObservableProperty]
    private Flashcard? _currentCard;

    [ObservableProperty]
    private int _currentCardIndex;

    [ObservableProperty]
    private int _displayCardNumber = 1;

    [ObservableProperty]
    private int _totalCardsCount = 0;

    [ObservableProperty]
    private double _progressPercentage = 0;

    [ObservableProperty]
    private string _progressText = "Chưa có thẻ nào";

    [ObservableProperty]
    private bool _isFlipped;

    [ObservableProperty]
    private bool _isReviewMode;

    [ObservableProperty]
    private string _newFront = string.Empty;

    [ObservableProperty]
    private string _newBack = string.Empty;

    [ObservableProperty]
    private string _newSubject = "Chung";

    [ObservableProperty]
    private bool _isGeneratingCards;

    [ObservableProperty]
    private string _aiGenerateTopic = string.Empty;

    public FlashcardsViewModel(IDatabaseService databaseService, AiServiceCoordinator aiCoordinator)
    {
        _databaseService = databaseService;
        _aiCoordinator = aiCoordinator;
    }

    public async Task InitializeAsync()
    {
        await LoadFlashcardsAsync();
    }

    public void RefreshGroups(string? preferredGroup = null)
    {
        var distinctSubjects = _allCards
            .Select(c => string.IsNullOrWhiteSpace(c.SubjectName) ? "Chung" : c.SubjectName.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(s => s)
            .ToList();

        var newGroupList = new ObservableCollection<FlashcardGroupItem>();

        // Always add "Tất cả" as first group
        newGroupList.Add(new FlashcardGroupItem
        {
            GroupName = "Tất cả",
            CardCount = _allCards.Count
        });

        foreach (var sub in distinctSubjects)
        {
            int count = _allCards.Count(c =>
                (string.IsNullOrWhiteSpace(c.SubjectName) ? "Chung" : c.SubjectName.Trim())
                .Equals(sub, StringComparison.OrdinalIgnoreCase));

            newGroupList.Add(new FlashcardGroupItem
            {
                GroupName = sub,
                CardCount = count
            });
        }

        GroupItems = newGroupList;

        // Select preferred group or fallback to currently selected or first
        if (!string.IsNullOrWhiteSpace(preferredGroup) &&
            GroupItems.Any(g => g.GroupName.Equals(preferredGroup, StringComparison.OrdinalIgnoreCase)))
        {
            SelectedGroupName = GroupItems.First(g => g.GroupName.Equals(preferredGroup, StringComparison.OrdinalIgnoreCase)).GroupName;
        }
        else if (!GroupItems.Any(g => g.GroupName.Equals(SelectedGroupName, StringComparison.OrdinalIgnoreCase)))
        {
            SelectedGroupName = GroupItems.Count > 1 ? GroupItems[1].GroupName : "Tất cả";
        }

        foreach (var item in GroupItems)
        {
            item.IsSelected = item.GroupName.Equals(SelectedGroupName, StringComparison.OrdinalIgnoreCase);
        }

        ApplyGroupFilter();
    }

    public void ApplyGroupFilter()
    {
        if (SelectedGroupName == "Tất cả")
        {
            Flashcards = new ObservableCollection<Flashcard>(_allCards);
        }
        else
        {
            var filtered = _allCards
                .Where(c => (string.IsNullOrWhiteSpace(c.SubjectName) ? "Chung" : c.SubjectName.Trim())
                    .Equals(SelectedGroupName, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Flashcards = new ObservableCollection<Flashcard>(filtered);
        }

        CurrentCardIndex = 0;
        if (Flashcards.Count > 0)
        {
            CurrentCard = Flashcards[0];
            IsFlipped = false;
        }
        else
        {
            CurrentCard = null;
        }

        if (SelectedGroupName != "Tất cả")
        {
            NewSubject = SelectedGroupName;
        }

        UpdateProgress();
    }

    [RelayCommand]
    public void SelectGroup(string groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName)) return;

        SelectedGroupName = groupName;
        foreach (var item in GroupItems)
        {
            item.IsSelected = item.GroupName.Equals(groupName, StringComparison.OrdinalIgnoreCase);
        }

        ApplyGroupFilter();
    }

    public void UpdateProgress()
    {
        TotalCardsCount = Flashcards.Count;
        if (TotalCardsCount == 0)
        {
            DisplayCardNumber = 0;
            ProgressPercentage = 0;
            ProgressText = $"Nhóm: {SelectedGroupName} | Chưa có thẻ nào";
            return;
        }

        DisplayCardNumber = CurrentCardIndex + 1;
        ProgressPercentage = (double)DisplayCardNumber / TotalCardsCount * 100.0;
        ProgressText = $"Nhóm: {SelectedGroupName} | Thẻ {DisplayCardNumber} / {TotalCardsCount}";
    }

    [RelayCommand]
    public async Task LoadFlashcardsAsync()
    {
        _allCards = await _databaseService.GetFlashcardsAsync();
        RefreshGroups(SelectedGroupName);
    }

    [RelayCommand]
    private void FlipCard()
    {
        IsFlipped = !IsFlipped;
    }

    [RelayCommand]
    private async Task RateCardAsync(object? parameter) // 1: Hard, 3: Good, 5: Easy
    {
        if (CurrentCard == null) return;

        int easeScore = 3;
        if (parameter is int i)
        {
            easeScore = i;
        }
        else if (parameter is string s && int.TryParse(s, out var parsed))
        {
            easeScore = parsed;
        }

        CurrentCard.ReviewCount++;
        CurrentCard.EaseLevel = easeScore;

        int daysToAdd = easeScore switch
        {
            1 => 1,
            3 => 3,
            5 => 7,
            _ => 2
        };

        CurrentCard.NextReviewDate = DateTime.UtcNow.AddDays(daysToAdd);
        await _databaseService.SaveFlashcardAsync(CurrentCard);

        NextCard();
    }

    [RelayCommand]
    private void NextCard()
    {
        if (Flashcards.Count == 0) return;

        CurrentCardIndex = (CurrentCardIndex + 1) % Flashcards.Count;
        CurrentCard = Flashcards[CurrentCardIndex];
        IsFlipped = false;
        UpdateProgress();
    }

    [RelayCommand]
    private void PreviousCard()
    {
        if (Flashcards.Count == 0) return;

        CurrentCardIndex = (CurrentCardIndex - 1 + Flashcards.Count) % Flashcards.Count;
        CurrentCard = Flashcards[CurrentCardIndex];
        IsFlipped = false;
        UpdateProgress();
    }

    [RelayCommand]
    private async Task AddCardAsync()
    {
        if (string.IsNullOrWhiteSpace(NewFront) || string.IsNullOrWhiteSpace(NewBack))
            return;

        var subject = string.IsNullOrWhiteSpace(NewSubject) ? "Chung" : NewSubject.Trim();
        var card = new Flashcard
        {
            Front = NewFront.Trim(),
            Back = NewBack.Trim(),
            SubjectName = subject,
            NextReviewDate = DateTime.UtcNow
        };

        await _databaseService.SaveFlashcardAsync(card);
        _allCards.Add(card);

        NewFront = string.Empty;
        NewBack = string.Empty;

        RefreshGroups(subject);
    }

    [RelayCommand]
    private async Task DeleteCardAsync(Flashcard card)
    {
        await _databaseService.DeleteFlashcardAsync(card.Id);
        _allCards.Remove(card);

        RefreshGroups(SelectedGroupName);
    }

    [RelayCommand]
    private async Task GenerateCardsWithAiAsync()
    {
        if (string.IsNullOrWhiteSpace(AiGenerateTopic) || IsGeneratingCards) return;

        IsGeneratingCards = true;
        try
        {
            var prompt = $"""
                Hãy tạo 4 thẻ ghi nhớ (Flashcard) về chủ đề: "{AiGenerateTopic}".
                Định dạng trả về duy nhất là danh sách các dòng theo mẫu:
                Mặt trước | Mặt sau
                Mặt trước | Mặt sau
                Không thêm lời mở đầu hay kết luận.
                """;

            var history = new[] { new ChatMessage { Role = "user", Content = prompt } };
            var response = await _aiCoordinator.SendMessageAsync(history);

            var topic = AiGenerateTopic.Trim();
            var lines = response.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var parts = line.Split('|');
                if (parts.Length == 2)
                {
                    var front = parts[0].Trim().TrimStart('-', '*', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', ' ');
                    var back = parts[1].Trim();

                    if (!string.IsNullOrWhiteSpace(front) && !string.IsNullOrWhiteSpace(back))
                    {
                        var card = new Flashcard
                        {
                            Front = front,
                            Back = back,
                            SubjectName = topic,
                            NextReviewDate = DateTime.UtcNow
                        };
                        await _databaseService.SaveFlashcardAsync(card);
                        _allCards.Add(card);
                    }
                }
            }

            AiGenerateTopic = string.Empty;
            RefreshGroups(topic);
        }
        catch
        {
            // Handled gracefully
        }
        finally
        {
            IsGeneratingCards = false;
        }
    }
}
