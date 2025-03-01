#region ========================================================================= USING =====================================================================================
using Konscious.Security.Cryptography;
using Lumina.Application.Common.Infrastructure.Security;
using System;
using System.Security.Cryptography;
using System.Text;
#endregion

namespace Lumina.Infrastructure.Core.Security;

/// <summary>
/// Service for hashing passwords.
/// </summary>
public sealed class PasswordHashService : IPasswordHashService
{
    // OWASP recommended parameters for Argon2id
    private const int MEMORY_SIZE = 19456; // 19 MiB (19 * 1024 = 19456)
    private const int ITERATIONS = 2;
    private const int DEGREE_OF_PARALLELISM = 1;
    private const int HASH_SIZE = 32; // 256 bits
    private const int SALT_SIZE = 16; // 128 bits

    /// <summary>
    /// Creates a hash from a string.
    /// </summary>
    /// <param name="password">The string to be hashed.</param>
    /// <returns>The hashed string.</returns>
    public string HashString(string password)
    {
        byte[] salt = new byte[SALT_SIZE];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            rng.GetBytes(salt); // generate a random salt

        byte[] hashBytes;
        using (Argon2id hasher = new(Encoding.UTF8.GetBytes(password)))
        {
            hasher.Salt = salt;
            hasher.DegreeOfParallelism = DEGREE_OF_PARALLELISM;
            hasher.MemorySize = MEMORY_SIZE;
            hasher.Iterations = ITERATIONS;
            hashBytes = hasher.GetBytes(HASH_SIZE);
        }
        // concatenate salt and hash and return as a Base64 string
        byte[] hashWithSaltBytes = new byte[SALT_SIZE + HASH_SIZE];
        Buffer.BlockCopy(salt, 0, hashWithSaltBytes, 0, SALT_SIZE);
        Buffer.BlockCopy(hashBytes, 0, hashWithSaltBytes, SALT_SIZE, HASH_SIZE);
        return Convert.ToBase64String(hashWithSaltBytes);
    }

    /// <summary>
    /// Verifies a password against a hash.
    /// </summary>
    /// <param name="password">The string to be checked.</param>
    /// <param name="hashedPassword">The hashed representation of the string to be checked.</param>
    /// <returns><see langword="true"/> if <paramref name="password"/> and the de-hashed versions of <paramref name="hashedPassword"/> are equal, <see langword="false"/> otherwise.</returns>
    public bool CheckStringAgainstHash(string password, string hashedPassword)
    {
        // convert the Base64 string back into a byte array
        byte[] hashWithSaltBytes = Convert.FromBase64String(hashedPassword);
        // validate the length too
        if (hashWithSaltBytes.Length != SALT_SIZE + HASH_SIZE)
            return false;

        byte[] saltBytes = new byte[SALT_SIZE];
        Buffer.BlockCopy(hashWithSaltBytes, 0, saltBytes, 0, SALT_SIZE);

        byte[] hashBytes;
        using (Argon2id hasher = new(Encoding.UTF8.GetBytes(password)))
        {
            hasher.Salt = saltBytes;
            hasher.DegreeOfParallelism = DEGREE_OF_PARALLELISM;
            hasher.MemorySize = MEMORY_SIZE;
            hasher.Iterations = ITERATIONS;
            hashBytes = hasher.GetBytes(HASH_SIZE);
        }
        // get the stored hash from the stored hashed password
        byte[] storedHashBytes = new byte[HASH_SIZE];
        Buffer.BlockCopy(hashWithSaltBytes, SALT_SIZE, storedHashBytes, 0, HASH_SIZE);
        // use constant-time comparison to prevent timing attacks
        return CryptographicOperations.FixedTimeEquals(hashBytes, storedHashBytes);
    }
}
