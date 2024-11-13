#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Authentication;
using OtpNet;
#endregion

namespace Lumina.Infrastructure.Core.Authentication;

/// <summary>
/// Service for generating and validating TOTP tokens.
/// </summary>
public class TotpTokenGenerator : ITotpTokenGenerator
{
    /// <summary>
    /// Generates a new TOTP secret for a user.
    /// </summary>
    /// <returns>The generated TOTP secret.</returns>
    public byte[] GenerateSecret()
    {
        return KeyGeneration.GenerateRandomKey();  // generate a random key for TOTP
    }

    /// <summary>
    /// Validates a TOTP token against the stored secret.
    /// </summary>
    /// <param name="secret">The user's TOTP secret.</param>
    /// <param name="token">The token to validate.</param>
    /// <returns><see langword="true"/> if valid, <see langword="false"/> otherwise.</returns>
    public bool ValidateToken(byte[] secret, string token)
    {
        Totp totp = new(secret);
        // verify the TOTP token, allowing for slight time drifts between server and client
        return totp.VerifyTotp(token, out _, VerificationWindow.RfcSpecifiedNetworkDelay);
    }
}
