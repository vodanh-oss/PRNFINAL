using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartStudyAssistant.Core.Models;
using SmartStudyAssistant.Core.Services.Database;

namespace SmartStudyAssistant.Wpf.ViewModels;

public partial class ProgressViewModel : ObservableObject
{
    private readonly IDatabaseService _databaseService;

    [ObservableProperty]
    private ObservableCollection<SubjectItem> _subjects = new();

    [ObservableProperty]
    private SubjectItem? _selectedSubject;

    [ObservableProperty]
    private ObservableCollection<StudyNote> _notes = new();

    [ObservableProperty]
    private StudyNote? _selectedNote;

    [ObservableProperty]
    private string _newSubjectName = string.Empty;

    [ObservableProperty]
    private string _newSubjectCode = string.Empty;

    [ObservableProperty]
    private string _newTargetGrade = "A";

    [ObservableProperty]
    private string _newChecklistTitle = string.Empty;

    [ObservableProperty]
    private string _newNoteTitle = string.Empty;

    [ObservableProperty]
    private string _newNoteContent = string.Empty;

    public ProgressViewModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task InitializeAsync()
    {
        await LoadSubjectsAsync();
        await LoadNotesAsync();
    }

    [RelayCommand]
    public async Task LoadSubjectsAsync()
    {
        var list = await _databaseService.GetSubjectsAsync();
        Subjects = new ObservableCollection<SubjectItem>(list);
        if (SelectedSubject == null && Subjects.Count > 0)
        {
            SelectedSubject = Subjects[0];
        }
    }

    [RelayCommand]
    public async Task LoadNotesAsync()
    {
        var list = await _databaseService.GetNotesAsync();
        Notes = new ObservableCollection<StudyNote>(list);
        if (SelectedNote == null && Notes.Count > 0)
        {
            SelectedNote = Notes[0];
        }
    }

    [RelayCommand]
    private async Task AddSubjectAsync()
    {
        if (string.IsNullOrWhiteSpace(NewSubjectName)) return;

        var subject = new SubjectItem
        {
            Name = NewSubjectName.Trim(),
            Code = NewSubjectCode.Trim(),
            TargetGrade = NewTargetGrade.Trim(),
            Checklist = new List<ChecklistItem>
            {
                new() { Title = "Đọc đề cương chi tiết" },
                new() { Title = "Xem slide bài giảng chương 1 & 2" },
                new() { Title = "Làm bài tập ôn tập" }
            }
        };

        await _databaseService.SaveSubjectAsync(subject);
        Subjects.Add(subject);
        SelectedSubject = subject;

        NewSubjectName = string.Empty;
        NewSubjectCode = string.Empty;
    }

    [RelayCommand]
    private async Task DeleteSubjectAsync(SubjectItem subject)
    {
        await _databaseService.DeleteSubjectAsync(subject.Id);
        Subjects.Remove(subject);
        if (SelectedSubject == subject)
        {
            SelectedSubject = Subjects.FirstOrDefault();
        }
    }

    [RelayCommand]
    private async Task AddChecklistItemAsync()
    {
        if (SelectedSubject == null || string.IsNullOrWhiteSpace(NewChecklistTitle)) return;

        var item = new ChecklistItem
        {
            Title = NewChecklistTitle.Trim(),
            IsCompleted = false
        };

        SelectedSubject.Checklist.Add(item);
        await _databaseService.SaveSubjectAsync(SelectedSubject);
        OnPropertyChanged(nameof(SelectedSubject));

        NewChecklistTitle = string.Empty;

        // Force refresh UI list
        var idx = Subjects.IndexOf(SelectedSubject);
        if (idx >= 0)
        {
            Subjects[idx] = SelectedSubject;
        }
    }

    [RelayCommand]
    private async Task ToggleChecklistItemAsync(ChecklistItem item)
    {
        if (SelectedSubject == null) return;
        item.IsCompleted = !item.IsCompleted;
        await _databaseService.SaveSubjectAsync(SelectedSubject);

        var idx = Subjects.IndexOf(SelectedSubject);
        if (idx >= 0)
        {
            Subjects[idx] = SelectedSubject;
        }
    }

    [RelayCommand]
    private async Task AddNoteAsync()
    {
        if (string.IsNullOrWhiteSpace(NewNoteTitle)) return;

        var note = new StudyNote
        {
            Title = NewNoteTitle.Trim(),
            Content = NewNoteContent.Trim(),
            SubjectName = SelectedSubject?.Name ?? "Chung"
        };

        await _databaseService.SaveNoteAsync(note);
        Notes.Insert(0, note);
        SelectedNote = note;

        NewNoteTitle = string.Empty;
        NewNoteContent = string.Empty;
    }

    [RelayCommand]
    private async Task DeleteNoteAsync(StudyNote note)
    {
        await _databaseService.DeleteNoteAsync(note.Id);
        Notes.Remove(note);
        if (SelectedNote == note)
        {
            SelectedNote = Notes.FirstOrDefault();
        }
    }
}
