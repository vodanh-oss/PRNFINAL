using SmartStudyAssistant.Core.Models;
using SmartStudyAssistant.Core.Services.Parsers;

namespace SmartStudyAssistant.Core.Services.Rag;

public class TextChunkerOptions
{
    public int ChunkSize { get; set; } = 600;
    public int ChunkOverlap { get; set; } = 100;
}

public class TextChunker
{
    private readonly TextChunkerOptions _options;

    public TextChunker(TextChunkerOptions? options = null)
    {
        _options = options ?? new TextChunkerOptions();
    }

    public List<DocumentChunk> CreateChunks(string documentId, ParsedDocumentResult parsedDoc)
    {
        var chunks = new List<DocumentChunk>();
        int globalChunkIndex = 0;

        foreach (var page in parsedDoc.Pages)
        {
            var text = page.Text;
            if (string.IsNullOrWhiteSpace(text)) continue;

            if (text.Length <= _options.ChunkSize)
            {
                chunks.Add(new DocumentChunk
                {
                    DocumentId = documentId,
                    DocumentName = parsedDoc.FileName,
                    ChunkIndex = globalChunkIndex++,
                    PageOrSlideNumber = page.PageNumber,
                    TextContent = text.Trim()
                });
                continue;
            }

            int start = 0;
            while (start < text.Length)
            {
                int length = Math.Min(_options.ChunkSize, text.Length - start);

                // Try to find natural sentence or paragraph break near the end
                if (start + length < text.Length)
                {
                    int lastBreak = FindBestBreak(text, start, length);
                    if (lastBreak > start + _options.ChunkOverlap)
                    {
                        length = lastBreak - start;
                    }
                }

                var chunkText = text.Substring(start, length).Trim();
                if (!string.IsNullOrWhiteSpace(chunkText))
                {
                    chunks.Add(new DocumentChunk
                    {
                        DocumentId = documentId,
                        DocumentName = parsedDoc.FileName,
                        ChunkIndex = globalChunkIndex++,
                        PageOrSlideNumber = page.PageNumber,
                        TextContent = chunkText
                    });
                }

                // Advance by length minus overlap
                int advance = length - _options.ChunkOverlap;
                if (advance <= 0) advance = length; // Avoid infinite loop
                start += advance;
            }
        }

        return chunks;
    }

    private static int FindBestBreak(string text, int start, int length)
    {
        int end = start + length;
        for (int i = end - 1; i >= start + (length / 2); i--)
        {
            char c = text[i];
            if (c == '\n' || c == '.' || c == '?' || c == '!' || c == ';')
            {
                return i + 1;
            }
        }
        return end;
    }
}
