#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Fixture class for the <see cref="BookMetadataDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookMetadataDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="BookMetadataDto"/>.
    /// </summary>
    /// <param name="title">Optional. The title of the book.</param>
    /// <param name="originalTitle">Optional. The original title of the book.</param>
    /// <param name="description">Optional. A brief description or summary of the book.</param>
    /// <param name="releaseInfo">Optional. The release information of the book.</param>
    /// <param name="genres">Optional. The genres of the book.</param>
    /// <param name="tags">Optional. The tags of the book.</param>
    /// <param name="language">Optional. The language of the book.</param>
    /// <param name="originalLanguage">Optional. The original language of the book.</param>
    /// <param name="publisher">Optional. The name of the publisher of the book.</param>
    /// <param name="pageCount">Optional. The number of pages in the book.</param>
    /// <param name="format">Optional. The format of the book.</param>
    /// <param name="edition">Optional. The edition of the book.</param>
    /// <param name="volumeNumber">Optional. The volume or book number in the series.</param>
    /// <param name="series">Optional. The series the book is part of.</param>
    /// <param name="asin">Optional. The ASIN of the book.</param>
    /// <param name="goodreadsId">Optional. The Goodreads ID of the book.</param>
    /// <param name="lccn">Optional. The LCCN of the book.</param>
    /// <param name="oclcNumber">Optional. The OCLC number of the book.</param>
    /// <param name="openLibraryId">Optional. The Open Library ID of the book.</param>
    /// <param name="libraryThingId">Optional. The LibraryThing ID of the book.</param>
    /// <param name="googleBooksId">Optional. The Google Books ID of the book.</param>
    /// <param name="barnesAndNobleId">Optional. The Barnes and Noble ID of the book.</param>
    /// <param name="appleBooksId">Optional. The Apple Books ID of the book.</param>
    /// <param name="isbns">Optional. The ISBNs of the book.</param>
    /// <param name="contributors">Optional. The contributors of the book.</param>
    /// <param name="ratings">Optional. The ratings of the book.</param>
    /// <param name="coverImagePath">Optional. The path of the cover image of the book.</param>
    /// <param name="includeTitle">Whether the title should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeOptionalProperties">Whether the properties that are not explicitly provided should be randomized, or left <see langword="null"/>.</param>
    /// <returns>The created <see cref="BookMetadataDto"/>.</returns>
    public BookMetadataDto Create(
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
        BookFormat? format = null,
        string? edition = null,
        float? volumeNumber = null,
        BookSeriesDto? series = null,
        string? asin = null,
        string? goodreadsId = null,
        string? lccn = null,
        string? oclcNumber = null,
        string? openLibraryId = null,
        string? libraryThingId = null,
        string? googleBooksId = null,
        string? barnesAndNobleId = null,
        string? appleBooksId = null,
        List<IsbnDto>? isbns = null,
        List<MediaContributorDto>? contributors = null,
        List<BookRatingDto>? ratings = null,
        string? coverImagePath = null,
        bool includeTitle = true,
        bool includeOptionalProperties = true)
    {
        int releaseYear = _faker.Random.Int(1900, 2024);
        return new BookMetadataDto(
            includeTitle ? title ?? (includeOptionalProperties ? _faker.Lorem.Sentence(3) : null) : null,
            originalTitle,
            description,
            releaseInfo ?? (includeOptionalProperties ? new ReleaseInfoDto(new DateOnly(releaseYear, 1, 1), releaseYear, null, null, _faker.Address.CountryCode(), null) : null),
            genres ?? (includeOptionalProperties ? [new GenreDto(_faker.Lorem.Word())] : null),
            tags ?? (includeOptionalProperties ? [new TagDto(_faker.Lorem.Word())] : null),
            language ?? (includeOptionalProperties ? new LanguageInfoDto("en", "English", "English") : null),
            originalLanguage,
            publisher,
            pageCount,
            format ?? (includeOptionalProperties ? _faker.PickRandom<BookFormat>() : null),
            edition,
            volumeNumber ?? (includeOptionalProperties ? _faker.Random.Int(1, 10) : null),
            series,
            asin,
            goodreadsId,
            lccn,
            oclcNumber,
            openLibraryId,
            libraryThingId,
            googleBooksId,
            barnesAndNobleId,
            appleBooksId,
            isbns,
            contributors,
            ratings,
            coverImagePath);
    }

    /// <summary>
    /// Creates a list of <see cref="BookMetadataDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<BookMetadataDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
