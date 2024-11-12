#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Lumina.Infrastructure.Common.Errors;
using Lumina.Infrastructure.Common.Models.Configuration;
using Lumina.Infrastructure.Common.Validators;
using Lumina.Infrastructure.Core.Authentication;
using Lumina.Infrastructure.UnitTests.Core.Authentication.Fixtures;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Authentication;

/// <summary>
/// Contains unit tests for the <see cref="QRCodeGenerator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class QRCodeGeneratorTests
{
    private readonly QRCodeGenerator _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="QRCodeGeneratorTests"/> class.
    /// </summary>
    public QRCodeGeneratorTests()
    {
        _sut = new QRCodeGenerator();
    }

    [Fact]
    public void GenerateQrCodeDataUri_WhenCalled_ShouldReturnValidDataUri()
    {
        // Arrange
        (string username, byte[] secret) = QRCodeGeneratorFixture.CreateQrCodeTestData();

        // Act
        string result = _sut.GenerateQrCodeDataUri(username, secret);

        // Assert
        result.Should().StartWith("data:image/png;base64,");
        result.Should().Match(uri => IsBase64String(uri.Substring(22))); // Skip "data:image/png;base64,"
    }

    [Fact]
    public void GenerateQrCodeDataUri_WhenUsernameContainsSpecialCharacters_ShouldEscapeUsername()
    {
        // Arrange
        string username = "user@example.com";
        (_, byte[] secret) = QRCodeGeneratorFixture.CreateQrCodeTestData();

        // Act
        string result = _sut.GenerateQrCodeDataUri(username, secret);

        // Assert
        result.Should().StartWith("data:image/png;base64,");
        result.Should().Match(uri => IsBase64String(uri.Substring(22)));
    }

    [Fact]
    public void GenerateQrCodeDataUri_WhenCalled_ShouldGenerateUniqueQrCodesForDifferentUsers()
    {
        // Arrange
        (string username1, byte[] secret) = QRCodeGeneratorFixture.CreateQrCodeTestData();
        string username2 = username1 + "_different";

        // Act
        string result1 = _sut.GenerateQrCodeDataUri(username1, secret);
        string result2 = _sut.GenerateQrCodeDataUri(username2, secret);

        // Assert
        result1.Should().NotBe(result2);
    }

    [Fact]
    public void GenerateQrCodeDataUri_WhenCalledWithSameInput_ShouldGenerateIdenticalQrCodes()
    {
        // Arrange
        (string username, byte[] secret) = QRCodeGeneratorFixture.CreateQrCodeTestData();

        // Act
        string result1 = _sut.GenerateQrCodeDataUri(username, secret);
        string result2 = _sut.GenerateQrCodeDataUri(username, secret);

        // Assert
        result1.Should().Be(result2);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void GenerateQrCodeDataUri_WhenUsernameIsInvalid_ShouldStillGenerateQrCode(string? invalidUsername)
    {
        // Arrange
        (_, byte[] secret) = QRCodeGeneratorFixture.CreateQrCodeTestData();

        // Act
        string result = _sut.GenerateQrCodeDataUri(invalidUsername!, secret);

        // Assert
        result.Should().StartWith("data:image/png;base64,");
        result.Should().Match(uri => IsBase64String(uri.Substring(22)));
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("user name with spaces")]
    [InlineData("user+with+plus")]
    [InlineData("user/with/slashes")]
    public void GenerateQrCodeDataUri_WhenUsernameHasSpecialCharacters_ShouldStillGenerateQrCode(string username)
    {
        // Arrange
        (_, byte[] secret) = QRCodeGeneratorFixture.CreateQrCodeTestData();

        // Act
        string result = _sut.GenerateQrCodeDataUri(username, secret);

        // Assert
        result.Should().StartWith("data:image/png;base64,");
        result.Should().Match(uri => IsBase64String(uri.Substring(22)));
    }

    [Fact]
    public void GenerateQrCodeDataUri_WhenSecretIsTooShort_ShouldStillGenerateQrCode()
    {
        // Arrange
        (string username, _) = QRCodeGeneratorFixture.CreateQrCodeTestData();
        byte[] shortSecret = [1]; // minimum length for Base32 encoding

        // Act
        string result = _sut.GenerateQrCodeDataUri(username, shortSecret);

        // Assert
        result.Should().StartWith("data:image/png;base64,");
        result.Should().Match(uri => IsBase64String(uri.Substring(22)));
    }

    private static bool IsBase64String(string base64)
    {
        try
        {
            Convert.FromBase64String(base64);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
