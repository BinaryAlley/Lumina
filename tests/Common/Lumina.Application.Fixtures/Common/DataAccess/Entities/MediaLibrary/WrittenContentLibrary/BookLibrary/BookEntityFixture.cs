#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Common;
using Lumina.Application.Fixtures.Common.Setup;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Fixture class for the <see cref="BookEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookEntityFixture
{
    private readonly Faker _faker = new();
    private readonly TagEntityFixture _tagEntityFixture = new();
    private readonly GenreEntityFixture _genreEntityFixture = new();
    private readonly IsbnEntityFixture _isbnEntityFixture = new();
    private readonly BookRatingEntityFixture _bookRatingEntityFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="BookEntity"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the book.</param>
    /// <param name="libraryId">Optional. The Id of the media library that owns the book.</param>
    /// <param name="path">Optional. The file system path of the book.</param>
    /// <param name="title">Optional. The title of the book.</param>
    /// <param name="originalTitle">Optional. The original title of the book.</param>
    /// <param name="includeMetadata">Whether the owned metadata collections (Tags, Genres, ISBNs, Ratings) should be included, or forced to empty collections.</param>
    /// <returns>The created <see cref="BookEntity"/>.</returns>
    public BookEntity Create(
        Guid? id = null,
        Guid? libraryId = null,
        string? path = null,
        string? title = null,
        string? originalTitle = null,
        bool includeMetadata = true)
    {
        int releaseYear = Random.Shared.Next(2000, 2010);
        int reReleaseYear = Random.Shared.Next(2010, 2020);

        return new Faker<BookEntity>()
            .RuleFor(x => x.Id, f => id ?? f.Random.Guid())
            .RuleFor(x => x.LibraryId, f => libraryId ?? f.Random.Guid())
            .RuleFor(x => x.Path, f => path ?? f.System.FilePath())
            .RuleFor(x => x.Title, f => title ?? f.Random.String2(f.Random.Number(1, 255)))
            .RuleFor(x => x.OriginalTitle, f => originalTitle ?? f.Random.String2(f.Random.Number(1, 255)))
            .RuleFor(x => x.Description, f => f.Random.String2(f.Random.Number(1, 2000)))
            .RuleFor(x => x.OriginalReleaseDate, _faker.DateOnlyBetween(new DateOnly(releaseYear, 1, 1), new DateOnly(releaseYear, 12, 31)))
            .RuleFor(x => x.OriginalReleaseYear, releaseYear)
            .RuleFor(x => x.ReReleaseDate, _faker.DateOnlyBetween(new DateOnly(reReleaseYear, 1, 1), new DateOnly(reReleaseYear, 12, 31)))
            .RuleFor(x => x.ReReleaseYear, reReleaseYear)
            .RuleFor(x => x.ReleaseCountry, f => f.PickRandom<ReleaseCountry>())
            .RuleFor(x => x.ReleaseVersion, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.LanguageCode, f => f.Random.String2(2))
            .RuleFor(x => x.LanguageName, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.LanguageNativeName, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.OriginalLanguageCode, f => f.Random.String2(2))
            .RuleFor(x => x.OriginalLanguageName, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.OriginalLanguageNativeName, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.Tags, f => includeMetadata ? [.. _tagEntityFixture.CreateMany(f.Random.Number(1, 5))] : [])
            .RuleFor(x => x.Genres, f => includeMetadata ? [.. _genreEntityFixture.CreateMany(f.Random.Number(1, 5))] : [])
            .RuleFor(x => x.Publisher, f => f.Random.String2(f.Random.Number(1, 100)))
            .RuleFor(x => x.PageCount, Random.Shared.Next(100, 300))
            .RuleFor(x => x.Format, f => f.PickRandom<BookFormat>())
            .RuleFor(x => x.Edition, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.VolumeNumber, Random.Shared.Next(1, 3))
            .RuleFor(x => x.ASIN, f => f.Random.String2(10))
            .RuleFor(x => x.GoodreadsId, Random.Shared.Next(100000, 500000).ToString())
            .RuleFor(x => x.LCCN, CreateLCCN)
            .RuleFor(x => x.OCLCNumber, CreateOCLCNumber)
            .RuleFor(x => x.OpenLibraryId, CreateOpenLibraryId)
            .RuleFor(x => x.LibraryThingId, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.GoogleBooksId, CreateGoogleBooksId)
            .RuleFor(x => x.BarnesAndNobleId, f => f.Random.String2(10, "0123456789"))
            .RuleFor(x => x.AppleBooksId, f => $"id{f.Random.Number(1, 999999)}")
            .RuleFor(x => x.ISBNs, f => includeMetadata ? _isbnEntityFixture.CreateMany(f.Random.Number(1, 5)) : [])
            .RuleFor(x => x.Ratings, f => includeMetadata ? _bookRatingEntityFixture.CreateMany(f.Random.Number(1, 5)) : [])
            .RuleFor(x => x.CreatedOnUtc, f => f.Date.Past())
            .RuleFor(x => x.UpdatedOnUtc, f => f.Date.Recent())
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="BookEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="BookEntity"/> instances.</returns>
    public List<BookEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }

    /// <summary>
    /// Creates a valid Library of Congress Control Number (LCCN).
    /// </summary>
    /// <param name="f">The Faker instance used for generating random data.</param>
    /// <returns>A properly formatted LCCN string.</returns>
    private string CreateLCCN(Faker f)
    {
        string letters = new([.. Enumerable.Range(0, f.Random.Number(0, 3)).Select(_ => f.Random.Char('a', 'z'))]);
        string digits = f.Random.String2(f.Random.Number(8, 10), "0123456789");
        return letters + digits;
    }

    /// <summary>
    /// Creates a valid OCLC (Online Computer Library Center) number.
    /// </summary>
    /// <param name="f">The Faker instance used for generating random data.</param>
    /// <returns>A properly formatted OCLC number string with valid prefix.</returns>
    private string CreateOCLCNumber(Faker f)
    {
        string[] prefixes = ["ocm", "ocn", "on", "(OCoLC)"];
        string prefix = f.Random.ArrayElement(prefixes);
        string number = prefix switch
        {
            "ocm" => f.Random.String2(8, "0123456789"),
            "ocn" => f.Random.String2(f.Random.Number(9, 11), "0123456789"),
            "on" => f.Random.String2(10, "0123456789"),
            "(OCoLC)" => f.Random.String2(f.Random.Number(8, 10), "0123456789"),
            _ => f.Random.String2(f.Random.Number(8, 10), "0123456789")
        };
        return prefix + number;
    }

    /// <summary>
    /// Creates a valid Open Library ID.
    /// </summary>
    /// <param name="f">The Faker instance used for generating random data.</param>
    /// <returns>A properly formatted Open Library ID string.</returns>
    private string CreateOpenLibraryId(Faker f)
    {
        int firstDigit = f.Random.Number(1, 9);
        string remainingDigits = f.Random.String2(f.Random.Number(0, 6), "0123456789");
        char suffix = f.Random.ArrayElement(['A', 'M', 'W']);
        return $"OL{firstDigit}{remainingDigits}{suffix}";
    }

    /// <summary>
    /// Creates a valid Google Books ID.
    /// </summary>
    /// <param name="f">The Faker instance used for generating random data.</param>
    /// <returns>A properly formatted Google Books ID string.</returns>
    private string CreateGoogleBooksId(Faker f)
    {
        const string VALID_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
        return new string([.. Enumerable.Repeat(VALID_CHARS, 12).Select(s => s[f.Random.Number(VALID_CHARS.Length - 1)])]);
    }
}
