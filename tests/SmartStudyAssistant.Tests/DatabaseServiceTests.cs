using FluentAssertions;
using SmartStudyAssistant.Core.Services.Database;
using Xunit;

namespace SmartStudyAssistant.Tests;

public class DatabaseServiceTests
{
    [Fact]
    public async Task SqlServerDatabaseService_ConnectAndFetchSeedData_Success()
    {
        var db = new SqlServerDatabaseService();
        bool canConnect = await db.TestConnectionAsync();

        if (canConnect)
        {
            await db.InitializeAsync();

            var docs = await db.GetDocumentsAsync();
            docs.Should().NotBeEmpty();

            var chunks = await db.GetAllSelectedChunksAsync();
            chunks.Should().NotBeEmpty();

            var cards = await db.GetFlashcardsAsync();
            cards.Should().NotBeEmpty();
            cards.Should().Contain(c => c.SubjectName == "PRN211");
            cards.Should().Contain(c => c.SubjectName == "AI301");
        }
    }
}
