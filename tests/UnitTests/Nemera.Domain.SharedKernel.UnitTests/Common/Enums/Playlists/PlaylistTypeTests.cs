#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.Playlists;

/// <summary>
/// Contains unit tests for the <see cref="PlaylistType"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistTypeTests
{
    [Fact]
    public void PlaylistType_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        PlaylistType[] values = Enum.GetValues<PlaylistType>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }

    [Fact]
    public void PlaylistType_WhenEnumeratingNames_ShouldHaveTheSingleDocumentedMember()
    {
        // Act
        string[] names = Enum.GetNames<PlaylistType>();

        // Assert
        string[] expectedNames =
        [
            nameof(PlaylistType.Music)
        ];
        Assert.Equal(expectedNames, names);
    }

    [Fact]
    public void PlaylistType_WhenEnumeratingValues_ShouldHaveTheDocumentedUnderlyingValues()
    {
        // Act
        PlaylistType[] values = Enum.GetValues<PlaylistType>();

        // Assert
        PlaylistType[] expectedValues =
        [
            PlaylistType.Music
        ];
        int[] expectedUnderlyingValues = [0];
        Assert.Equal(expectedValues, values);
        Assert.Equal(expectedUnderlyingValues, values.Select(value => (int)value));
    }

    [Fact]
    public void PlaylistType_WhenDefaulted_ShouldBeMusic()
    {
        // Act
        PlaylistType playlistType = default;

        // Assert
        Assert.Equal(PlaylistType.Music, playlistType);
    }
}
