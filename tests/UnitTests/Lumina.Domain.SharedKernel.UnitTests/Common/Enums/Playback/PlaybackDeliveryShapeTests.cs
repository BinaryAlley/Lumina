#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.Playback;

/// <summary>
/// Contains unit tests for the <see cref="PlaybackDeliveryShape"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaybackDeliveryShapeTests
{
    [Fact]
    public void PlaybackDeliveryShape_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        PlaybackDeliveryShape[] values = Enum.GetValues<PlaybackDeliveryShape>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }

    [Fact]
    public void PlaybackDeliveryShape_WhenEnumeratingNames_ShouldHaveTheTwoDocumentedMembersInOrder()
    {
        // Act
        string[] names = Enum.GetNames<PlaybackDeliveryShape>();

        // Assert
        string[] expectedNames =
        [
            nameof(PlaybackDeliveryShape.ByteRangeSeekable),
            nameof(PlaybackDeliveryShape.TimeAddressedSegments)
        ];
        Assert.Equal(expectedNames, names);
    }

    [Fact]
    public void PlaybackDeliveryShape_WhenEnumeratingValues_ShouldHaveTheDocumentedUnderlyingValues()
    {
        // Act
        PlaybackDeliveryShape[] values = Enum.GetValues<PlaybackDeliveryShape>();

        // Assert
        PlaybackDeliveryShape[] expectedValues =
        [
            PlaybackDeliveryShape.ByteRangeSeekable,
            PlaybackDeliveryShape.TimeAddressedSegments
        ];
        int[] expectedUnderlyingValues = [0, 1];
        Assert.Equal(expectedValues, values);
        Assert.Equal(expectedUnderlyingValues, values.Select(value => (int)value));
    }

    [Fact]
    public void PlaybackDeliveryShape_WhenDefaulted_ShouldBeByteRangeSeekable()
    {
        // Act
        PlaybackDeliveryShape deliveryShape = default;

        // Assert
        Assert.Equal(PlaybackDeliveryShape.ByteRangeSeekable, deliveryShape);
    }
}
