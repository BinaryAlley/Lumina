#region ========================================================================= USING =====================================================================================
using Bogus;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Authentication.Fixtures;

/// <summary>
/// Fixture class for QR code generation tests.
/// </summary>
[ExcludeFromCodeCoverage]
public class QRCodeGeneratorFixture
{
    /// <summary>
    /// Creates test data for QR code generation.
    /// </summary>
    /// <returns>A tuple containing username and secret bytes.</returns>
    public static (string Username, byte[] Secret) CreateQrCodeTestData()
    {
        string username = new Faker().Person.UserName;
        byte[] secret = new byte[20]; // Standard TOTP secret length
        new Random().NextBytes(secret);
        return (username, secret);
    }

    /// <summary>
    /// Gets the expected otpauth URI format for validation.
    /// </summary>
    /// <param name="username">The username to use in the URI.</param>
    /// <param name="base32Secret">The Base32 encoded secret.</param>
    /// <returns>The formatted otpauth URI.</returns>
    public static string GetExpectedOtpauthUri(string username, string base32Secret)
    {
        return $"otpauth://totp/Lumina:{Uri.EscapeDataString(username)}?secret={base32Secret}&issuer=Lumina";
    }
}
