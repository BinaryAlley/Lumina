#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Contains unit tests for the <see cref="PlaybackDeliveryId"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaybackDeliveryIdTests
{
    [Fact]
    public void CHARACTER_COUNT_WhenAccessed_ShouldBeThirtyTwo()
    {
        // Act
        int characterCount = PlaybackDeliveryId.CHARACTER_COUNT;

        // Assert
        Assert.Equal(32, characterCount);
    }

    [Fact]
    public void IsValid_WhenValueIsTheNFormatOfAGuid_ShouldReturnTrue()
    {
        // Arrange
        string value = Guid.NewGuid().ToString("N");

        // Act
        bool isValid = PlaybackDeliveryId.IsValid(value);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_WhenValueIsTheLowestPossibleHandle_ShouldReturnTrue()
    {
        // Arrange
        string value = Guid.Empty.ToString("N");

        // Act
        bool isValid = PlaybackDeliveryId.IsValid(value);

        // Assert
        Assert.True(isValid);
    }

    [Theory]
    [InlineData(null)] // an absent handle cannot address a delivery
    [InlineData("")] // an empty handle has the wrong length
    [InlineData("   ")] // a whitespace handle has the wrong length
    [InlineData("                                ")] // a handle made only of whitespace has the right length but is not hexadecimal
    [InlineData("0123456789abcdef0123456789abcde")] // 31 characters, one short of a handle
    [InlineData("0123456789abcdef0123456789abcdef0")] // 33 characters, one over a handle
    [InlineData("0123456789ABCDEF0123456789ABCDEF")] // uppercase hexadecimal is not the canonical lowercase form
    [InlineData("01234567-89ab-cdef-0123-456789abcdef")] // the D format of a Guid carries dashes
    [InlineData("0123456789abcdef0123456789abcdeg")] // the last character is not a hexadecimal digit
    [InlineData("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz")] // no character is a hexadecimal digit
    public void IsValid_WhenValueIsNotACanonicalHandle_ShouldReturnFalse(string? value)
    {
        // Act
        bool isValid = PlaybackDeliveryId.IsValid(value);

        // Assert
        Assert.False(isValid);
    }
}
