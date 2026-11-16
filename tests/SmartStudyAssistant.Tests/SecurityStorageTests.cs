using FluentAssertions;
using SmartStudyAssistant.Core.Services.Security;
using Xunit;

namespace SmartStudyAssistant.Tests;

public class SecurityStorageTests : IDisposable
{
    private readonly string _testPath;

    public SecurityStorageTests()
    {
        _testPath = Path.Combine(Path.GetTempPath(), $"sec_test_{Guid.NewGuid():N}.dat");
    }

    public void Dispose()
    {
        if (File.Exists(_testPath))
        {
            try { File.Delete(_testPath); } catch { }
        }
    }

    [Fact]
    public void EncryptDecrypt_Roundtrip_ShouldRestoreOriginalPlainText()
    {
        // Arrange
        var storage = new DpapiSecureStorage(_testPath);
        var secret = "sk-proj-Gemini12345SecretKeyToken";

        // Act
        var encrypted = storage.Encrypt(secret);
        var decrypted = storage.Decrypt(encrypted);

        // Assert
        encrypted.Should().NotBeNullOrEmpty();
        encrypted.Should().NotBe(secret);
        decrypted.Should().Be(secret);
    }

    [Fact]
    public void SaveAndGetSecret_ShouldPersistAcrossInstances()
    {
        // Arrange
        var storage1 = new DpapiSecureStorage(_testPath);
        storage1.SaveSecret("ClaudeKey", "claude-secret-api-key");

        // Act
        var storage2 = new DpapiSecureStorage(_testPath);
        var retrieved = storage2.GetSecret("ClaudeKey");

        // Assert
        retrieved.Should().Be("claude-secret-api-key");
    }
}
