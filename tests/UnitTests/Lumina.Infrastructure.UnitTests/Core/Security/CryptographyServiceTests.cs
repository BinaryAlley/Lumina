#region ========================================================================= USING =====================================================================================
using Lumina.Infrastructure.Common.Models.Configuration;
using Lumina.Infrastructure.Core.Security;
using Lumina.Infrastructure.UnitTests.Core.Security.Fixtures;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Security;

/// <summary>
/// Contains unit tests for the <see cref="CryptographyService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CryptographyServiceTests
{
    private readonly CryptographyService _sut;
    private readonly IOptions<EncryptionSettingsModel> _encryptionSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="CryptographyServiceTests"/> class.
    /// </summary>
    public CryptographyServiceTests()
    {
        _encryptionSettings = CryptographyServiceFixture.CreateEncryptionSettings();
        _sut = new CryptographyService(_encryptionSettings);
    }

    [Fact]
    public void Encrypt_WhenCalledWithValidData_ShouldReturnNonEmptyString()
    {
        // Arrange
        string plaintext = CryptographyServiceFixture.CreateTestData();

        // Act
        string result = _sut.Encrypt(plaintext);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.NotEqual(plaintext, result);
    }

    [Fact]
    public void Encrypt_WhenCalledMultipleTimesWithSameInput_ShouldReturnDifferentResults()
    {
        // Arrange
        string plaintext = CryptographyServiceFixture.CreateTestData();

        // Act
        string result1 = _sut.Encrypt(plaintext);
        string result2 = _sut.Encrypt(plaintext);

        // Assert
        Assert.NotEqual(result1, result2);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Encrypt_WhenInputIsNullOrEmpty_ShouldThrowArgumentException(string? invalidInput)
    {
        // Act
        Action act = () => _sut.Encrypt(invalidInput!);

        // Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => _sut.Encrypt(invalidInput!));
        Assert.Equal("plaintext", exception.ParamName);
        Assert.StartsWith("Value cannot be null or empty", exception.Message);
    }

    [Fact]
    public void Decrypt_WhenCalledWithValidData_ShouldReturnOriginalText()
    {
        // Arrange
        string originalText = CryptographyServiceFixture.CreateTestData();
        string encrypted = _sut.Encrypt(originalText);

        // Act
        string decrypted = _sut.Decrypt(encrypted);

        // Assert
        Assert.Equal(originalText, decrypted);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Decrypt_WhenInputIsNullOrEmpty_ShouldThrowArgumentException(string? invalidInput)
    {
        // Act
        Action act = () => _sut.Decrypt(invalidInput!);

        // Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => _sut.Decrypt(invalidInput!));
        Assert.Equal("ciphertext", exception.ParamName);
        Assert.StartsWith("Value cannot be null or empty", exception.Message);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("not-base64-!@#")]
    public void Decrypt_WhenInputIsNotBase64_ShouldThrowFormatException(string invalidInput)
    {
        // Act
        Action act = () => _sut.Decrypt(invalidInput);

        // Assert
        Assert.Throws<FormatException>(() => _sut.Decrypt(invalidInput));
    }

    [Fact]
    public void Decrypt_WhenInputIsTooShort_ShouldThrowArgumentException()
    {
        // Arrange
        string shortCiphertext = Convert.ToBase64String(new byte[8]); // less than required 16 bytes

        // Act
        Action act = () => _sut.Decrypt(shortCiphertext);

        // Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => _sut.Decrypt(shortCiphertext));
        Assert.Equal("Invalid ciphertext", exception.Message);
    }

    [Fact]
    public void Decrypt_WhenInputIsCorrupted_ShouldThrowCryptographicException()
    {
        // Arrange
        string originalText = CryptographyServiceFixture.CreateTestData();
        string encrypted = _sut.Encrypt(originalText);

        // decode the Base64 string, modify one byte, then re-encode
        byte[] encryptedBytes = Convert.FromBase64String(encrypted);
        encryptedBytes[^1] ^= 0xFF; // flip all bits in the last byte
        string corrupted = Convert.ToBase64String(encryptedBytes);

        // Act
        Action act = () => _sut.Decrypt(corrupted);

        // Assert
        Assert.Throws<CryptographicException>(() => _sut.Decrypt(corrupted));
    }

    [Fact]
    public void Dispose_WhenCalled_ShouldClearKey()
    {
        // Arrange
        string plaintext = CryptographyServiceFixture.CreateTestData();
        string encrypted = _sut.Encrypt(plaintext);

        // Act
        _sut.Dispose();

        // Assert
        Assert.Throws<CryptographicException>(() => _sut.Decrypt(encrypted));
    }
}
