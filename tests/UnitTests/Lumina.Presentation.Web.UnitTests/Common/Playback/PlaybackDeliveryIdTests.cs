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
    [InlineData("0123456789abcdef0123456789abcdef")] // a lowercase hexadecimal handle
    [InlineData("ffffffffffffffffffffffffffffffff")] // an all lowercase hexadecimal handle
    [InlineData("00000000000000000000000000000000")] // an all zero handle
    [InlineData("8f14e45fceea167a5a36dedd4bea2543")] // a random lowercase hexadecimal handle
    public void IsValid_WhenValueIsThirtyTwoLowercaseHexadecimalCharacters_ShouldReturnTrue(string value)
    {
        // Act
        bool result = PlaybackDeliveryId.IsValid(value);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(null)] // an absent handle is not valid
    [InlineData("")] // an empty handle is not valid
    [InlineData("   ")] // a whitespace only handle is not valid
    [InlineData("0123456789abcdef0123456789abcde")] // a handle with 31 characters is too short
    [InlineData("0123456789abcdef0123456789abcdef0")] // a handle with 33 characters is too long
    [InlineData("0123456789ABCDEF0123456789ABCDEF")] // an uppercase handle is not accepted
    [InlineData("01234567-89ab-cdef-0123-456789abcdef")] // a dashed GUID is not accepted
    [InlineData("0123456789abcdef0123456789abcdeg")] // a non hexadecimal handle is not accepted
    [InlineData("0123456789abcdef0123456789abcde ")] // a handle with a trailing space is not accepted
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
