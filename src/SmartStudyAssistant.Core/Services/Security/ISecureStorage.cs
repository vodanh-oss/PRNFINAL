namespace SmartStudyAssistant.Core.Services.Security;

public interface ISecureStorage
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
    void SaveSecret(string key, string secret);
    string? GetSecret(string key);
    void DeleteSecret(string key);
}
