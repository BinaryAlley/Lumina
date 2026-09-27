#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Fixture class for the <see cref="UpdateBookRequest"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateBookRequestFixture
{
    private readonly IsbnDtoFixture _isbnDtoFixture = new();
    private readonly BookRatingDtoFixture _bookRatingDtoFixture = new();
    private readonly MediaContributorReferenceDtoFixture _mediaContributorReferenceDtoFixture = new();
    private readonly WrittenContentMetadataDtoFixture _writtenContentMetadataDtoFixture = new();

    /// <summary>
    /// Creates a random valid request to update a book.
    /// </summary>
    /// <param name="metadata">Optional. The written content metadata of the book.</param>
    /// <param name="format">Optional. The format of the book.</param>
    /// <param name="edition">Optional. The edition of the book.</param>
    /// <param name="volumeNumber">Optional. The volume or book number in the series.</param>
    /// <param name="series">Optional. The series the book is part of.</param>
    /// <param name="asin">Optional. The ASIN of the book.</param>
    /// <param name="goodreadsId">Optional. The Goodreads Id of the book.</param>
    /// <param name="lccn">Optional. The LCCN of the book.</param>
    /// <param name="oclcNumber">Optional. The OCLC number of the book.</param>
    /// <param name="openLibraryId">Optional. The Open Library Id of the book.</param>
    /// <param name="libraryThingId">Optional. The LibraryThing Id of the book.</param>
    /// <param name="googleBooksId">Optional. The Google Books Id of the book.</param>
    /// <param name="barnesAndNobleId">Optional. The Barnes and Noble Id of the book.</param>
    /// <param name="appleBooksId">Optional. The Apple Books Id of the book.</param>
    /// <param name="isbns">Optional. The ISBNs of the book.</param>
    /// <param name="contributors">Optional. The contributors of the book.</param>
    /// <param name="ratings">Optional. The ratings of the book.</param>
    /// <param name="includeFormat">Whether the format should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeEdition">Whether the edition should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeVolumeNumber">Whether the volume number should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAsin">Whether the ASIN should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeGoodreadsId">Whether the Goodreads Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLccn">Whether the LCCN should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeOclcNumber">Whether the OCLC number should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeOpenLibraryId">Whether the Open Library Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLibraryThingId">Whether the LibraryThing Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeGoogleBooksId">Whether the Google Books Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeBarnesAndNobleId">Whether the Barnes and Noble Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAppleBooksId">Whether the Apple Books Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeIsbns">Whether the ISBNs should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeRatings">Whether the ratings should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeContributors">Whether the contributors should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created request to update a book.</returns>
    public UpdateBookRequest Create(
        WrittenContentMetadataDto? metadata = null,
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
        List<MediaContributorReferenceDto>? contributors = null,
        List<BookRatingDto>? ratings = null,
        bool includeFormat = true,
        bool includeEdition = true,
        bool includeVolumeNumber = true,
        bool includeAsin = true,
        bool includeGoodreadsId = true,
        bool includeLccn = true,
        bool includeOclcNumber = true,
        bool includeOpenLibraryId = true,
        bool includeLibraryThingId = true,
        bool includeGoogleBooksId = true,
        bool includeBarnesAndNobleId = true,
        bool includeAppleBooksId = true,
        bool includeIsbns = true,
        bool includeRatings = true,
        bool includeContributors = true)
    {
        return new Faker<UpdateBookRequest>()
            .CustomInstantiator(f => new UpdateBookRequest(
                default!,
                default,
                default,
                default,
                default,
                default,
                default,
                default,
                default,
                default,
                default,
                default,
                default,
                default,
                default!,
                default!,
                default!
            ))
            .RuleFor(x => x.Metadata, metadata ?? _writtenContentMetadataDtoFixture.Create())
            .RuleFor(x => x.Format, f => format ?? (includeFormat ? f.PickRandom<BookFormat>() : null))
            .RuleFor(x => x.Edition, f => edition ?? (includeEdition ? f.Random.String2(f.Random.Number(1, 50)) : null))
            .RuleFor(x => x.VolumeNumber, f => volumeNumber ?? (includeVolumeNumber ? (float?)f.Random.Number(1, 3) : null))
            .RuleFor(x => x.Series, series)
            .RuleFor(x => x.ASIN, f => asin ?? (includeAsin ? f.Random.String2(10) : null))
            .RuleFor(x => x.GoodreadsId, f => goodreadsId ?? (includeGoodreadsId ? f.Random.Number(100000, 500000).ToString() : null))
            .RuleFor(x => x.LCCN, f => lccn ?? (includeLccn ? CreateLccn(f) : null))
            .RuleFor(x => x.OCLCNumber, f => oclcNumber ?? (includeOclcNumber ? CreateOclcNumber(f) : null))
            .RuleFor(x => x.OpenLibraryId, f => openLibraryId ?? (includeOpenLibraryId ? CreateOpenLibraryId(f) : null))
            .RuleFor(x => x.LibraryThingId, f => libraryThingId ?? (includeLibraryThingId ? f.Random.String2(f.Random.Number(1, 50)) : null))
            .RuleFor(x => x.GoogleBooksId, f => googleBooksId ?? (includeGoogleBooksId ? CreateGoogleBooksId(f) : null))
            .RuleFor(x => x.BarnesAndNobleId, f => barnesAndNobleId ?? (includeBarnesAndNobleId ? f.Random.String2(10, "0123456789") : null))
            .RuleFor(x => x.AppleBooksId, f => appleBooksId ?? (includeAppleBooksId ? $"id{f.Random.Number(1, 999999)}" : null))
            .RuleFor(p => p.ISBNs, f => isbns ?? (includeIsbns ? [.. _isbnDtoFixture.CreateMany(f.Random.Number(1, 3))] : null))
            .RuleFor(p => p.Ratings, f => ratings ?? (includeRatings ? [.. _bookRatingDtoFixture.CreateMany(f.Random.Number(1, 3))] : null))
            .RuleFor(x => x.Contributors, f => contributors ?? (includeContributors ? [.. _mediaContributorReferenceDtoFixture.CreateMany(f.Random.Number(1, 3))] : null));
    }

    /// <summary>
    /// Generates a random LCCN (Library of Congress Control Number).
    /// </summary>
    /// <param name="faker">The faker used to generate the value.</param>
    /// <returns>The generated LCCN.</returns>
    private static string CreateLccn(Faker faker)
    {
        string letters = new([.. Enumerable.Range(0, faker.Random.Number(0, 3)).Select(_ => faker.Random.Char('a', 'z'))]);
        string digits = faker.Random.String2(faker.Random.Number(8, 10), "0123456789");
        return letters + digits;
    }

    /// <summary>
    /// Generates a random OCLC number (WorldCat identifier).
    /// </summary>
    /// <param name="faker">The faker used to generate the value.</param>
    /// <returns>The generated OCLC number.</returns>
    private static string CreateOclcNumber(Faker faker)
    {
        string[] prefixes = ["ocm", "ocn", "on", "(OCoLC)"];
        string prefix = faker.Random.ArrayElement(prefixes);
        string number;
        switch (prefix)
        {
            case "ocm":
                number = faker.Random.String2(8, "0123456789");
                break;
            case "ocn":
                number = faker.Random.String2(faker.Random.Number(9, 11), "0123456789");
                break;
            case "on":
                number = faker.Random.String2(10, "0123456789");
                break;
            case "(OCoLC)":
                number = faker.Random.String2(faker.Random.Number(8, 10), "0123456789");
                break;
            default:
                number = faker.Random.String2(faker.Random.Number(8, 10), "0123456789");
                return number;
        }
        return prefix + number;
    }

    /// <summary>
    /// Generates a random Open Library Id.
    /// </summary>
    /// <param name="faker">The faker used to generate the value.</param>
    /// <returns>The generated Open Library Id.</returns>
    private static string CreateOpenLibraryId(Faker faker)
    {
        int firstDigit = faker.Random.Number(1, 9);
        string remainingDigits = faker.Random.String2(faker.Random.Number(0, 6), "0123456789");
        char suffix = faker.Random.ArrayElement(['A', 'M', 'W']);
        return $"OL{firstDigit}{remainingDigits}{suffix}";
    }

    /// <summary>
    /// Generates a random Google Books Id.
    /// </summary>
    /// <param name="faker">The faker used to generate the value.</param>
    /// <returns>The generated Google Books Id.</returns>
    private static string CreateGoogleBooksId(Faker faker)
    {
        const string VALID_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
        return new string([.. Enumerable.Repeat(VALID_CHARS, 12).Select(s => s[faker.Random.Number(VALID_CHARS.Length - 1)])]);
    }
}
