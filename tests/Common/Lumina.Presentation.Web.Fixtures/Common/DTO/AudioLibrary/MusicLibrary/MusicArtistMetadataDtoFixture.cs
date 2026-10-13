#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Enums.AudioLibrary;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="MusicArtistMetadataDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicArtistMetadataDtoFixture
{
    private readonly Faker _faker = new();
    private readonly MusicAreaDtoFixture _musicAreaDtoFixture = new();
    private readonly MusicArtistAliasDtoFixture _musicArtistAliasDtoFixture = new();
    private readonly TagDtoFixture _tagDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();

    /// <summary>
    /// Creates a new <see cref="MusicArtistMetadataDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="name">Optional. The name of the artist.</param>
    /// <param name="sortName">Optional. The sort name of the artist.</param>
    /// <param name="disambiguation">Optional. The disambiguation comment of the artist.</param>
    /// <param name="type">Optional. The type of the artist.</param>
    /// <param name="gender">Optional. The gender of the artist.</param>
    /// <param name="country">Optional. The ISO 3166-1 alpha-2 code of the country the artist is associated with.</param>
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
    /// <param name="includeLifeSpanBegin">Whether the life span begin date should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLifeSpanEnd">Whether the life span end date should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeGenres">Whether the genres should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTags">Whether the tags should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAliases">Whether the aliases should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="MusicArtistMetadataDto"/> instance.</returns>
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
        bool isEnded = false,
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
        return new MusicArtistMetadataDto
        {
            Name = includeName ? (name ?? _faker.Name.FullName()) : null,
            SortName = includeSortName ? (sortName ?? _faker.Name.LastName()) : null,
            Disambiguation = includeDisambiguation ? (disambiguation ?? _faker.Lorem.Word()) : null,
            Type = includeType ? (type ?? _faker.PickRandom<MusicArtistType>()) : null,
            Gender = includeGender ? (gender ?? _faker.PickRandom<MusicArtistGender>()) : null,
            Country = includeCountry ? (country ?? _faker.Address.CountryCode()) : null,
            Area = includeArea ? (area ?? _musicAreaDtoFixture.Create()) : null,
            BeginArea = includeBeginArea ? (beginArea ?? _musicAreaDtoFixture.Create()) : null,
            EndArea = includeEndArea ? (endArea ?? _musicAreaDtoFixture.Create()) : null,
            LifeSpanBegin = includeLifeSpanBegin ? (lifeSpanBegin ?? DateOnly.FromDateTime(_faker.Date.Past())) : null,
            LifeSpanEnd = includeLifeSpanEnd ? (lifeSpanEnd ?? DateOnly.FromDateTime(_faker.Date.Past())) : null,
            IsEnded = isEnded,
            Genres = includeGenres ? (genres ?? [.. _genreDtoFixture.CreateMany(_faker.Random.Number(1, 3))]) : null,
            Tags = includeTags ? (tags ?? [.. _tagDtoFixture.CreateMany(_faker.Random.Number(1, 3))]) : null,
            Aliases = includeAliases ? (aliases ?? [.. _musicArtistAliasDtoFixture.CreateMany(_faker.Random.Number(1, 3))]) : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicArtistMetadataDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicArtistMetadataDto"/> instances.</returns>
    public List<MusicArtistMetadataDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
