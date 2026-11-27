#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.AddAlbum;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.AddArtist;
using Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Commands.AddAlbum;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.AddArtist;

/// <summary>
/// Fixture class for the <see cref="AddArtistCommand"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddArtistCommandFixture
{
    private readonly Faker _faker = new();
    private readonly MusicArtistMetadataDtoFixture _musicArtistMetadataDtoFixture = new();
    private readonly AddAlbumCommandFixture _addAlbumCommandFixture = new();
    private readonly MediaContributorReferenceDtoFixture _mediaContributorReferenceDtoFixture = new();
    private readonly AudioRatingDtoFixture _audioRatingDtoFixture = new();

    /// <summary>
    /// Creates a random valid command to add an artist.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library the artist belongs to, taken from the route.</param>
    /// <param name="metadata">Optional. The metadata of the artist.</param>
    /// <param name="website">Optional. The website of the artist.</param>
    /// <param name="musicBrainzArtistId">Optional. The MusicBrainz identifier of the artist.</param>
    /// <param name="ipis">Optional. The IPI codes of the artist.</param>
    /// <param name="isnis">Optional. The ISNI codes of the artist.</param>
    /// <param name="contributors">Optional. The media contributors that make up the artist.</param>
    /// <param name="ratings">Optional. The ratings of the artist.</param>
    /// <param name="albums">Optional. The albums of the artist, each with its own tracks.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMetadata">Whether the metadata should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeWebsite">Whether the website should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzArtistId">Whether the MusicBrainz identifier should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeIpis">Whether the IPI codes should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeIsnis">Whether the ISNI codes should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeContributors">Whether the contributors should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeRatings">Whether the ratings should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAlbums">Whether the albums should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created command to add an artist.</returns>
    public AddArtistCommand Create(
        string? libraryId = null,
        MusicArtistMetadataDto? metadata = null,
        string? website = null,
        Guid? musicBrainzArtistId = null,
        List<string>? ipis = null,
        List<string>? isnis = null,
        List<MediaContributorReferenceDto>? contributors = null,
        List<AudioRatingDto>? ratings = null,
        List<AddAlbumCommand>? albums = null,
        bool includeLibraryId = true,
        bool includeMetadata = true,
        bool includeWebsite = true,
        bool includeMusicBrainzArtistId = true,
        bool includeIpis = true,
        bool includeIsnis = true,
        bool includeContributors = true,
        bool includeRatings = true,
        bool includeAlbums = true)
    {
        string? resolvedLibraryId = includeLibraryId ? (libraryId ?? Guid.NewGuid().ToString()) : null;

        return new AddArtistCommand(
            resolvedLibraryId,
            includeMetadata ? (metadata ?? _musicArtistMetadataDtoFixture.Create()) : null,
            includeWebsite ? (website ?? _faker.Internet.Url()) : null,
            includeMusicBrainzArtistId ? (musicBrainzArtistId ?? _faker.Random.Guid()) : null,
            includeIpis ? (ipis ?? [.. Enumerable.Range(0, _faker.Random.Int(1, 2)).Select(_ => _faker.Random.AlphaNumeric(9))]) : null,
            includeIsnis ? (isnis ?? [.. Enumerable.Range(0, _faker.Random.Int(1, 2)).Select(_ => _faker.Random.AlphaNumeric(16))]) : null,
            includeContributors ? (contributors ?? [.. _mediaContributorReferenceDtoFixture.CreateMany(_faker.Random.Int(1, 3))]) : null,
            includeRatings ? (ratings ?? [.. _audioRatingDtoFixture.CreateMany(_faker.Random.Int(1, 3))]) : null,
            includeAlbums ? (albums ?? GenerateAlbums(resolvedLibraryId)) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="AddArtistCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<AddArtistCommand> CreateMany(int count = 3)
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
