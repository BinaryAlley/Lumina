#region ========================================================================= USING =====================================================================================
using Bogus;
using Nemera.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Nemera.Presentation.Web.Common.DTO.Libraries;
using Nemera.Presentation.Web.Fixtures.Common.DTO.Libraries;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Nemera.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="DeletedMusicItemsPageDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeletedMusicItemsPageDtoFixture
{
    private readonly Faker _faker = new();
    private readonly LibraryDtoFixture _libraryDtoFixture = new();
    private readonly DeletedMusicArtistDtoFixture _deletedMusicArtistDtoFixture = new();
    private readonly DeletedMusicAlbumDtoFixture _deletedMusicAlbumDtoFixture = new();
    private readonly DeletedMusicTrackDtoFixture _deletedMusicTrackDtoFixture = new();

    /// <summary>
    /// Creates a new <see cref="DeletedMusicItemsPageDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="library">Optional. The media library whose deleted music items are shown.</param>
    /// <param name="deletedArtists">Optional. The deleted artists of the media library.</param>
    /// <param name="deletedAlbums">Optional. The deleted albums of the media library.</param>
    /// <param name="deletedTracks">Optional. The deleted tracks of the media library.</param>
    /// <returns>The created <see cref="DeletedMusicItemsPageDto"/>.</returns>
    public DeletedMusicItemsPageDto Create(
        LibraryDto? library = null,
        IReadOnlyList<DeletedMusicArtistDto>? deletedArtists = null,
        IReadOnlyList<DeletedMusicAlbumDto>? deletedAlbums = null,
        IReadOnlyList<DeletedMusicTrackDto>? deletedTracks = null)
    {
        return new DeletedMusicItemsPageDto(
            Library: library ?? _libraryDtoFixture.Create(),
            DeletedArtists: deletedArtists ?? _deletedMusicArtistDtoFixture.CreateMany(_faker.Random.Number(1, 3)),
            DeletedAlbums: deletedAlbums ?? _deletedMusicAlbumDtoFixture.CreateMany(_faker.Random.Number(1, 3)),
            DeletedTracks: deletedTracks ?? _deletedMusicTrackDtoFixture.CreateMany(_faker.Random.Number(1, 3))
        );
    }

    /// <summary>
    /// Creates multiple <see cref="DeletedMusicItemsPageDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="DeletedMusicItemsPageDto"/> instances.</returns>
    public List<DeletedMusicItemsPageDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
