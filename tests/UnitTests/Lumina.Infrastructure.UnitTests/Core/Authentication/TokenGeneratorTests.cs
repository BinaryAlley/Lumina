#region ========================================================================= USING =====================================================================================
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
    private readonly TokenGenerator _sut = new();

    [Fact]
    public void GenerateToken_WhenCalled_ShouldGenerateNonEmptyToken()
    {
        // Act
        string token = _sut.GenerateToken();

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void GenerateToken_WhenCalled_ShouldGenerateUrlSafeToken()
    {
        // Act
        string token = _sut.GenerateToken();

        // Assert
        Assert.DoesNotContain("+", token);
        Assert.DoesNotContain("/", token);
        Assert.DoesNotContain("=", token);
    }

    [Fact]
    public void GenerateToken_WhenCalledMultipleTimes_ShouldGenerateUniqueTokens()
    {
        // Act
        string token1 = _sut.GenerateToken();
        string token2 = _sut.GenerateToken();
        string token3 = _sut.GenerateToken();

        // Assert
        Assert.NotEqual(token1, token2);
        Assert.NotEqual(token1, token3);
        Assert.NotEqual(token2, token3);
    }

    [Fact]
    public void GenerateToken_WhenCalled_ShouldGenerateTokenOfCorrectLength()
    {
        // Act
        string token = _sut.GenerateToken();

        // Assert
        // 32 bytes in Base64 without padding should be 43 characters
        Assert.Equal(43, token.Length);
    }
}
