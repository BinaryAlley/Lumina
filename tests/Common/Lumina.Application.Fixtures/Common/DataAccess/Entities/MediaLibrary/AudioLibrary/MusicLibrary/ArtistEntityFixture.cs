#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="ArtistEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ArtistEntityFixture
{
    private readonly AlbumEntityFixture _albumEntityFixture = new();
    private readonly ArtistContributorEntityFixture _artistContributorEntityFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="ArtistEntity"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the artist.</param>
    /// <param name="libraryId">Optional. The Id of the media library that owns the artist.</param>
    /// <param name="name">Optional. The name of the artist.</param>
    /// <param name="albums">Optional. The albums of the artist.</param>
    /// <param name="includeAlbums">Whether the artist should own a generated album, or no album at all.</param>
    /// <param name="includeContributors">Whether the artist should own a generated media contributor, or none at all.</param>
    /// <param name="includeMetadata">Whether the owned metadata collections of the generated album should be included, or forced to empty collections.</param>
    /// <returns>The created <see cref="ArtistEntity"/>.</returns>
    public ArtistEntity Create(
        Guid? id = null,
        Guid? libraryId = null,
        string? name = null,
        List<AlbumEntity>? albums = null,
        bool includeAlbums = true,
        bool includeContributors = true,
        bool includeMetadata = true)
    {
        Guid resolvedId = id ?? Guid.NewGuid();
        Guid resolvedLibraryId = libraryId ?? Guid.NewGuid();
        List<AlbumEntity> resolvedAlbums = albums ?? (includeAlbums ? [_albumEntityFixture.Create(artistId: resolvedId, libraryId: resolvedLibraryId, includeMetadata: includeMetadata)] : []);

        return new Faker<ArtistEntity>()
            .CustomInstantiator(f => new ArtistEntity
            {
                Id = resolvedId,
                LibraryId = resolvedLibraryId,
                Name = default!,
                CreatedOnUtc = default,
                CreatedBy = default,
                UpdatedBy = null
            })
            .RuleFor(x => x.Name, f => name ?? f.Name.FullName())
            .RuleFor(x => x.Website, f => f.Internet.Url())
            .RuleFor(x => x.MusicBrainzArtistId, f => f.Random.Guid())
            .RuleFor(x => x.Albums, resolvedAlbums)
            .RuleFor(x => x.Contributors, f => includeContributors ? _artistContributorEntityFixture.CreateMany(f.Random.Number(1, 3), artistId: resolvedId) : [])
            .RuleFor(x => x.CreatedOnUtc, f => f.Date.Past())
            .RuleFor(x => x.UpdatedOnUtc, f => f.Date.Recent())
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="ArtistEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ArtistEntity"/> instances.</returns>
    public List<ArtistEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
