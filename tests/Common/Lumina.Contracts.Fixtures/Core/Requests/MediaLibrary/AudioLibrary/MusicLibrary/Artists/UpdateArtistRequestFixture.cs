#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Fixture class for the <see cref="UpdateArtistRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateArtistRequestFixture
{
    private readonly Faker _faker = new();
    private readonly UpdateArtistAlbumRequestFixture _updateArtistAlbumRequestFixture = new();
    private readonly MediaContributorReferenceDtoFixture _mediaContributorReferenceDtoFixture = new();

    /// <summary>
    /// Creates a random valid request to update an artist.
    /// </summary>
    /// <param name="name">Optional. The name of the artist.</param>
    /// <param name="website">Optional. The website of the artist.</param>
    /// <param name="musicBrainzArtistId">Optional. The MusicBrainz identifier of the artist.</param>
    /// <param name="contributors">Optional. The media contributors that make up the artist.</param>
    /// <param name="albums">Optional. The albums of the artist.</param>
    /// <param name="includeName">Whether the name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeWebsite">Whether the website should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzArtistId">Whether the MusicBrainz artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeContributors">Whether the contributors list should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAlbums">Whether the albums list should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created request to update an artist.</returns>
    public UpdateArtistRequest Create(
        string? name = null,
        string? website = null,
        Guid? musicBrainzArtistId = null,
        List<MediaContributorReferenceDto>? contributors = null,
        List<UpdateArtistAlbumRequest>? albums = null,
        bool includeName = true,
        bool includeWebsite = true,
        bool includeMusicBrainzArtistId = true,
        bool includeContributors = true,
        bool includeAlbums = true)
    {
        return new UpdateArtistRequest(
            includeName ? (name ?? _faker.Name.FullName()) : null,
            includeWebsite ? (website ?? _faker.Internet.Url()) : null,
            includeMusicBrainzArtistId ? (musicBrainzArtistId ?? _faker.Random.Guid()) : null,
            includeContributors ? (contributors ?? _mediaContributorReferenceDtoFixture.CreateMany(_faker.Random.Int(1, 3))) : null,
            includeAlbums ? (albums ?? _updateArtistAlbumRequestFixture.CreateMany(_faker.Random.Int(1, 3))) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="UpdateArtistRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<UpdateArtistRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
