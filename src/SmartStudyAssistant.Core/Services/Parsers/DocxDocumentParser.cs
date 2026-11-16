using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace SmartStudyAssistant.Core.Services.Parsers;

public class DocxDocumentParser : IDocumentParser
{
    public bool CanParse(string extension) => extension.Equals(".docx", StringComparison.OrdinalIgnoreCase);

    public Task<ParsedDocumentResult> ParseAsync(string filePath, CancellationToken ct = default)
    {
        return Task.Run(() =>
        {
            var result = new ParsedDocumentResult
            {
                FileName = Path.GetFileName(filePath)
            };

            using var doc = WordprocessingDocument.Open(filePath, false);
            var body = doc.MainDocumentPart?.Document.Body;
            if (body == null)
            {
                result.TotalPages = 1;
                return result;
            }

            var paragraphs = body.Elements<Paragraph>().ToList();
            var pages = new List<DocumentPage>();
            var currentPageText = new StringBuilder();
            int currentPageNumber = 1;
            int wordCountInPage = 0;

            foreach (var para in paragraphs)
            {
                ct.ThrowIfCancellationRequested();
                var text = para.InnerText?.Trim();
                if (string.IsNullOrEmpty(text)) continue;

                currentPageText.AppendLine(text);
                wordCountInPage += text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

                // Approximate 350-400 words per page or page breaks
                if (wordCountInPage >= 350)
                {
                    pages.Add(new DocumentPage
                    {
                        PageNumber = currentPageNumber++,
                        Text = currentPageText.ToString().Trim()
                    });
                    currentPageText.Clear();
                    wordCountInPage = 0;
                }
            }

            if (currentPageText.Length > 0)
            {
                pages.Add(new DocumentPage
                {
                    PageNumber = currentPageNumber,
                    Text = currentPageText.ToString().Trim()
                });
            }

            result.TotalPages = Math.Max(1, pages.Count);
            result.Pages = pages;
            return result;
        }, ct);
    }
}
