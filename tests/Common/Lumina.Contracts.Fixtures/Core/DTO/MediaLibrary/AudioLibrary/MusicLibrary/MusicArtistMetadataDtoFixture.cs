#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="MusicArtistMetadataDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicArtistMetadataDtoFixture
{
    private const int MINIMUM_LIFESPAN_YEAR = 1900;
    private const int MAXIMUM_LIFESPAN_YEAR = 2026;

    private readonly Faker _faker = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly TagDtoFixture _tagDtoFixture = new();
    private readonly MusicAreaDtoFixture _musicAreaDtoFixture = new();
    private readonly MusicArtistAliasDtoFixture _musicArtistAliasDtoFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="MusicArtistMetadataDto"/>.
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
    /// <param name="genres">Optional. The genres associated with the artist.</param>
    /// <param name="tags">Optional. The tags that describe or categorize the artist.</param>
    /// <param name="aliases">Optional. The alternative names of the artist.</param>
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
    /// <param name="includeGenres">Whether the genres should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTags">Whether the tags should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAliases">Whether the aliases should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="MusicArtistMetadataDto"/>.</returns>
    public MusicArtistMetadataDto Create(
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
        List<GenreDto>? genres = null,
        List<TagDto>? tags = null,
        List<MusicArtistAliasDto>? aliases = null,
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
        bool includeGenres = true,
        bool includeTags = true,
        bool includeAliases = true)
    {
        int lifeSpanBeginYear = Random.Shared.Next(MINIMUM_LIFESPAN_YEAR, MAXIMUM_LIFESPAN_YEAR);
        int lifeSpanEndYear = Random.Shared.Next(lifeSpanBeginYear, Math.Max(lifeSpanBeginYear + 1, MAXIMUM_LIFESPAN_YEAR));
        return new MusicArtistMetadataDto(
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
            includeGenres ? (genres ?? _genreDtoFixture.CreateMany(_faker.Random.Int(1, 3))) : null,
            includeTags ? (tags ?? _tagDtoFixture.CreateMany(_faker.Random.Int(1, 3))) : null,
            includeAliases ? (aliases ?? _musicArtistAliasDtoFixture.CreateMany(_faker.Random.Int(1, 3))) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="MusicArtistMetadataDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<MusicArtistMetadataDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
