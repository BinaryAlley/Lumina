#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Fixture class for the <see cref="ArtistResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ArtistResponseFixture
{
    private readonly Faker _faker = new();
    private readonly MediaContributorReferenceDtoFixture _mediaContributorReferenceDtoFixture = new();
    private readonly AlbumResponseFixture _albumResponseFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="ArtistResponse"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the artist.</param>
    /// <param name="libraryId">Optional. The Id of the media library the artist belongs to.</param>
    /// <param name="name">Optional. The name of the artist.</param>
    /// <param name="website">Optional. The website of the artist.</param>
    /// <param name="musicBrainzArtistId">Optional. The MusicBrainz identifier of the artist.</param>
    /// <param name="contributors">Optional. The list of contributor references of the artist.</param>
    /// <param name="albums">Optional. The list of albums of the artist.</param>
    /// <param name="createdOnUtc">Optional. The date and time when the artist was created.</param>
    /// <param name="updatedOnUtc">Optional. The date and time when the artist was updated.</param>
    /// <param name="includeWebsite">Whether the website should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzArtistId">Whether the MusicBrainz artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeUpdatedOnUtc">Whether the update date should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeContributors">Whether the contributors should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAlbums">Whether the albums should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="ArtistResponse"/>.</returns>
    public ArtistResponse Create(
        Guid? id = null,
        Guid? libraryId = null,
        string? name = null,
        string? website = null,
        Guid? musicBrainzArtistId = null,
        List<MediaContributorReferenceDto>? contributors = null,
        List<AlbumResponse>? albums = null,
        DateTime? createdOnUtc = null,
        DateTime? updatedOnUtc = null,
        bool includeWebsite = true,
        bool includeMusicBrainzArtistId = true,
        bool includeUpdatedOnUtc = true,
        bool includeContributors = true,
        bool includeAlbums = true)
    {
        return new ArtistResponse(
            id ?? Guid.NewGuid(),
            libraryId ?? Guid.NewGuid(),
            name ?? _faker.Name.FullName(),
            includeWebsite ? (website ?? _faker.Internet.Url()) : null,
            includeMusicBrainzArtistId ? (musicBrainzArtistId ?? Guid.NewGuid()) : null,
            includeContributors ? (contributors ?? _mediaContributorReferenceDtoFixture.CreateMany(1)) : null,
            includeAlbums ? (albums ?? _albumResponseFixture.CreateMany(1)) : null,
            createdOnUtc ?? _faker.Date.Past().ToUniversalTime(),
            includeUpdatedOnUtc ? (updatedOnUtc ?? _faker.Date.Recent().ToUniversalTime()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="ArtistResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ArtistResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
