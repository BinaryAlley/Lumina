namespace Lumina.Application.Common.Infrastructure.Authentication;

/// <summary>
/// Interface for the service for generating and validating TOTP tokens.
/// </summary>
public interface ITotpTokenGenerator
{
    /// <summary>
    /// Generates a new TOTP secret for a user.
    /// </summary>
    /// <returns>The generated TOTP secret.</returns>
    byte[] GenerateSecret();

    /// <summary>
    /// Validates a TOTP token against the stored secret.
    /// </summary>
    /// <param name="secret">The user's TOTP secret.</param>
    /// <param name="token">The token to validate.</param>
    /// <returns><see langword="true"/> if valid, <see langword="false"/> otherwise.</returns>
    bool ValidateToken(byte[] secret, string token);
}
