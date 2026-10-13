#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.DTO.MediaContributors;
using Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary;
using Lumina.Presentation.Web.Fixtures.Common.DTO.MediaContributors;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="MusicArtistDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicArtistDtoFixture
{
    private readonly Faker _faker = new();
    private readonly MusicArtistMetadataDtoFixture _musicArtistMetadataDtoFixture = new();
    private readonly MediaContributorReferenceDtoFixture _mediaContributorReferenceDtoFixture = new();
    private readonly AudioRatingDtoFixture _audioRatingDtoFixture = new();
    private readonly MusicAlbumDtoFixture _musicAlbumDtoFixture = new();

    /// <summary>
    /// Creates a new <see cref="MusicArtistDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional. The Id of the artist.</param>
    /// <param name="libraryId">Optional. The Id of the media library this artist belongs to.</param>
    /// <param name="coverPath">Optional. The path of the image representing the artist.</param>
    /// <param name="metadata">Optional. The metadata of the artist.</param>
    /// <param name="website">Optional. The website of the artist.</param>
    /// <param name="musicBrainzArtistId">Optional. The MusicBrainz identifier of the artist.</param>
    /// <param name="ipis">Optional. The IPI codes of the artist.</param>
    /// <param name="isnis">Optional. The ISNI codes of the artist.</param>
    /// <param name="createdOnUtc">Optional. The date and time when the artist was created.</param>
    /// <param name="updatedOnUtc">Optional. The date and time when the artist was updated.</param>
    /// <param name="contributors">Optional. The references to the media contributors that make up the artist.</param>
    /// <param name="ratings">Optional. The ratings of the artist.</param>
    /// <param name="albums">Optional. The albums of the artist.</param>
    /// <param name="includeCoverPath">Whether the cover path should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMetadata">Whether the metadata should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeWebsite">Whether the website should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzArtistId">Whether the MusicBrainz artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeIpis">Whether the IPI codes should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeIsnis">Whether the ISNI codes should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeUpdatedOnUtc">Whether the update date should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeContributors">Whether the contributors should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeRatings">Whether the ratings should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAlbums">Whether the albums should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="MusicArtistDto"/> instance.</returns>
    public MusicArtistDto Create(
        Guid? id = null,
        Guid? libraryId = null,
        string? coverPath = null,
        MusicArtistMetadataDto? metadata = null,
        string? website = null,
        Guid? musicBrainzArtistId = null,
        List<string>? ipis = null,
        List<string>? isnis = null,
        DateTime? createdOnUtc = null,
        DateTime? updatedOnUtc = null,
        List<MediaContributorReferenceDto>? contributors = null,
        List<AudioRatingDto>? ratings = null,
        List<MusicAlbumDto>? albums = null,
        bool includeCoverPath = true,
        bool includeMetadata = true,
        bool includeWebsite = true,
        bool includeMusicBrainzArtistId = true,
        bool includeIpis = true,
        bool includeIsnis = true,
        bool includeUpdatedOnUtc = true,
        bool includeContributors = true,
        bool includeRatings = true,
        bool includeAlbums = true)
    {
        return new MusicArtistDto
        {
            Id = id ?? _faker.Random.Guid(),
            LibraryId = libraryId ?? _faker.Random.Guid(),
            CoverPath = includeCoverPath ? (coverPath ?? _faker.System.FilePath()) : null,
            Metadata = includeMetadata ? (metadata ?? _musicArtistMetadataDtoFixture.Create()) : null,
            Website = includeWebsite ? (website ?? _faker.Internet.Url()) : null,
            MusicBrainzArtistId = includeMusicBrainzArtistId ? (musicBrainzArtistId ?? _faker.Random.Guid()) : null,
            Ipis = includeIpis ? (ipis ?? [.. Enumerable.Range(0, _faker.Random.Number(1, 3)).Select(_ => _faker.Random.String2(10, "0123456789"))]) : null,
            Isnis = includeIsnis ? (isnis ?? [.. Enumerable.Range(0, _faker.Random.Number(1, 3)).Select(_ => _faker.Random.String2(16, "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"))]) : null,
            CreatedOnUtc = createdOnUtc ?? _faker.Date.Past(),
            UpdatedOnUtc = includeUpdatedOnUtc ? (updatedOnUtc ?? _faker.Date.Recent()) : null,
            Contributors = includeContributors ? (contributors ?? [.. _mediaContributorReferenceDtoFixture.CreateMany(_faker.Random.Number(1, 3))]) : null,
            Ratings = includeRatings ? (ratings ?? [.. _audioRatingDtoFixture.CreateMany(_faker.Random.Number(1, 3))]) : null,
            Albums = includeAlbums ? (albums ?? [.. _musicAlbumDtoFixture.CreateMany(_faker.Random.Number(1, 3))]) : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicArtistDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicArtistDto"/> instances.</returns>
    public List<MusicArtistDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
