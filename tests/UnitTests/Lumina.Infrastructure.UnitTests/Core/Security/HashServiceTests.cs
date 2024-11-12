#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Infrastructure.Core.Security;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Security;

/// <summary>
/// Contains unit tests for the <see cref="HashService"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class HashServiceTests
{
    private readonly HashService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="HashServiceTests"/> class.
    /// </summary>
    public HashServiceTests()
    {
        _sut = new HashService();
    }

    [Fact]
    public void HashString_WhenCalled_ShouldReturnNonEmptyHash()
    {
        // Arrange
        string password = "TestPassword123!";

        // Act
        string result = _sut.HashString(password);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().NotBe(password);
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
        hash1.Should().NotBe(hash2);
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
        hash.Should().NotBeNullOrEmpty();
        // verify it's a valid Base64 string
        Action decode = () => Convert.FromBase64String(hash);
        decode.Should().NotThrow();
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
        result.Should().BeTrue();
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
        result.Should().BeFalse();
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
        result.Should().BeFalse();
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
        act.Should().Throw<FormatException>();
    }

    [Theory]
    [InlineData(null)]
    public void HashString_WhenPasswordIsNull_ShouldThrowArgumentNullException(string? nullPassword)
    {
        // Act
        Action act = () => _sut.HashString(nullPassword!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
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
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CheckStringAgainstHash_WhenHashIsNull_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => _sut.CheckStringAgainstHash("password", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
