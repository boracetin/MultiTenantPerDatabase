using System.Security.Cryptography;
using System.Text;

namespace MultitenantPerDb.Shared.Kernel.Infrastructure.Security;

/// <summary>
/// AES encryption service for sensitive data in JWT claims
/// Used to encrypt TenantId in JWT to prevent users from reading/modifying it
/// </summary>
public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}

public class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public AesEncryptionService(IConfiguration configuration)
    {
        // Get encryption key from configuration (should be in secrets/environment variables in production)
        var encryptionKey = configuration["Encryption:Key"] ?? "ThisIsAVerySecureKey123456789012"; // 32 chars for AES-256
        var encryptionIV = configuration["Encryption:IV"] ?? "ThisIsSecureIV16"; // 16 chars for AES

        _key = Encoding.UTF8.GetBytes(encryptionKey.PadRight(32).Substring(0, 32));
        _iv = Encoding.UTF8.GetBytes(encryptionIV.PadRight(16).Substring(0, 16));
    }

    /// <summary>
    /// Encrypts plain text using AES-256
    /// </summary>
    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using var msEncrypt = new MemoryStream();
        using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(plainText);
        }

        var encrypted = msEncrypt.ToArray();
        return Convert.ToBase64String(encrypted);
    }

    /// <summary>
    /// Decrypts cipher text using AES-256
    /// </summary>
    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        try
        {
            var buffer = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using var msDecrypt = new MemoryStream(buffer);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            
            return srDecrypt.ReadToEnd();
        }
        catch (Exception ex)
        {
            throw new CryptographicException("Failed to decrypt TenantId. Token may be tampered or invalid.", ex);
        }
    }
}
