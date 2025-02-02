#region ========================================================================= USING =====================================================================================
using Lumina.Infrastructure.Core.Authentication;
using OtpNet;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Authentication;

/// <summary>
/// Contains unit tests for the <see cref="TotpTokenGenerator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TotpTokenGeneratorTests
{
    private readonly TotpTokenGenerator _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="TotpTokenGeneratorTests"/> class.
    /// </summary>
    public TotpTokenGeneratorTests()
    {
        _sut = new TotpTokenGenerator();
    }

    [Fact]
    public void GenerateSecret_WhenCalled_ShouldGenerateNonEmptySecret()
    {
        // Act
        byte[] secret = _sut.GenerateSecret();

        // Assert
        Assert.NotEmpty(secret);
    }

    [Fact]
    public void GenerateSecret_WhenCalledMultipleTimes_ShouldGenerateUniqueSecrets()
    {
        // Act
        byte[] secret1 = _sut.GenerateSecret();
        byte[] secret2 = _sut.GenerateSecret();
        byte[] secret3 = _sut.GenerateSecret();

        // Assert
        Assert.NotEqual(secret1, secret2);
        Assert.NotEqual(secret1, secret3);
        Assert.NotEqual(secret2, secret3);
    }

    [Fact]
    public void ValidateToken_WhenTokenIsValid_ShouldReturnTrue()
    {
        // Arrange
        byte[] secret = _sut.GenerateSecret();
        Totp totp = new(secret);
        string validToken = totp.ComputeTotp();

        // Act
        bool result = _sut.ValidateToken(secret, validToken);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("123456")] // Valid format but wrong token
    [InlineData("abcdef")] // Invalid format
    [InlineData("12345")] // Too short
    [InlineData("1234567")] // Too long
    public void ValidateToken_WhenTokenIsInvalid_ShouldReturnFalse(string invalidToken)
    {
        // Arrange
        byte[] secret = _sut.GenerateSecret();

        // Act
        bool result = _sut.ValidateToken(secret, invalidToken);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateToken_WhenSecretIsWrong_ShouldReturnFalse()
    {
        // Arrange
        byte[] secret1 = _sut.GenerateSecret();
        byte[] secret2 = _sut.GenerateSecret();
        Totp totp = new(secret1);
        string token = totp.ComputeTotp();

        // Act
        bool result = _sut.ValidateToken(secret2, token);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GenerateSecret_WhenCalled_ShouldGenerateSecretOfCorrectLength()
    {
        // Act
        byte[] secret = _sut.GenerateSecret();

        // Assert
        Assert.Equal(20, secret.Length); // standard TOTP secret length
    }
}
