#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="ArtistMetadataDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ArtistMetadataDtoFixture
{
    private const int MINIMUM_LIFESPAN_YEAR = 1900;
    private const int MAXIMUM_LIFESPAN_YEAR = 2026;

    private readonly Faker _faker = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly TagDtoFixture _tagDtoFixture = new();
    private readonly MusicAreaDtoFixture _musicAreaDtoFixture = new();
    private readonly MusicArtistAliasDtoFixture _musicArtistAliasDtoFixture = new();
    private readonly AudioRatingDtoFixture _audioRatingDtoFixture = new();
    private readonly MediaContributorDtoFixture _mediaContributorDtoFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="ArtistMetadataDto"/>.
    /// </summary>
    /// <param name="name">Optional. The name of the artist.</param>
    /// <param name="sortName">Optional. The sort name of the artist.</param>
    /// <param name="disambiguation">Optional. The disambiguation comment of the artist.</param>
    /// <param name="type">Optional. The type of the artist.</param>
    /// <param name="gender">Optional. The gender of the artist.</param>
    /// <param name="country">Optional. The ISO 3166-1 alpha-2 code of the country of the artist.</param>
    /// <param name="area">Optional. The area the artist is primarily identified with.</param>
    /// <param name="beginArea">Optional. The area the artist began in.</param>
    /// <param name="endArea">Optional. The area the artist ended in.</param>
    /// <param name="lifeSpanBegin">Optional. The date the artist started existing.</param>
    /// <param name="lifeSpanEnd">Optional. The date the artist stopped existing.</param>
    /// <param name="isEnded">Optional. Whether the artist no longer exists.</param>
    /// <param name="website">Optional. The website of the artist.</param>
    /// <param name="musicBrainzArtistId">Optional. The MusicBrainz identifier of the artist.</param>
    /// <param name="ipis">Optional. The IPI codes of the artist.</param>
    /// <param name="isnis">Optional. The ISNI codes of the artist.</param>
    /// <param name="aliases">Optional. The alternative names of the artist.</param>
    /// <param name="genres">Optional. The genres associated with the artist.</param>
    /// <param name="tags">Optional. The tags that describe or categorize the artist.</param>
    /// <param name="ratings">Optional. The ratings of the artist.</param>
    /// <param name="contributors">Optional. The media contributors that make up the artist.</param>
    /// <param name="includeName">Whether the name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeSortName">Whether the sort name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeDisambiguation">Whether the disambiguation should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeType">Whether the type should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeGender">Whether the gender should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeCountry">Whether the country should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeArea">Whether the area should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeBeginArea">Whether the begin area should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeEndArea">Whether the end area should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLifeSpanBegin">Whether the lifespan begin should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLifeSpanEnd">Whether the lifespan end should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeWebsite">Whether the website should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzArtistId">Whether the MusicBrainz artist Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeIpis">Whether the IPI codes should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeIsnis">Whether the ISNI codes should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeGenres">Whether the genres should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTags">Whether the tags should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAliases">Whether the aliases should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeRatings">Whether the ratings should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeContributors">Whether the contributors should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="ArtistMetadataDto"/>.</returns>
    public ArtistMetadataDto Create(
        string? name = null,
        string? sortName = null,
        string? disambiguation = null,
        MusicArtistType? type = null,
        MusicArtistGender? gender = null,
        string? country = null,
        MusicAreaDto? area = null,
        MusicAreaDto? beginArea = null,
        MusicAreaDto? endArea = null,
        DateOnly? lifeSpanBegin = null,
        DateOnly? lifeSpanEnd = null,
        bool? isEnded = null,
        string? website = null,
        Guid? musicBrainzArtistId = null,
        List<string>? ipis = null,
        List<string>? isnis = null,
        List<MusicArtistAliasDto>? aliases = null,
        List<GenreDto>? genres = null,
        List<TagDto>? tags = null,
        List<AudioRatingDto>? ratings = null,
        List<MediaContributorDto>? contributors = null,
        bool includeName = true,
        bool includeSortName = true,
        bool includeDisambiguation = true,
        bool includeType = true,
        bool includeGender = true,
        bool includeCountry = true,
        bool includeArea = true,
        bool includeBeginArea = true,
        bool includeEndArea = true,
        bool includeLifeSpanBegin = true,
        bool includeLifeSpanEnd = true,
        bool includeWebsite = true,
        bool includeMusicBrainzArtistId = true,
        bool includeIpis = true,
        bool includeIsnis = true,
        bool includeGenres = true,
        bool includeTags = true,
        bool includeAliases = true,
        bool includeRatings = true,
        bool includeContributors = true)
    {
        int lifeSpanBeginYear = Random.Shared.Next(MINIMUM_LIFESPAN_YEAR, MAXIMUM_LIFESPAN_YEAR);
        int lifeSpanEndYear = Random.Shared.Next(lifeSpanBeginYear, Math.Max(lifeSpanBeginYear + 1, MAXIMUM_LIFESPAN_YEAR));
        return new ArtistMetadataDto(
            includeName ? (name ?? _faker.Name.FullName()) : null,
            includeSortName ? (sortName ?? _faker.Name.FullName()) : null,
            includeDisambiguation ? (disambiguation ?? _faker.Lorem.Sentence()) : null,
            includeType ? (type ?? _faker.PickRandom<MusicArtistType>()) : null,
            includeGender ? (gender ?? _faker.PickRandom<MusicArtistGender>()) : null,
            includeCountry ? (country ?? _faker.Address.CountryCode()) : null,
            includeArea ? (area ?? _musicAreaDtoFixture.Create()) : null,
            includeBeginArea ? (beginArea ?? _musicAreaDtoFixture.Create()) : null,
            includeEndArea ? (endArea ?? _musicAreaDtoFixture.Create()) : null,
            includeLifeSpanBegin ? (lifeSpanBegin ?? new DateOnly(lifeSpanBeginYear, 1, 1)) : null,
            includeLifeSpanEnd ? (lifeSpanEnd ?? new DateOnly(lifeSpanEndYear, 1, 1)) : null,
            isEnded ?? _faker.Random.Bool(),
            includeWebsite ? (website ?? _faker.Internet.Url()) : null,
            includeMusicBrainzArtistId ? (musicBrainzArtistId ?? _faker.Random.Guid()) : null,
            includeIpis ? (ipis ?? [.. Enumerable.Range(0, _faker.Random.Int(1, 2)).Select(_ => _faker.Random.AlphaNumeric(9))]) : null,
            includeIsnis ? (isnis ?? [.. Enumerable.Range(0, _faker.Random.Int(1, 2)).Select(_ => _faker.Random.AlphaNumeric(16))]) : null,
            includeAliases ? (aliases ?? _musicArtistAliasDtoFixture.CreateMany(_faker.Random.Int(1, 3))) : null,
            includeGenres ? (genres ?? _genreDtoFixture.CreateMany(_faker.Random.Int(1, 3))) : null,
            includeTags ? (tags ?? _tagDtoFixture.CreateMany(_faker.Random.Int(1, 3))) : null,
            includeRatings ? (ratings ?? _audioRatingDtoFixture.CreateMany(_faker.Random.Int(1, 3))) : null,
            includeContributors ? (contributors ?? _mediaContributorDtoFixture.CreateMany(_faker.Random.Int(1, 3))) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="ArtistMetadataDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ArtistMetadataDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
