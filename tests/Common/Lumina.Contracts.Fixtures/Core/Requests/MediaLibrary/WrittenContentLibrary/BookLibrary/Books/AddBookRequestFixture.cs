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
/// Fixture class for the <see cref="AddBookRequest"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddBookRequestFixture
{
    private readonly IsbnDtoFixture _isbnDtoFixture = new();
    private readonly BookRatingDtoFixture _bookRatingDtoFixture = new();
    private readonly MediaContributorDtoFixture _mediaContributorDtoFixture = new();
    private readonly WrittenContentMetadataDtoFixture _writtenContentMetadataDtoFixture = new();

    /// <summary>
    /// Creates a random valid request to add a book.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library the book belongs to.</param>
    /// <param name="path">Optional. The file system path of the book.</param>
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
    /// <param name="includeOptionalProperties">Whether the properties that are not explicitly provided should be randomized, or forced to <see langword="null"/>.</param>
    /// <returns>The created request to add a book.</returns>
    public AddBookRequest Create(
        Guid? libraryId = null,
        string? path = null,
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
        List<MediaContributorDto>? contributors = null,
        List<BookRatingDto>? ratings = null,
        bool includeOptionalProperties = true)
    {
        return new Faker<AddBookRequest>()
            .CustomInstantiator(f => new AddBookRequest(
                default,
                default!,
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
                default!,
                default!,
                default!,
                default!
            ))
            .RuleFor(x => x.LibraryId, libraryId ?? Guid.NewGuid())
            .RuleFor(x => x.Path, f => path ?? f.System.FilePath())
            .RuleFor(x => x.Metadata, metadata ?? _writtenContentMetadataDtoFixture.Create())
            .RuleFor(x => x.Format, f => format ?? (includeOptionalProperties ? f.PickRandom<BookFormat>() : null))
            .RuleFor(x => x.Edition, f => edition ?? (includeOptionalProperties ? f.Random.String2(f.Random.Number(1, 50)) : null))
            .RuleFor(x => x.VolumeNumber, f => volumeNumber ?? (includeOptionalProperties ? (float?)f.Random.Number(1, 3) : null))
            .RuleFor(x => x.Series, series)
            .RuleFor(x => x.ASIN, f => asin ?? (includeOptionalProperties ? f.Random.String2(10) : null))
            .RuleFor(x => x.GoodreadsId, f => goodreadsId ?? (includeOptionalProperties ? f.Random.Number(100000, 500000).ToString() : null))
            .RuleFor(x => x.LCCN, f => lccn ?? (includeOptionalProperties ? CreateLccn(f) : null))
            .RuleFor(x => x.OCLCNumber, f => oclcNumber ?? (includeOptionalProperties ? CreateOclcNumber(f) : null))
            .RuleFor(x => x.OpenLibraryId, f => openLibraryId ?? (includeOptionalProperties ? CreateOpenLibraryId(f) : null))
            .RuleFor(x => x.LibraryThingId, f => libraryThingId ?? (includeOptionalProperties ? f.Random.String2(f.Random.Number(1, 50)) : null))
            .RuleFor(x => x.GoogleBooksId, f => googleBooksId ?? (includeOptionalProperties ? CreateGoogleBooksId(f) : null))
            .RuleFor(x => x.BarnesAndNobleId, f => barnesAndNobleId ?? (includeOptionalProperties ? f.Random.String2(10, "0123456789") : null))
            .RuleFor(x => x.AppleBooksId, f => appleBooksId ?? (includeOptionalProperties ? $"id{f.Random.Number(1, 999999)}" : null))
            .RuleFor(p => p.ISBNs, f => isbns ?? (includeOptionalProperties ? [.. _isbnDtoFixture.CreateMany(f.Random.Number(1, 3))] : null))
            .RuleFor(p => p.Ratings, f => ratings ?? (includeOptionalProperties ? [.. _bookRatingDtoFixture.CreateMany(f.Random.Number(1, 3))] : null))
            .RuleFor(x => x.Contributors, f => contributors ?? (includeOptionalProperties ? [.. _mediaContributorDtoFixture.CreateMany(f.Random.Number(1, 3))] : null));
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
