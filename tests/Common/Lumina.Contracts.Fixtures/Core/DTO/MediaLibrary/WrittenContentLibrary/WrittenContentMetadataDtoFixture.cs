#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary;

/// <summary>
/// Fixture class for the <see cref="WrittenContentMetadataDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class WrittenContentMetadataDtoFixture
{
    private readonly Faker _faker = new();
    private readonly Random _random = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly TagDtoFixture _tagDtoFixture = new();
    private readonly LanguageInfoDtoFixture _languageInfoDtoFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="WrittenContentMetadataDto"/>.
    /// </summary>
    /// <param name="title">Optional. The title of the written content.</param>
    /// <param name="originalTitle">Optional. The original title of the written content.</param>
    /// <param name="description">Optional. A brief description or summary of the written content.</param>
    /// <param name="releaseInfo">Optional. The release information of the written content.</param>
    /// <param name="genres">Optional. The genres associated with the written content.</param>
    /// <param name="tags">Optional. The tags that describe or categorize the written content.</param>
    /// <param name="language">Optional. The language in which the written content is written.</param>
    /// <param name="originalLanguage">Optional. The original language of the written content.</param>
    /// <param name="publisher">Optional. The name of the publisher of the written content.</param>
    /// <param name="pageCount">Optional. The number of pages in the written content.</param>
    /// <param name="includeTitle">Whether the title should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeOriginalTitle">Whether the original title should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeDescription">Whether the description should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeReleaseInfo">Whether the release information should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeGenres">Whether the genres should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTags">Whether the tags should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLanguage">Whether the language should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeOriginalLanguage">Whether the original language should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePublisher">Whether the publisher should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePageCount">Whether the page count should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="WrittenContentMetadataDto"/>.</returns>
    public WrittenContentMetadataDto Create(
        string? title = null,
        string? originalTitle = null,
        string? description = null,
        ReleaseInfoDto? releaseInfo = null,
        List<GenreDto>? genres = null,
        List<TagDto>? tags = null,
        LanguageInfoDto? language = null,
        LanguageInfoDto? originalLanguage = null,
        string? publisher = null,
        int? pageCount = null,
        bool includeTitle = true,
        bool includeOriginalTitle = true,
        bool includeDescription = true,
        bool includeReleaseInfo = true,
        bool includeGenres = true,
        bool includeTags = true,
        bool includeLanguage = true,
        bool includeOriginalLanguage = true,
        bool includePublisher = true,
        bool includePageCount = true)
    {
        return new WrittenContentMetadataDto(
            includeTitle ? (title ?? _faker.Commerce.ProductName()) : null,
            includeOriginalTitle ? (originalTitle ?? _faker.Commerce.ProductName()) : null,
            includeDescription ? (description ?? _faker.Lorem.Paragraph()) : null,
            includeReleaseInfo ? (releaseInfo ?? CreateReleaseInfo()) : null,
            includeGenres ? (genres ?? CreateDistinctGenres()) : null,
            includeTags ? (tags ?? CreateDistinctTags()) : null,
            includeLanguage ? (language ?? _languageInfoDtoFixture.Create()) : null,
            includeOriginalLanguage ? (originalLanguage ?? _languageInfoDtoFixture.Create()) : null,
            includePublisher ? (publisher ?? _faker.Company.CompanyName()) : null,
            includePageCount ? (pageCount ?? _faker.Random.Int(1, 1000)) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="WrittenContentMetadataDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<WrittenContentMetadataDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }

    /// <summary>
    /// Creates a random valid <see cref="ReleaseInfoDto"/> with consistent dates and years.
    /// </summary>
    /// <returns>The created <see cref="ReleaseInfoDto"/>.</returns>
    private ReleaseInfoDto CreateReleaseInfo()
    {
        int originalReleaseYear = _random.Next(1900, 2026);
        int reReleaseYear = _random.Next(originalReleaseYear, 2026);
        return new ReleaseInfoDto(
            new DateOnly(originalReleaseYear, 1, 1),
            originalReleaseYear,
            new DateOnly(reReleaseYear, 1, 1),
            reReleaseYear,
            _faker.Address.CountryCode(),
            _faker.Random.String2(_faker.Random.Number(1, 50)));
    }

    /// <summary>
    /// Creates two genres with distinct names, so that the storage deduplication never collapses them into one.
    /// </summary>
    /// <returns>The created genres.</returns>
    private List<GenreDto> CreateDistinctGenres()
    {
        List<GenreDto> genres = [];
        while (genres.Count < 2)
        {
            GenreDto genre = _genreDtoFixture.Create();
            if (genres.All(existing => existing.Name != genre.Name))
                genres.Add(genre);
        }
        return genres;
    }

    /// <summary>
    /// Creates two tags with distinct names, so that the storage deduplication never collapses them into one.
    /// </summary>
    /// <returns>The created tags.</returns>
    private List<TagDto> CreateDistinctTags()
    {
        List<TagDto> tags = [];
        while (tags.Count < 2)
        {
            TagDto tag = _tagDtoFixture.Create();
            if (tags.All(existing => existing.Name != tag.Name))
                tags.Add(tag);
        }
        return tags;
    }
}
