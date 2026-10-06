#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Fixture class for the <see cref="AddArtistRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddArtistRequestFixture
{
    private readonly Faker _faker = new();
    private readonly MusicArtistMetadataDtoFixture _musicArtistMetadataDtoFixture = new();
    private readonly AddAlbumRequestFixture _addAlbumRequestFixture = new();
    private readonly MediaContributorReferenceDtoFixture _mediaContributorReferenceDtoFixture = new();
    private readonly AudioRatingDtoFixture _audioRatingDtoFixture = new();

    /// <summary>
    /// Creates a random valid request to add an artist.
    /// </summary>
    /// <param name="metadata">Optional. The metadata of the artist.</param>
    /// <param name="website">Optional. The website of the artist.</param>
    /// <param name="musicBrainzArtistId">Optional. The MusicBrainz identifier of the artist.</param>
    /// <param name="ipis">Optional. The IPI codes of the artist.</param>
    /// <param name="isnis">Optional. The ISNI codes of the artist.</param>
    /// <param name="contributors">Optional. The media contributors that make up the artist.</param>
    /// <param name="ratings">Optional. The ratings of the artist.</param>
    /// <param name="albums">Optional. The albums of the artist.</param>
    /// <param name="includeMetadata">Whether the metadata should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeWebsite">Whether the website should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzArtistId">Whether the MusicBrainz artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeIpis">Whether the IPI codes should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeIsnis">Whether the ISNI codes should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeContributors">Whether the contributors list should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeRatings">Whether the ratings list should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAlbums">Whether the albums list should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created request to add an artist.</returns>
    public AddArtistRequest Create(
        MusicArtistMetadataDto? metadata = null,
        string? website = null,
        Guid? musicBrainzArtistId = null,
        List<string>? ipis = null,
        List<string>? isnis = null,
        List<MediaContributorReferenceDto>? contributors = null,
        List<AudioRatingDto>? ratings = null,
        List<AddAlbumRequest>? albums = null,
        bool includeMetadata = true,
        bool includeWebsite = true,
        bool includeMusicBrainzArtistId = true,
        bool includeIpis = true,
        bool includeIsnis = true,
        bool includeContributors = true,
        bool includeRatings = true,
        bool includeAlbums = true)
    {
        return new AddArtistRequest(
            includeMetadata ? (metadata ?? _musicArtistMetadataDtoFixture.Create()) : null,
            includeWebsite ? (website ?? _faker.Internet.Url()) : null,
            includeMusicBrainzArtistId ? (musicBrainzArtistId ?? _faker.Random.Guid()) : null,
            includeIpis ? (ipis ?? [.. Enumerable.Range(0, _faker.Random.Int(1, 2)).Select(_ => _faker.Random.AlphaNumeric(9))]) : null,
            includeIsnis ? (isnis ?? [.. Enumerable.Range(0, _faker.Random.Int(1, 2)).Select(_ => _faker.Random.AlphaNumeric(16))]) : null,
            includeContributors ? (contributors ?? _mediaContributorReferenceDtoFixture.CreateMany(_faker.Random.Int(1, 3))) : null,
            includeRatings ? (ratings ?? _audioRatingDtoFixture.CreateMany(_faker.Random.Int(1, 3))) : null,
            includeAlbums ? (albums ?? _addAlbumRequestFixture.CreateMany(_faker.Random.Int(1, 3))) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="AddArtistRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<AddArtistRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
