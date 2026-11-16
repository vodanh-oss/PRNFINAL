using System.Text;

namespace SmartStudyAssistant.Core.Services.Parsers;

public class TxtDocumentParser : IDocumentParser
{
    public bool CanParse(string extension) => extension.Equals(".txt", StringComparison.OrdinalIgnoreCase);

    public async Task<ParsedDocumentResult> ParseAsync(string filePath, CancellationToken ct = default)
    {
        var result = new ParsedDocumentResult
        {
            FileName = Path.GetFileName(filePath)
        };

        var lines = await File.ReadAllLinesAsync(filePath, ct);
        var pages = new List<DocumentPage>();
        var currentPageText = new StringBuilder();
        int pageNumber = 1;
        int lineCounter = 0;

        foreach (var line in lines)
        {
            ct.ThrowIfCancellationRequested();
            currentPageText.AppendLine(line);
            lineCounter++;

            if (lineCounter >= 50)
            {
                pages.Add(new DocumentPage
                {
                    PageNumber = pageNumber++,
                    Text = currentPageText.ToString().Trim()
                });
                currentPageText.Clear();
                lineCounter = 0;
            }
        }

        if (currentPageText.Length > 0)
        {
            pages.Add(new DocumentPage
            {
                PageNumber = pageNumber,
                Text = currentPageText.ToString().Trim()
            });
        }

        result.TotalPages = Math.Max(1, pages.Count);
        result.Pages = pages;
        return result;
    }
}
