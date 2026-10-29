#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.Playback;

/// <summary>
/// Contains unit tests for the <see cref="PlaybackStreamingQuality"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaybackStreamingQualityTests
{
    [Fact]
    public void PlaybackStreamingQuality_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        PlaybackStreamingQuality[] values = Enum.GetValues<PlaybackStreamingQuality>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }

    [Fact]
    public void PlaybackStreamingQuality_WhenEnumeratingNames_ShouldHaveTheFourDocumentedMembersInOrder()
    {
        // Act
        string[] names = Enum.GetNames<PlaybackStreamingQuality>();

        // Assert
        string[] expectedNames =
        [
            nameof(PlaybackStreamingQuality.Auto),
            nameof(PlaybackStreamingQuality.Low),
            nameof(PlaybackStreamingQuality.Medium),
            nameof(PlaybackStreamingQuality.High)
        ];
        Assert.Equal(expectedNames, names);
    }

    [Fact]
    public void PlaybackStreamingQuality_WhenEnumeratingValues_ShouldHaveTheDocumentedUnderlyingValues()
    {
        // Act
        PlaybackStreamingQuality[] values = Enum.GetValues<PlaybackStreamingQuality>();

        // Assert
        PlaybackStreamingQuality[] expectedValues =
        [
            PlaybackStreamingQuality.Auto,
            PlaybackStreamingQuality.Low,
            PlaybackStreamingQuality.Medium,
            PlaybackStreamingQuality.High
        ];
        int[] expectedUnderlyingValues = [0, 1, 2, 3];
        Assert.Equal(expectedValues, values);
        Assert.Equal(expectedUnderlyingValues, values.Select(value => (int)value));
    }

    [Fact]
    public void PlaybackStreamingQuality_WhenDefaulted_ShouldBeAuto()
    {
        // Act
        PlaybackStreamingQuality streamingQuality = default;

        // Assert
        Assert.Equal(PlaybackStreamingQuality.Auto, streamingQuality);
    }
}
