#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.Playback;

/// <summary>
/// Contains unit tests for the <see cref="PlaybackDeliveryMode"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaybackDeliveryModeTests
{
    [Fact]
    public void PlaybackDeliveryMode_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        PlaybackDeliveryMode[] values = Enum.GetValues<PlaybackDeliveryMode>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }

    [Fact]
    public void PlaybackDeliveryMode_WhenEnumeratingNames_ShouldHaveTheThreeDocumentedMembersInOrder()
    {
        // Act
        string[] names = Enum.GetNames<PlaybackDeliveryMode>();

        // Assert
        string[] expectedNames =
        [
            nameof(PlaybackDeliveryMode.DirectPlay),
            nameof(PlaybackDeliveryMode.Remux),
            nameof(PlaybackDeliveryMode.Transcode)
        ];
        Assert.Equal(expectedNames, names);
    }

    [Fact]
    public void PlaybackDeliveryMode_WhenEnumeratingValues_ShouldHaveTheDocumentedUnderlyingValues()
    {
        // Act
        PlaybackDeliveryMode[] values = Enum.GetValues<PlaybackDeliveryMode>();

        // Assert
        PlaybackDeliveryMode[] expectedValues =
        [
            PlaybackDeliveryMode.DirectPlay,
            PlaybackDeliveryMode.Remux,
            PlaybackDeliveryMode.Transcode
        ];
        int[] expectedUnderlyingValues = [0, 1, 2];
        Assert.Equal(expectedValues, values);
        Assert.Equal(expectedUnderlyingValues, values.Select(value => (int)value));
    }

    [Fact]
    public void PlaybackDeliveryMode_WhenDefaulted_ShouldBeDirectPlay()
    {
        // Act
        PlaybackDeliveryMode deliveryMode = default;

        // Assert
        Assert.Equal(PlaybackDeliveryMode.DirectPlay, deliveryMode);
    }
}
