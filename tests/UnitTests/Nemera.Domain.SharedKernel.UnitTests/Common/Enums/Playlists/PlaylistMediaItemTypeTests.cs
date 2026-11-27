#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.Playlists;

/// <summary>
/// Contains unit tests for the <see cref="PlaylistMediaItemType"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistMediaItemTypeTests
{
    [Fact]
    public void PlaylistMediaItemType_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        PlaylistMediaItemType[] values = Enum.GetValues<PlaylistMediaItemType>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }

    [Fact]
    public void PlaylistMediaItemType_WhenEnumeratingNames_ShouldHaveTheTwoDocumentedMembersInOrder()
    {
        // Act
        string[] names = Enum.GetNames<PlaylistMediaItemType>();

        // Assert
        string[] expectedNames =
        [
            nameof(PlaylistMediaItemType.MusicTrack),
            nameof(PlaylistMediaItemType.VideoFile)
        ];
        Assert.Equal(expectedNames, names);
    }

    [Fact]
    public void PlaylistMediaItemType_WhenEnumeratingValues_ShouldHaveTheDocumentedUnderlyingValues()
    {
        // Act
        PlaylistMediaItemType[] values = Enum.GetValues<PlaylistMediaItemType>();

        // Assert
        PlaylistMediaItemType[] expectedValues =
        [
            PlaylistMediaItemType.MusicTrack,
            PlaylistMediaItemType.VideoFile
        ];
        int[] expectedUnderlyingValues = [0, 1];
        Assert.Equal(expectedValues, values);
        Assert.Equal(expectedUnderlyingValues, values.Select(value => (int)value));
    }

    [Fact]
    public void PlaylistMediaItemType_WhenDefaulted_ShouldBeMusicTrack()
    {
        // Act
        PlaylistMediaItemType mediaItemType = default;

        // Assert
        Assert.Equal(PlaylistMediaItemType.MusicTrack, mediaItemType);
    }
}
