using UglyToad.PdfPig;

namespace SmartStudyAssistant.Core.Services.Parsers;

public class PdfDocumentParser : IDocumentParser
{
    public bool CanParse(string extension) => extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase);

    public Task<ParsedDocumentResult> ParseAsync(string filePath, CancellationToken ct = default)
    {
        return Task.Run(() =>
        {
            var result = new ParsedDocumentResult
            {
                FileName = Path.GetFileName(filePath)
            };

            using var pdf = PdfDocument.Open(filePath);
            result.TotalPages = pdf.NumberOfPages;

            for (int i = 1; i <= pdf.NumberOfPages; i++)
            {
                ct.ThrowIfCancellationRequested();
                var page = pdf.GetPage(i);
                var text = page.Text ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(text))
                {
                    result.Pages.Add(new DocumentPage
                    {
                        PageNumber = i,
                        Text = text.Trim()
                    });
                }
            }

            return result;
        }, ct);
    }
}
