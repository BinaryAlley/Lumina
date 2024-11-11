namespace Lumina.Application.Common.Infrastructure.Authentication;

/// <summary>
/// Interface for the service for generating QR codes for TOTP.
/// </summary>
public interface IQRCodeGenerator
{
    /// <summary>
    /// Generates a QR code Data URI for the specified TOTP secret.
    /// </summary>
    /// <param name="username">User's username or unique identifier.</param>
    /// <param name="secret">The TOTP secret for which the QR code should be generated.</param>
    /// <returns>Data URI for the QR code.</returns>
    string GenerateQrCodeDataUri(string username, byte[] secret);
}
