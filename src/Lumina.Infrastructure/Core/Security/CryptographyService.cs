#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Security;
using Lumina.Infrastructure.Common.Models.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Security.Cryptography;
using System.Text;
#endregion

namespace Lumina.Infrastructure.Core.Security;

/// <summary>
/// Service for cryptographic concerns.
/// </summary>
public sealed class CryptographyService : ICryptographyService, IDisposable
{
    private readonly byte[] _key;

    /// <summary>
    /// Initializes a new instance of the <see cref="CryptographyService"/> class.
    /// </summary>
    /// <param name="encryptionSettingsModelOptions">Injected service for retrieving <see cref="EncryptionSettingsModel"/>.</param>
    public CryptographyService(IOptions<EncryptionSettingsModel> encryptionSettingsModelOptions)
    {
        _key = Convert.FromBase64String(encryptionSettingsModelOptions.Value.SecretKey);
    }

    /// <summary>
    /// Encrypts a plaintext string using AES encryption with a randomly generated initialization vector (IV).
    /// </summary>
    /// <param name="plaintext">The plaintext string to encrypt.</param>
    /// <returns>A Base64-encoded string of the combined IV and encrypted ciphertext.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="plaintext"/> is <see langword="null"/> or empty.</exception>
    public string Encrypt(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
            throw new ArgumentException("Value cannot be null or empty", nameof(plaintext));

        // generate a new, random 16-byte IV for each encryption to ensure unique ciphertexts
        byte[] iv = new byte[16];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            rng.GetBytes(iv);

        using Aes aes = Aes.Create();
        aes.Key = _key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using ICryptoTransform encryptor = aes.CreateEncryptor();
        byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        byte[] ciphertext = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);

        // combine IV and ciphertext
        byte[] combined = new byte[iv.Length + ciphertext.Length];
        Buffer.BlockCopy(iv, 0, combined, 0, iv.Length);
        Buffer.BlockCopy(ciphertext, 0, combined, iv.Length, ciphertext.Length);

        return Convert.ToBase64String(combined);
    }

    /// <summary>
    /// Decrypts a Base64-encoded string that contains both the IV and ciphertext.
    /// </summary>
    /// <param name="ciphertext">The Base64-encoded string containing both IV and ciphertext.</param>
    /// <returns>The decrypted plaintext string.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="ciphertext"/> is <see langword="null"/>, empty, or invalid.</exception>
    public string Decrypt(string ciphertext)
    {
        if (string.IsNullOrEmpty(ciphertext))
            throw new ArgumentException("Value cannot be null or empty", nameof(ciphertext));

        // decode the combined data (IV + ciphertext) from Base64
        byte[] combined = Convert.FromBase64String(ciphertext);

        if (combined.Length < 16)
            throw new ArgumentException("Invalid ciphertext");

        // extract IV and ciphertext
        byte[] iv = new byte[16];
        byte[] encryptedData = new byte[combined.Length - 16];
        Buffer.BlockCopy(combined, 0, iv, 0, 16);
        Buffer.BlockCopy(combined, 16, encryptedData, 0, encryptedData.Length);

        using Aes aes = Aes.Create();
        aes.Key = _key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using ICryptoTransform decryptor = aes.CreateDecryptor();
        byte[] decrypted = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
        return Encoding.UTF8.GetString(decrypted);
    }

    /// <summary>
    /// Clears sensitive key data when the service is disposed.
    /// </summary>
    public void Dispose()
    {
        Array.Clear(_key, 0, _key.Length);
    }
}
