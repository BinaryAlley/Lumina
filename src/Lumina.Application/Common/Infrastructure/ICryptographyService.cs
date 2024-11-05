#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Application.Common.Infrastructure;

/// <summary>
/// Interface for the service for cryptographic concerns.
/// </summary>
public interface ICryptographyService
{
    /// <summary>
    /// Encrypts a plaintext string using AES encryption with a randomly generated initialization vector (IV).
    /// </summary>
    /// <param name="plaintext">The plaintext string to encrypt.</param>
    /// <returns>A Base64-encoded string of the combined IV and encrypted ciphertext.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="plaintext"/> is <see langword="null"/> or empty.</exception>
    string Encrypt(string plaintext);

    /// <summary>
    /// Decrypts a Base64-encoded string that contains both the IV and ciphertext.
    /// </summary>
    /// <param name="ciphertext">The Base64-encoded string containing both IV and ciphertext.</param>
    /// <returns>The decrypted plaintext string.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="ciphertext"/> is <see langword="null"/>, empty, or invalid.</exception>
    string Decrypt(string ciphertext);
}
