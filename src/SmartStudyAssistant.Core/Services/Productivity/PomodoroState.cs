namespace SmartStudyAssistant.Core.Services.Productivity;

public enum PomodoroMode
{
    Work,
    ShortBreak,
    LongBreak
}

public class PomodoroStateMachine
{
    public const int WorkDurationMinutes = 25;
    public const int ShortBreakDurationMinutes = 5;
    public const int LongBreakDurationMinutes = 15;

    public PomodoroMode CurrentMode { get; private set; } = PomodoroMode.Work;
    public int SecondsRemaining { get; private set; } = WorkDurationMinutes * 60;
    public int CompletedSessions { get; private set; }
    public int TotalFocusMinutes { get; private set; }

    public string TimeDisplay => $"{SecondsRemaining / 60:D2}:{SecondsRemaining % 60:D2}";

    public void SetMode(PomodoroMode mode)
    {
        CurrentMode = mode;
        SecondsRemaining = mode switch
        {
            PomodoroMode.Work => WorkDurationMinutes * 60,
            PomodoroMode.ShortBreak => ShortBreakDurationMinutes * 60,
            PomodoroMode.LongBreak => LongBreakDurationMinutes * 60,
            _ => WorkDurationMinutes * 60
        };
    }

    public bool Tick()
    {
        if (SecondsRemaining > 0)
        {
            SecondsRemaining--;
            return false;
        }

        // Session completed
        if (CurrentMode == PomodoroMode.Work)
        {
            CompletedSessions++;
            TotalFocusMinutes += WorkDurationMinutes;
            SetMode(CompletedSessions % 4 == 0 ? PomodoroMode.LongBreak : PomodoroMode.ShortBreak);
        }
        else
        {
            SetMode(PomodoroMode.Work);
        }

        return true;
    }

    public void Reset()
    {
        SetMode(CurrentMode);
    }
}
