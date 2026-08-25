#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Configuration;
using Lumina.Presentation.Web.Common.Security;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Common.Security;

/// <summary>
/// Contains unit tests for the <see cref="CryptographyService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CryptographyServiceTests
{
    private const string TEST_ENCRYPTION_KEY = "FLYO0QRo6u2VzoFOgNkkEwYNGtqhJ3QGZd7iAHNEJeM=";
    private readonly EncryptionSettingsDtoFixture _encryptionSettingsDtoFixture = new();
    private readonly CryptographyService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="CryptographyServiceTests"/> class.
    /// </summary>
    public CryptographyServiceTests()
    {
        EncryptionSettingsDto settings = _encryptionSettingsDtoFixture.Create(secretKey: TEST_ENCRYPTION_KEY);
        _sut = new CryptographyService(Options.Create(settings));
    }

    [Fact]
    public void Encrypt_WhenCalled_ShouldReturnCiphertextThatDecryptsToPlaintext()
    {
        // Arrange
        const string PLAINTEXT = "sensitive-data";

        // Act
        string ciphertext = _sut.Encrypt(PLAINTEXT);

        // Assert
        Assert.NotEqual(PLAINTEXT, ciphertext);
        Assert.Equal(PLAINTEXT, _sut.Decrypt(ciphertext));
    }

    [Fact]
    public void Encrypt_WhenCalledTwice_ShouldReturnDifferentCiphertexts()
    {
        // Arrange
        const string PLAINTEXT = "sensitive-data";

        // Act
        string firstCiphertext = _sut.Encrypt(PLAINTEXT);
        string secondCiphertext = _sut.Encrypt(PLAINTEXT);

        // Assert
        Assert.NotEqual(firstCiphertext, secondCiphertext);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Encrypt_WhenPlaintextIsNullOrEmpty_ShouldThrowArgumentException(string? plaintext)
    {
        // Act
        ArgumentException exception = Assert.Throws<ArgumentException>(() => _sut.Encrypt(plaintext!));

        // Assert
        Assert.Equal("plaintext", exception.ParamName);
    }

    [Fact]
    public void Decrypt_WhenCiphertextIsNotValidBase64_ShouldThrowFormatException()
    {
        // Act
        FormatException exception = Assert.Throws<FormatException>(() => _sut.Decrypt("not-valid-base64!!!"));

        // Assert
        Assert.Contains("Base-64", exception.Message);
    }

    [Fact]
    public void Decrypt_WhenCiphertextIsTooShort_ShouldThrowArgumentException()
    {
        // Act
        ArgumentException exception = Assert.Throws<ArgumentException>(() => _sut.Decrypt(Convert.ToBase64String([1, 2, 3])));

        // Assert
        Assert.Equal("Invalid ciphertext", exception.Message);
    }
}
