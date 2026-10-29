#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.SharedKernel.UnitTests.Common.Enums.Favorites;

/// <summary>
/// Contains unit tests for the <see cref="FavoriteMediaKind"/> enumeration.
/// </summary>
[ExcludeFromCodeCoverage]
public class FavoriteMediaKindTests
{
    [Fact]
    public void FavoriteMediaKind_WhenEnumeratingValues_ShouldHaveNoDuplicateValues()
    {
        // Act
        FavoriteMediaKind[] values = Enum.GetValues<FavoriteMediaKind>();

        // Assert
        Assert.Equal(values.Length, values.Distinct().Count());
    }

    [Fact]
    public void FavoriteMediaKind_WhenEnumeratingNames_ShouldHaveTheSevenDocumentedMembersInOrder()
    {
        // Act
        string[] names = Enum.GetNames<FavoriteMediaKind>();

        // Assert
        string[] expectedNames =
        [
            nameof(FavoriteMediaKind.None),
            nameof(FavoriteMediaKind.MusicArtist),
            nameof(FavoriteMediaKind.MusicAlbum),
            nameof(FavoriteMediaKind.MusicTrack),
            nameof(FavoriteMediaKind.Book),
            nameof(FavoriteMediaKind.Video), // planned: video libraries are not catalogued yet
            nameof(FavoriteMediaKind.Photo) // planned: photo libraries are not catalogued yet
        ];
        Assert.Equal(expectedNames, names);
    }

    [Fact]
    public void FavoriteMediaKind_WhenEnumeratingValues_ShouldHaveTheDocumentedUnderlyingValues()
    {
        // Act
        FavoriteMediaKind[] values = Enum.GetValues<FavoriteMediaKind>();

        // Assert
        FavoriteMediaKind[] expectedValues =
        [
            FavoriteMediaKind.None,
            FavoriteMediaKind.MusicArtist,
            FavoriteMediaKind.MusicAlbum,
            FavoriteMediaKind.MusicTrack,
            FavoriteMediaKind.Book,
            FavoriteMediaKind.Video, // planned: video libraries are not catalogued yet
            FavoriteMediaKind.Photo // planned: photo libraries are not catalogued yet
        ];
        int[] expectedUnderlyingValues = [0, 1, 2, 3, 4, 5, 6];
        Assert.Equal(expectedValues, values);
        Assert.Equal(expectedUnderlyingValues, values.Select(value => (int)value));
    }

    [Fact]
    public void FavoriteMediaKind_WhenDefaulted_ShouldBeNone()
    {
        // Act
        FavoriteMediaKind mediaKind = default;

        // Assert
        Assert.Equal(FavoriteMediaKind.None, mediaKind);
    }
}
