#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Infrastructure.Core.Authentication;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.Authentication;

/// <summary>
/// Contains unit tests for the <see cref="TokenGenerator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TokenGeneratorTests
{
    private readonly TokenGenerator _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenGeneratorTests"/> class.
    /// </summary>
    public TokenGeneratorTests()
    {
        _sut = new TokenGenerator();
    }

    [Fact]
    public void GenerateToken_WhenCalled_ShouldGenerateNonEmptyToken()
    {
        // Act
        string token = _sut.GenerateToken();

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateToken_WhenCalled_ShouldGenerateUrlSafeToken()
    {
        // Act
        string token = _sut.GenerateToken();

        // Assert
        token.Should().NotContain("+", "Token should not contain '+'");
        token.Should().NotContain("/", "Token should not contain '/'");
        token.Should().NotContain("=", "Token should not contain '='");
    }

    [Fact]
    public void GenerateToken_WhenCalledMultipleTimes_ShouldGenerateUniqueTokens()
    {
        // Act
        string token1 = _sut.GenerateToken();
        string token2 = _sut.GenerateToken();
        string token3 = _sut.GenerateToken();

        // Assert
        token1.Should().NotBe(token2);
        token1.Should().NotBe(token3);
        token2.Should().NotBe(token3);
    }

    [Fact]
    public void GenerateToken_WhenCalled_ShouldGenerateTokenOfCorrectLength()
    {
        // Act
        string token = _sut.GenerateToken();

        // Assert
        // 32 bytes in Base64 without padding should be 43 characters
        token.Length.Should().Be(43);
    }
}
