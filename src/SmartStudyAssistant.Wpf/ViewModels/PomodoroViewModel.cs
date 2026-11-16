using System.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartStudyAssistant.Core.Models;
using SmartStudyAssistant.Core.Services.Database;

namespace SmartStudyAssistant.Wpf.ViewModels;

public enum PomodoroMode
{
    Work,
    ShortBreak,
    LongBreak
}

public partial class PomodoroViewModel : ObservableObject
{
    private readonly IDatabaseService _databaseService;
    private readonly DispatcherTimer _timer;

    private const int WorkDurationMinutes = 25;
    private const int ShortBreakDurationMinutes = 5;
    private const int LongBreakDurationMinutes = 15;

    [ObservableProperty]
    private PomodoroMode _currentMode = PomodoroMode.Work;

    [ObservableProperty]
    private int _secondsRemaining = WorkDurationMinutes * 60;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private int _completedWorkSessionsToday;

    [ObservableProperty]
    private int _totalFocusMinutesToday;

    public string TimeDisplay => $"{SecondsRemaining / 60:D2}:{SecondsRemaining % 60:D2}";

    public double ProgressPercentage
    {
        get
        {
            int total = CurrentMode switch
            {
                PomodoroMode.Work => WorkDurationMinutes * 60,
                PomodoroMode.ShortBreak => ShortBreakDurationMinutes * 60,
                PomodoroMode.LongBreak => LongBreakDurationMinutes * 60,
                _ => WorkDurationMinutes * 60
            };

            return (double)(total - SecondsRemaining) / total * 100.0;
        }
    }

    public string ModeTitle => CurrentMode switch
    {
        PomodoroMode.Work => "🎯 Tập Trung Học Tập",
        PomodoroMode.ShortBreak => "☕ Nghỉ Ngắn",
        PomodoroMode.LongBreak => "🛋️ Nghỉ Dài",
        _ => "Pomodoro"
    };

    public PomodoroViewModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += (s, e) => TimerTick();
    }

    public async Task InitializeAsync()
    {
        var stats = await _databaseService.GetTodayPomodoroStatsAsync();
        CompletedWorkSessionsToday = stats.CompletedSessions;
        TotalFocusMinutesToday = stats.TotalFocusMinutes;
    }

    private void TimerTick()
    {
        if (SecondsRemaining > 0)
        {
            SecondsRemaining--;
            OnPropertyChanged(nameof(TimeDisplay));
            OnPropertyChanged(nameof(ProgressPercentage));
        }
        else
        {
            _timer.Stop();
            IsRunning = false;
            PlayNotificationSound();
            HandleSessionComplete();
        }
    }

    private async void HandleSessionComplete()
    {
        if (CurrentMode == PomodoroMode.Work)
        {
            CompletedWorkSessionsToday++;
            TotalFocusMinutesToday += WorkDurationMinutes;
            await _databaseService.RecordPomodoroSessionAsync(WorkDurationMinutes);

            if (CompletedWorkSessionsToday % 4 == 0)
            {
                SetMode(PomodoroMode.LongBreak);
            }
            else
            {
                SetMode(PomodoroMode.ShortBreak);
            }
        }
        else
        {
            SetMode(PomodoroMode.Work);
        }
    }

    private void PlayNotificationSound()
    {
        try
        {
            SystemSounds.Asterisk.Play();
        }
        catch
        {
            // Ignore sound errors
        }
    }

    [RelayCommand]
    private void StartPause()
    {
        if (IsRunning)
        {
            _timer.Stop();
            IsRunning = false;
        }
        else
        {
            _timer.Start();
            IsRunning = true;
        }
    }

    [RelayCommand]
    private void Reset()
    {
        _timer.Stop();
        IsRunning = false;
        SetMode(CurrentMode);
    }

    [RelayCommand]
    private void SetMode(PomodoroMode mode)
    {
        CurrentMode = mode;
        _timer.Stop();
        IsRunning = false;

        SecondsRemaining = mode switch
        {
            PomodoroMode.Work => WorkDurationMinutes * 60,
            PomodoroMode.ShortBreak => ShortBreakDurationMinutes * 60,
            PomodoroMode.LongBreak => LongBreakDurationMinutes * 60,
            _ => WorkDurationMinutes * 60
        };

        OnPropertyChanged(nameof(TimeDisplay));
        OnPropertyChanged(nameof(ProgressPercentage));
        OnPropertyChanged(nameof(ModeTitle));
    }
}
