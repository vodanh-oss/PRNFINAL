using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SmartStudyAssistant.Core.Services.Security;

public class DpapiSecureStorage : ISecureStorage
{
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("SmartStudyAssistant_Entropy_v1");
    private readonly string _storagePath;
    private readonly Dictionary<string, string> _memoryStore = new();
    private readonly object _lock = new();

    public DpapiSecureStorage(string? customPath = null)
    {
        if (!string.IsNullOrEmpty(customPath))
        {
            _storagePath = customPath;
        }
        else
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var folder = Path.Combine(appData, "SmartStudyAssistant");
            Directory.CreateDirectory(folder);
            _storagePath = Path.Combine(folder, "secrets.dat");
        }

        LoadStore();
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;
        try
        {
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = ProtectedData.Protect(plainBytes, Entropy, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(cipherBytes);
        }
        catch (PlatformNotSupportedException)
        {
            // Fallback for non-windows environments or tests
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
        }
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return string.Empty;
        try
        {
            var cipherBytes = Convert.FromBase64String(cipherText);
            var plainBytes = ProtectedData.Unprotect(cipherBytes, Entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plainBytes);
        }
        catch (Exception)
        {
            // If decryption fails, try standard base64 or return empty
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(cipherText));
            }
            catch
            {
                return string.Empty;
            }
        }
    }

    public void SaveSecret(string key, string secret)
    {
        lock (_lock)
        {
            _memoryStore[key] = Encrypt(secret);
            PersistStore();
        }
    }

    public string? GetSecret(string key)
    {
        lock (_lock)
        {
            if (_memoryStore.TryGetValue(key, out var encrypted))
            {
                var decrypted = Decrypt(encrypted);
                return string.IsNullOrEmpty(decrypted) ? null : decrypted;
            }
            return null;
        }
    }

    public void DeleteSecret(string key)
    {
        lock (_lock)
        {
            if (_memoryStore.Remove(key))
            {
                PersistStore();
            }
        }
    }

    private void LoadStore()
    {
        lock (_lock)
        {
            try
            {
                if (File.Exists(_storagePath))
                {
                    var json = File.ReadAllText(_storagePath);
                    var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    if (dict != null)
                    {
                        foreach (var kvp in dict)
                        {
                            _memoryStore[kvp.Key] = kvp.Value;
                        }
                    }
                }
            }
            catch
            {
                // Ignore load errors and start fresh
            }
        }
    }

    private void PersistStore()
    {
        try
        {
            var json = JsonSerializer.Serialize(_memoryStore, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_storagePath, json);
        }
        catch
        {
            // Silently handle write errors in restricted contexts
        }
    }
}
