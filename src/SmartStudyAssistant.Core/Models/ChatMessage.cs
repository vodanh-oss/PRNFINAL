namespace SmartStudyAssistant.Core.Models;

public class CitationItem
{
    public string DocumentName { get; set; } = string.Empty;
    public int PageOrSlideNumber { get; set; }
    public string Snippet { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }

    public string BadgeText => PageOrSlideNumber > 0 
        ? $"[Slide/Trang {PageOrSlideNumber} - {DocumentName}]"
        : $"[{DocumentName}]";
}

public class ChatMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string SessionId { get; set; } = string.Empty;
    public string Role { get; set; } = "user"; // "user", "assistant", "system"
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public List<CitationItem> Citations { get; set; } = new();
    public string? ProviderUsed { get; set; }
    public string? ModelUsed { get; set; }
}

public class ChatSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Title { get; set; } = "Phiên học mới";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;
    public AiProviderType PreferredProvider { get; set; } = AiProviderType.Gemini;
    public bool IsRagEnabled { get; set; } = true;
}
