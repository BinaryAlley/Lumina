#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Security;
using Lumina.Infrastructure.Common.Models.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
#endregion

namespace Lumina.Infrastructure.Core.Security;

/// <summary>
/// Provides encryption and decryption services using AES encryption.
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
    /// Encrypts a plaintext string using AES encryption with a randomly generated initialization vector (IV) and appends a SHA-256 checksum.
    /// </summary>
    /// <param name="plaintext">The plaintext string to encrypt.</param>
    /// <returns>A Base64-encoded string containing the IV, encrypted ciphertext, and checksum.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="plaintext"/> is <see langword="null"/> or empty.</exception>
    public string Encrypt(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
            throw new ArgumentException("Value cannot be null or empty", nameof(plaintext));

        // Generate a new, random 16-byte IV for each encryption to ensure unique ciphertexts
        byte[] iv = new byte[16];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            rng.GetBytes(iv);

        using Aes aes = Aes.Create();
        aes.Key = _key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        // Encrypt the plaintext
        using ICryptoTransform encryptor = aes.CreateEncryptor();
        byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        byte[] ciphertext = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);

        // Compute SHA-256 checksum of the plaintext
        byte[] checksum = SHA256.HashData(plaintextBytes);

        // Combine IV, ciphertext, and checksum
        byte[] combined = new byte[iv.Length + ciphertext.Length + checksum.Length];
        Buffer.BlockCopy(iv, 0, combined, 0, iv.Length);
        Buffer.BlockCopy(ciphertext, 0, combined, iv.Length, ciphertext.Length);
        Buffer.BlockCopy(checksum, 0, combined, iv.Length + ciphertext.Length, checksum.Length);

        // Return the combined data as a Base64-encoded string
        return Convert.ToBase64String(combined);
    }

    /// <summary>
    /// Decrypts a Base64-encoded string that contains the IV, encrypted ciphertext, and checksum.
    /// </summary>
    /// <param name="ciphertext">The Base64-encoded string containing IV, encrypted data, and checksum.</param>
    /// <returns>The decrypted plaintext string.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="ciphertext"/> is <see langword="null"/>, empty, or invalid.</exception>
    /// <exception cref="CryptographicException">Thrown when the checksum validation fails, indicating potential data corruption.</exception>
    public string Decrypt(string ciphertext)
    {
        if (string.IsNullOrEmpty(ciphertext))
            throw new ArgumentException("Value cannot be null or empty", nameof(ciphertext));

        // Decode the combined data (IV + ciphertext + checksum) from Base64
        byte[] combined = Convert.FromBase64String(ciphertext);

        // Validate the minimum length (16 bytes for IV + 32 bytes for checksum)
        if (combined.Length < 16 + 32)
            throw new ArgumentException("Invalid ciphertext");

        // Extract IV, ciphertext, and checksum from the combined data
        byte[] iv = new byte[16];
        byte[] checksum = new byte[32];
        byte[] encryptedData = new byte[combined.Length - 16 - 32];

        Buffer.BlockCopy(combined, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(combined, iv.Length, encryptedData, 0, encryptedData.Length);
        Buffer.BlockCopy(combined, iv.Length + encryptedData.Length, checksum, 0, checksum.Length);

        using Aes aes = Aes.Create();
        aes.Key = _key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        // Decrypt the encrypted data
        using ICryptoTransform decryptor = aes.CreateDecryptor();
        byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);

        // Compute the checksum of the decrypted data and validate against the provided checksum
        byte[] computedChecksum = SHA256.HashData(decryptedBytes);

        // If checksums do not match, throw a CryptographicException indicating potential corruption
        if (!computedChecksum.SequenceEqual(checksum))
            throw new CryptographicException("Checksum validation failed. The data may be corrupted.");

        // Return the decrypted plaintext as a UTF-8 string
        return Encoding.UTF8.GetString(decryptedBytes);
    }

    /// <summary>
    /// Clears sensitive data from memory.
    /// </summary>
    public void Dispose()
    {
        Array.Clear(_key, 0, _key.Length);
    }
}
