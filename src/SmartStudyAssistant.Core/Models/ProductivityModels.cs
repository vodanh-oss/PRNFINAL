namespace SmartStudyAssistant.Core.Models;

public class Flashcard
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string SubjectName { get; set; } = "Chung";
    public string Front { get; set; } = string.Empty;
    public string Back { get; set; } = string.Empty;
    public string? Hint { get; set; }
    public string Tags { get; set; } = string.Empty;
    public int ReviewCount { get; set; }
    public int EaseLevel { get; set; } = 3; // 1: Hard, 3: Good, 5: Easy
    public DateTime NextReviewDate { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDueForReview => DateTime.UtcNow >= NextReviewDate;
}

public class ChecklistItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? DueDate { get; set; }
}

public class SubjectItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string TargetGrade { get; set; } = "A";
    public DateTime? ExamDate { get; set; }
    public List<ChecklistItem> Checklist { get; set; } = new();

    public double ProgressPercentage
    {
        get
        {
            if (Checklist.Count == 0) return 0;
            return (double)Checklist.Count(c => c.IsCompleted) / Checklist.Count * 100.0;
        }
        set { }
    }
}

public class PomodoroStats
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime SessionDate { get; set; } = DateTime.UtcNow.Date;
    public int CompletedSessions { get; set; }
    public int TotalFocusMinutes { get; set; }
}

public class StudyNote
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string SubjectName { get; set; } = "Chung";
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
