#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Infrastructure.Common.Models.Configuration;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Security.Fixtures;

/// <summary>
/// Fixture class for cryptography service tests.
/// </summary>
[ExcludeFromCodeCoverage]
public class CryptographyServiceFixture
{
    /// <summary>
    /// Creates a valid encryption settings model for testing.
    /// </summary>
    /// <returns>The configured options.</returns>
    public static IOptions<EncryptionSettingsModel> CreateEncryptionSettings()
    {
        // Generate a valid 256-bit (32 byte) key for AES-256
        byte[] key = new byte[32];
        RandomNumberGenerator.Fill(key);
        string base64Key = Convert.ToBase64String(key);

        EncryptionSettingsModel settings = new()
        {
            SecretKey = base64Key
        };

        IOptions<EncryptionSettingsModel> options = Substitute.For<IOptions<EncryptionSettingsModel>>();
        options.Value.Returns(settings);

        return options;
    }

    /// <summary>
    /// Creates test data for encryption/decryption.
    /// </summary>
    /// <returns>A string to be encrypted/decrypted.</returns>
    public static string CreateTestData()
    {
        return new Faker().Lorem.Sentence();
    }
}
