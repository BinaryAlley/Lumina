#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.AddAlbum;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.UpdateArtist;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.AddAlbum;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.UpdateArtist;

/// <summary>
/// Fixture class for the <see cref="UpdateArtistCommand"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateArtistCommandFixture
{
    private readonly Faker _faker = new();
    private readonly AddAlbumCommandFixture _addAlbumCommandFixture = new();
    private readonly MediaContributorReferenceDtoFixture _mediaContributorReferenceDtoFixture = new();

    /// <summary>
    /// Creates a random valid command to update an artist.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library the artist belongs to, taken from the route.</param>
    /// <param name="artistId">Optional. The Id of the artist, taken from the route.</param>
    /// <param name="name">Optional. The name of the artist.</param>
    /// <param name="website">Optional. The website of the artist.</param>
    /// <param name="musicBrainzArtistId">Optional. The MusicBrainz identifier of the artist.</param>
    /// <param name="contributors">Optional. The media contributors that make up the artist.</param>
    /// <param name="albums">Optional. The albums of the artist, each with its own tracks.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeArtistId">Whether the artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeName">Whether the name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeWebsite">Whether the website should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzArtistId">Whether the MusicBrainz identifier should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeContributors">Whether the contributors should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAlbums">Whether the albums should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created command to update an artist.</returns>
    public UpdateArtistCommand Create(
        string? libraryId = null,
        string? artistId = null,
        string? name = null,
        string? website = null,
        Guid? musicBrainzArtistId = null,
        List<MediaContributorReferenceDto>? contributors = null,
        List<AddAlbumCommand>? albums = null,
        bool includeLibraryId = true,
        bool includeArtistId = true,
        bool includeName = true,
        bool includeWebsite = true,
        bool includeMusicBrainzArtistId = true,
        bool includeContributors = true,
        bool includeAlbums = true)
    {
        string? resolvedLibraryId = includeLibraryId ? (libraryId ?? Guid.NewGuid().ToString()) : null;

        return new UpdateArtistCommand(
            resolvedLibraryId,
            includeArtistId ? (artistId ?? Guid.NewGuid().ToString()) : null,
            includeName ? (name ?? _faker.Name.FullName()) : null,
            includeWebsite ? (website ?? _faker.Internet.Url()) : null,
            includeMusicBrainzArtistId ? (musicBrainzArtistId ?? _faker.Random.Guid()) : null,
            includeContributors ? (contributors ?? [.. _mediaContributorReferenceDtoFixture.CreateMany(_faker.Random.Int(1, 3))]) : null,
            includeAlbums ? (albums ?? GenerateAlbums(resolvedLibraryId)) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="UpdateArtistCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<UpdateArtistCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }

    /// <summary>
    /// Generates the albums of the artist, tied to the artist's library identifier.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the albums belong to.</param>
    /// <returns>The generated albums.</returns>
    private List<AddAlbumCommand> GenerateAlbums(string? libraryId)
    {
        return [.. Enumerable.Range(0, _faker.Random.Int(1, 3)).Select(_ => _addAlbumCommandFixture.Create(libraryId: libraryId))];
    }
}
