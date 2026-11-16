namespace SmartStudyAssistant.Core.Models;

public class Flashcard
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string SubjectName { get; set; } = "Chung";
    public string Front { get; set; } = string.Empty;
    public string Back { get; set; } = string.Empty;
    public int EaseLevel { get; set; } = 3; // 1: Khó (1 ngày), 3: Tốt (3 ngày), 5: Dễ (7 ngày)
    public int ReviewCount { get; set; }
    public DateTime NextReviewDate { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDueForReview => DateTime.UtcNow >= NextReviewDate;
}
