#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Playback;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Common.Playback;

/// <summary>
/// Contains unit tests for the <see cref="PlaybackDeliveryId"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaybackDeliveryIdTests
{
    [Theory]
    [InlineData("0123456789abcdef0123456789abcdef")] // A lowercase hexadecimal handle.
    [InlineData("ffffffffffffffffffffffffffffffff")] // An all lowercase hexadecimal handle.
    [InlineData("00000000000000000000000000000000")] // An all zero handle.
    [InlineData("8f14e45fceea167a5a36dedd4bea2543")] // A random lowercase hexadecimal handle.
    public void IsValid_WhenValueIsThirtyTwoLowercaseHexadecimalCharacters_ShouldReturnTrue(string value)
    {
        // Act
        bool result = PlaybackDeliveryId.IsValid(value);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(null)] // An absent handle is not valid.
    [InlineData("")] // An empty handle is not valid.
    [InlineData("   ")] // A whitespace only handle is not valid.
    [InlineData("0123456789abcdef0123456789abcde")] // A handle with 31 characters is too short.
    [InlineData("0123456789abcdef0123456789abcdef0")] // A handle with 33 characters is too long.
    [InlineData("0123456789ABCDEF0123456789ABCDEF")] // An uppercase handle is not accepted.
    [InlineData("01234567-89ab-cdef-0123-456789abcdef")] // A dashed GUID is not accepted.
    [InlineData("0123456789abcdef0123456789abcdeg")] // A non hexadecimal handle is not accepted.
    [InlineData("0123456789abcdef0123456789abcde ")] // A handle with a trailing space is not accepted.
    public void IsValid_WhenValueDoesNotMatchTheHandleFormat_ShouldReturnFalse(string? value)
    {
        // Act
        bool result = PlaybackDeliveryId.IsValid(value);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CharacterCount_ShouldBeThirtyTwo()
    {
        // Act
        int characterCount = PlaybackDeliveryId.CHARACTER_COUNT;

        // Assert
        Assert.Equal(32, characterCount);
    }
}
