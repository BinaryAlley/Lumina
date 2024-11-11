namespace Lumina.Application.Common.Infrastructure.Security;

/// <summary>
/// Interface for the service for hashing passwords.
/// </summary>
public interface IHashService
{
    /// <summary>
    /// Creates a hash from a string.
    /// </summary>
    /// <param name="password">The string to be hashed.</param>
    /// <returns>The hashed string.</returns>
    string HashString(string password);

    /// <summary>
    /// Verifies a password against a hash.
    /// </summary>
    /// <param name="password">The string to be checked.</param>
    /// <param name="hashedPassword">The hashed representation of the string to be checked.</param>
    /// <returns><see langword="true"/> if <paramref name="password"/> and the de-hashed versions of <paramref name="hashedPassword"/> are equal, <see langword="false"/> otherwise.</returns>
    bool CheckStringAgainstHash(string password, string hashedPassword);
}
