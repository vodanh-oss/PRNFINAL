using FluentAssertions;
using SmartStudyAssistant.Core.Models;
using SmartStudyAssistant.Core.Services.Rag;
using Xunit;

namespace SmartStudyAssistant.Tests;

public class VectorSimilarityTests
{
    [Fact]
    public void CosineSimilarity_WithIdenticalVectors_ShouldReturnOne()
    {
        // Arrange
        float[] v1 = [1.0f, 2.0f, 3.0f];
        float[] v2 = [1.0f, 2.0f, 3.0f];

        // Act
        double sim = VectorRagEngine.CosineSimilarity(v1, v2);

        // Assert
        sim.Should().BeApproximately(1.0, 0.001);
    }

    [Fact]
    public void CosineSimilarity_WithOrthogonalVectors_ShouldReturnZero()
    {
        // Arrange
        float[] v1 = [1.0f, 0.0f];
        float[] v2 = [0.0f, 1.0f];

        // Act
        double sim = VectorRagEngine.CosineSimilarity(v1, v2);

        // Assert
        sim.Should().BeApproximately(0.0, 0.001);
    }

    [Fact]
    public void SearchSimilarChunks_LexicalFallback_ShouldRankRelevantChunkFirst()
    {
        // Arrange
        var engine = new VectorRagEngine();
        var chunks = new List<DocumentChunk>
        {
            new() { DocumentName = "Slide.pdf", PageOrSlideNumber = 5, TextContent = "Bộ nhớ ảo Virtual Memory và phân trang Paging trong hệ điều hành." },
            new() { DocumentName = "Slide.pdf", PageOrSlideNumber = 12, TextContent = "Đồ họa máy tính và xử lý ảnh đa chiều." },
            new() { DocumentName = "Slide.pdf", PageOrSlideNumber = 20, TextContent = "Giao thức mạng TCP IP và bắt tay 3 bước." }
        };

        // Act
        var results = engine.SearchSimilarChunks("Bộ nhớ ảo Paging", chunks, topK: 1);

        // Assert
        results.Should().HaveCount(1);
        results[0].PageOrSlideNumber.Should().Be(5);
        results[0].Snippet.Should().Contain("Virtual Memory");
    }
}
