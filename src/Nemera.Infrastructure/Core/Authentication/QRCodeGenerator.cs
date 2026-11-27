#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Authentication;
using OtpNet;
using QRCoder;
using System;
#endregion

namespace Nemera.Infrastructure.Core.Authentication;

/// <summary>
/// Service for generating QR codes for TOTP.
/// </summary>
public class QRCodeGenerator : IQRCodeGenerator
{
    private const string ISSUER = "Nemera";

    /// <summary>
    /// Generates a QR code Data URI for the specified TOTP secret.
    /// </summary>
    /// <param name="username">User's username or unique identifier.</param>
    /// <param name="secret">The TOTP secret for which the QR code should be generated.</param>
    /// <returns>Data URI for the QR code.</returns>
    public string GenerateQrCodeDataUri(string username, byte[] secret)
    {
        // Convert secret to Base32.
        string base32Secret = Base32Encoding.ToString(secret);
        // Build the otpauth URI.
        string otpauthString = $"otpauth://totp/{ISSUER}:{Uri.EscapeDataString(username)}?secret={base32Secret}&issuer={ISSUER}";
        // Generate QR Code.
        QRCoder.QRCodeGenerator qrGenerator = new();
        QRCodeData qrData = qrGenerator.CreateQrCode(otpauthString, QRCoder.QRCodeGenerator.ECCLevel.Q);
        PngByteQRCode qrCode = new(qrData);
        // Convert QR Code to Data URI.
        byte[] pngBytes = qrCode.GetGraphic(20);  // Specify a pixel size if needed.
        string dataUri = "data:image/png;base64," + Convert.ToBase64String(pngBytes);
        return dataUri;
    }
}
