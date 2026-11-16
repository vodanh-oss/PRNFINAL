using System.Text;
using System.Text.RegularExpressions;
using SmartStudyAssistant.Core.Models;

namespace SmartStudyAssistant.Core.Services.Rag;

public interface IRagEngine
{
    List<CitationItem> SearchSimilarChunks(string query, IEnumerable<DocumentChunk> candidateChunks, int topK = 4, float[]? queryEmbedding = null);
    string BuildRagSystemPrompt(IEnumerable<CitationItem> citations);
}

public class VectorRagEngine : IRagEngine
{
    public List<CitationItem> SearchSimilarChunks(string query, IEnumerable<DocumentChunk> candidateChunks, int topK = 4, float[]? queryEmbedding = null)
    {
        var chunkList = candidateChunks.ToList();
        if (chunkList.Count == 0) return new List<CitationItem>();

        var scored = new List<(DocumentChunk Chunk, double Score)>();

        // If query embedding is provided and chunks have embeddings of same dimension
        bool useDenseVector = queryEmbedding != null && queryEmbedding.Length > 0 && chunkList.Any(c => c.Embedding != null);

        if (useDenseVector)
        {
            foreach (var chunk in chunkList)
            {
                if (chunk.Embedding != null && chunk.Embedding.Length == queryEmbedding!.Length)
                {
                    double sim = CosineSimilarity(queryEmbedding, chunk.Embedding);
                    scored.Add((chunk, sim));
                }
                else
                {
                    // Fallback to sparse lexical score for this chunk
                    double lexicalScore = CalculateLexicalScore(query, chunk.TextContent);
                    scored.Add((chunk, lexicalScore * 0.7)); // Slight discount
                }
            }
        }
        else
        {
            // Pure Lexical / TF-IDF Vector Cosine Similarity
            foreach (var chunk in chunkList)
            {
                double score = CalculateLexicalScore(query, chunk.TextContent);
                scored.Add((chunk, score));
            }
        }

        return scored
            .OrderByDescending(s => s.Score)
            .Take(topK)
            .Select(s => new CitationItem
            {
                DocumentName = s.Chunk.DocumentName,
                PageOrSlideNumber = s.Chunk.PageOrSlideNumber,
                Snippet = s.Chunk.TextContent,
                SimilarityScore = s.Score
            })
            .ToList();
    }

    public static double CosineSimilarity(float[] vectorA, float[] vectorB)
    {
        if (vectorA.Length != vectorB.Length || vectorA.Length == 0) return 0.0;

        double dotProduct = 0.0;
        double normA = 0.0;
        double normB = 0.0;

        for (int i = 0; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            normA += vectorA[i] * vectorA[i];
            normB += vectorB[i] * vectorB[i];
        }

        if (normA <= 0.0 || normB <= 0.0) return 0.0;
        return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }

    private static double CalculateLexicalScore(string query, string document)
    {
        var queryTokens = Tokenize(query);
        var docTokens = Tokenize(document);

        if (queryTokens.Count == 0 || docTokens.Count == 0) return 0.0;

        var allTerms = queryTokens.Keys.Union(docTokens.Keys).ToList();
        double dot = 0.0;
        double normQ = 0.0;
        double normD = 0.0;

        foreach (var term in allTerms)
        {
            queryTokens.TryGetValue(term, out int qTf);
            docTokens.TryGetValue(term, out int dTf);

            dot += qTf * dTf;
            normQ += qTf * qTf;
            normD += dTf * dTf;
        }

        if (normQ <= 0.0 || normD <= 0.0) return 0.0;
        return dot / (Math.Sqrt(normQ) * Math.Sqrt(normD));
    }

    private static Dictionary<string, int> Tokenize(string text)
    {
        var tokens = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var words = Regex.Split(text.ToLowerInvariant(), @"\W+");

        foreach (var word in words)
        {
            if (word.Length <= 1) continue;
            tokens[word] = tokens.GetValueOrDefault(word, 0) + 1;
        }

        return tokens;
    }

    public string BuildRagSystemPrompt(IEnumerable<CitationItem> citations)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Bạn là Trợ lý Ôn thi & Học tập Thông minh dành cho Sinh viên (Smart Study Assistant).");
        sb.AppendLine("Dưới đây là các đoạn trích từ tài liệu bài giảng / slide / đề cương của sinh viên được tìm thấy qua hệ thống RAG:");
        sb.AppendLine("--------------------------------------------------");

        int index = 1;
        foreach (var c in citations)
        {
            sb.AppendLine($"[Nguồn {index++}]: {c.BadgeText}");
            sb.AppendLine(c.Snippet);
            sb.AppendLine();
        }

        sb.AppendLine("--------------------------------------------------");
        sb.AppendLine("HƯỚNG DẪN TRẢ LỜI QUAN TRỌNG:");
        sb.AppendLine("1. Ưu tiên giải thích kiến thức dựa trên các nguồn tài liệu bài giảng được cung cấp ở trên.");
        sb.AppendLine("2. Mỗi khi trích dẫn hoặc nhắc đến thông tin từ slide/trang nào, BẮT BUỘC ghi rõ nguồn tham khảo theo cú pháp: [Slide/Trang X - TenFile.pdf] ngay sau câu hoặc luận điểm đó.");
        sb.AppendLine("3. Trình bày khoa học, dễ hiểu, dùng gạch đầu dòng, công thức hoặc code block khi cần.");
        sb.AppendLine("4. Nếu trong tài liệu không có thông tin, hãy nói rõ là tài liệu chưa đề cập và bổ sung kiến thức chuẩn học thuật bên ngoài.");

        return sb.ToString();
    }
}
