using FluentAssertions;
using SmartStudyAssistant.Core.Services.Database;
using Xunit;

namespace SmartStudyAssistant.Tests;

public class DatabaseServiceTests : IDisposable
{
    private readonly string _testDbPath;

    public DatabaseServiceTests()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_study_db_{Guid.NewGuid():N}.db");
    }

    public void Dispose()
    {
        if (File.Exists(_testDbPath))
        {
            try { File.Delete(_testDbPath); } catch { }
        }
    }

    [Fact]
    public async Task InitializeAsync_ShouldCreateTables_AndSeedSampleData()
    {
        // Arrange
        var db = new SqliteDatabaseService(_testDbPath);

        // Act
        await db.InitializeAsync();

        // Assert
        db.DatabasePath.Should().Be(_testDbPath);
        File.Exists(_testDbPath).Should().BeTrue();

        var docs = await db.GetDocumentsAsync();
        docs.Should().NotBeEmpty();
        docs[0].FileName.Should().Contain("DeCuong");

        var chunks = await db.GetAllSelectedChunksAsync();
        chunks.Should().HaveCount(4);

        var subjects = await db.GetSubjectsAsync();
        subjects.Should().HaveCount(2);

        var cards = await db.GetFlashcardsAsync();
        cards.Should().HaveCount(5);
    }
}
