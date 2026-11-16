namespace SmartStudyAssistant.Core.Services.Parsers;

public class DocumentPage
{
    public int PageNumber { get; set; }
    public string Text { get; set; } = string.Empty;
}

public class ParsedDocumentResult
{
    public string FileName { get; set; } = string.Empty;
    public int TotalPages { get; set; }
    public List<DocumentPage> Pages { get; set; } = new();
}

public interface IDocumentParser
{
    bool CanParse(string extension);
    Task<ParsedDocumentResult> ParseAsync(string filePath, CancellationToken ct = default);
}
