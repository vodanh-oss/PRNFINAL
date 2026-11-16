using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartStudyAssistant.Core.Models;
using SmartStudyAssistant.Core.Services.Ai;
using SmartStudyAssistant.Core.Services.Database;

namespace SmartStudyAssistant.Wpf.ViewModels;

public partial class FlashcardsViewModel : ObservableObject
{
    private readonly IDatabaseService _databaseService;
    private readonly AiServiceCoordinator _aiCoordinator;

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

    public void UpdateProgress()
    {
        TotalCardsCount = Flashcards.Count;
        if (TotalCardsCount == 0)
        {
            DisplayCardNumber = 0;
            ProgressPercentage = 0;
            ProgressText = "Chưa có thẻ nào";
            return;
        }

        DisplayCardNumber = CurrentCardIndex + 1;
        ProgressPercentage = (double)DisplayCardNumber / TotalCardsCount * 100.0;
        ProgressText = $"Thẻ {DisplayCardNumber} / {TotalCardsCount}";
    }

    [RelayCommand]
    public async Task LoadFlashcardsAsync()
    {
        var list = await _databaseService.GetFlashcardsAsync();
        Flashcards = new ObservableCollection<Flashcard>(list);

        if (Flashcards.Count > 0)
        {
            CurrentCardIndex = 0;
            CurrentCard = Flashcards[0];
            IsFlipped = false;
        }
        else
        {
            CurrentCard = null;
        }
        UpdateProgress();
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

        var card = new Flashcard
        {
            Front = NewFront.Trim(),
            Back = NewBack.Trim(),
            SubjectName = string.IsNullOrWhiteSpace(NewSubject) ? "Chung" : NewSubject.Trim(),
            NextReviewDate = DateTime.UtcNow
        };

        await _databaseService.SaveFlashcardAsync(card);
        Flashcards.Add(card);

        NewFront = string.Empty;
        NewBack = string.Empty;

        if (CurrentCard == null)
        {
            CurrentCard = card;
            CurrentCardIndex = 0;
        }
        UpdateProgress();
    }

    [RelayCommand]
    private async Task DeleteCardAsync(Flashcard card)
    {
        await _databaseService.DeleteFlashcardAsync(card.Id);
        Flashcards.Remove(card);
        if (CurrentCard == card)
        {
            if (Flashcards.Count > 0)
            {
                CurrentCardIndex = Math.Min(CurrentCardIndex, Flashcards.Count - 1);
                CurrentCard = Flashcards[CurrentCardIndex];
            }
            else
            {
                CurrentCard = null;
                CurrentCardIndex = 0;
            }
        }
        UpdateProgress();
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
                            SubjectName = AiGenerateTopic,
                            NextReviewDate = DateTime.UtcNow
                        };
                        await _databaseService.SaveFlashcardAsync(card);
                        Flashcards.Add(card);
                    }
                }
            }

            AiGenerateTopic = string.Empty;
            if (CurrentCard == null && Flashcards.Count > 0)
            {
                CurrentCard = Flashcards[0];
            }
        }
        catch
        {
            // Handled
        }
        finally
        {
            IsGeneratingCards = false;
        }
    }
}
