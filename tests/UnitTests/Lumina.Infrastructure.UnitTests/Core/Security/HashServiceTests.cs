#region ========================================================================= USING =====================================================================================
using Lumina.Infrastructure.Core.Security;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Security;

/// <summary>
/// Contains unit tests for the <see cref="PasswordHashService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class HashServiceTests
{
    private readonly PasswordHashService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="HashServiceTests"/> class.
    /// </summary>
    public HashServiceTests()
    {
        _sut = new PasswordHashService();
    }

    [Fact]
    public void HashString_WhenCalled_ShouldReturnNonEmptyHash()
    {
        // Arrange
        string password = "TestPassword123!";

        // Act
        string result = _sut.HashString(password);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.NotEqual(password, result);
    }

    [Fact]
    public void HashString_WhenCalledMultipleTimesWithSameInput_ShouldReturnDifferentHashes()
    {
        // Arrange
        string password = "TestPassword123!";

        // Act
        string hash1 = _sut.HashString(password);
        string hash2 = _sut.HashString(password);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("short")]
    [InlineData("verylongpasswordwithlotsofcharactersbutnostringformatting")]
    [InlineData("Password123!")]
    [InlineData("@#$%^&*()")]
    public void HashString_WhenCalledWithDifferentInputs_ShouldGenerateValidHashes(string input)
    {
        // Act
        string hash = _sut.HashString(input);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        // verify it's a valid Base64 string
        Assert.NotNull(Convert.FromBase64String(hash));
    }

    [Fact]
    public void CheckStringAgainstHash_WhenPasswordMatches_ShouldReturnTrue()
    {
        // Arrange
        string password = "TestPassword123!";
        string hash = _sut.HashString(password);

        // Act
        bool result = _sut.CheckStringAgainstHash(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CheckStringAgainstHash_WhenPasswordDoesNotMatch_ShouldReturnFalse()
    {
        // Arrange
        string password = "TestPassword123!";
        string wrongPassword = "WrongPassword123!";
        string hash = _sut.HashString(password);

        // Act
        bool result = _sut.CheckStringAgainstHash(wrongPassword, hash);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("dGVzdA==")] // valid base64 but wrong length
    [InlineData("YWJjZGVmZ2hpamtsbW5vcHFyc3R1dnd4eXo=")] // valid base64 but wrong length
    public void CheckStringAgainstHash_WhenHashIsInvalidLength_ShouldReturnFalse(string invalidHash)
    {
        // Arrange
        string password = "TestPassword123!";

        // Act
        bool result = _sut.CheckStringAgainstHash(password, invalidHash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CheckStringAgainstHash_WhenHashIsNotBase64_ShouldThrowFormatException()
    {
        // Arrange
        string password = "TestPassword123!";
        string invalidHash = "not-base64-string";

        // Act
        Action act = () => _sut.CheckStringAgainstHash(password, invalidHash);

        // Assert
        Assert.Throws<FormatException>(() => _sut.CheckStringAgainstHash(password, invalidHash));
    }

    [Theory]
    [InlineData(null)]
    public void HashString_WhenPasswordIsNull_ShouldThrowArgumentNullException(string? nullPassword)
    {
        // Act
        Action act = () => _sut.HashString(nullPassword!);

        // Assert
        Assert.Throws<ArgumentNullException>(() => _sut.HashString(nullPassword!));
    }

    [Theory]
    [InlineData(null)]
    public void CheckStringAgainstHash_WhenPasswordIsNull_ShouldThrowArgumentNullException(string? nullPassword)
    {
        // Arrange
        string hash = _sut.HashString("somepassword");

        // Act
        Action act = () => _sut.CheckStringAgainstHash(nullPassword!, hash);

        // Assert
        Assert.Throws<ArgumentNullException>(() => _sut.CheckStringAgainstHash(nullPassword!, hash));
    }

    [Fact]
    public void CheckStringAgainstHash_WhenHashIsNull_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => _sut.CheckStringAgainstHash("password", null!);

        // Assert
        Assert.Throws<ArgumentNullException>(() => _sut.CheckStringAgainstHash("password", null!));
    }
}
