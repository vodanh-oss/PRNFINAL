using FluentAssertions;
using SmartStudyAssistant.Core.Services.Productivity;
using Xunit;

namespace SmartStudyAssistant.Tests;

public class PomodoroTimerTests
{
    [Fact]
    public void InitialState_ShouldBeWorkModeWith25Minutes()
    {
        // Arrange
        var sm = new PomodoroStateMachine();

        // Assert
        sm.CurrentMode.Should().Be(PomodoroMode.Work);
        sm.SecondsRemaining.Should().Be(25 * 60);
        sm.TimeDisplay.Should().Be("25:00");
    }

    [Fact]
    public void SetMode_ShortBreak_ShouldResetSecondsTo5Minutes()
    {
        // Arrange
        var sm = new PomodoroStateMachine();

        // Act
        sm.SetMode(PomodoroMode.ShortBreak);

        // Assert
        sm.CurrentMode.Should().Be(PomodoroMode.ShortBreak);
        sm.SecondsRemaining.Should().Be(5 * 60);
        sm.TimeDisplay.Should().Be("05:00");
    }

    [Fact]
    public void WorkSessionCompletion_ShouldIncrementStatsAndSwitchToBreak()
    {
        // Arrange
        var sm = new PomodoroStateMachine();
        sm.SetMode(PomodoroMode.Work);

        // Set remaining to 0 and tick
        while (sm.SecondsRemaining > 0)
        {
            sm.Tick();
        }

        // Act - last tick triggers completion
        bool completed = sm.Tick();

        // Assert
        completed.Should().BeTrue();
        sm.CompletedSessions.Should().Be(1);
        sm.TotalFocusMinutes.Should().Be(25);
        sm.CurrentMode.Should().Be(PomodoroMode.ShortBreak);
    }
}
