using FluentAssertions;
using SmartStudyAssistant.Core.Services.Parsers;
using SmartStudyAssistant.Core.Services.Rag;
using Xunit;

namespace SmartStudyAssistant.Tests;

public class TextChunkerTests
{
    [Fact]
    public void CreateChunks_ShouldPreservePageAndSlideNumbers()
    {
        // Arrange
        var chunker = new TextChunker(new TextChunkerOptions { ChunkSize = 100, ChunkOverlap = 20 });
        var parsed = new ParsedDocumentResult
        {
            FileName = "HeDieuHanh_Chuong3.pdf",
            TotalPages = 2,
            Pages = new List<DocumentPage>
            {
                new() { PageNumber = 1, Text = "Slide 1: Giới thiệu hệ điều hành và tiến trình." },
                new() { PageNumber = 2, Text = "Slide 2: Thuật toán lập lịch Round Robin và FIFO." }
            }
        };

        // Act
        var chunks = chunker.CreateChunks("doc-1", parsed);

        // Assert
        chunks.Should().HaveCount(2);
        chunks[0].PageOrSlideNumber.Should().Be(1);
        chunks[0].DocumentName.Should().Be("HeDieuHanh_Chuong3.pdf");
        chunks[0].TextContent.Should().Contain("tiến trình");

        chunks[1].PageOrSlideNumber.Should().Be(2);
        chunks[1].TextContent.Should().Contain("Round Robin");
    }

    [Fact]
    public void CreateChunks_WithLongText_ShouldSplitWithOverlap()
    {
        // Arrange
        var chunker = new TextChunker(new TextChunkerOptions { ChunkSize = 50, ChunkOverlap = 15 });
        var longText = "Đây là một đoạn văn bản rất dài được viết để kiểm tra việc phân tách các đoạn văn bản trong hệ thống RAG.";
        var parsed = new ParsedDocumentResult
        {
            FileName = "LongDoc.txt",
            TotalPages = 1,
            Pages = new List<DocumentPage>
            {
                new() { PageNumber = 1, Text = longText }
            }
        };

        // Act
        var chunks = chunker.CreateChunks("doc-2", parsed);

        // Assert
        chunks.Count.Should().BeGreaterThan(1);
        foreach (var c in chunks)
        {
            c.TextContent.Length.Should().BeLessThanOrEqualTo(65);
            c.PageOrSlideNumber.Should().Be(1);
        }
    }
}
