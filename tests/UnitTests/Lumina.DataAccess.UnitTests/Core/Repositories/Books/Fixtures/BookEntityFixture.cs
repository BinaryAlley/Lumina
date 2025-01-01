#region ========================================================================= USING =====================================================================================
using AutoFixture;
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Common.Enums.BookLibrary;
using Lumina.DataAccess.UnitTests.Common.Setup;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.Repositories.Books.Fixtures;

/// <summary>
/// Fixture class for the <see cref="BookEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookEntityFixture
{
    private readonly Fixture _fixture;
    private readonly Random _random = new();
    private readonly Faker _faker;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookEntityFixture"/> class.
    /// </summary>
    public BookEntityFixture()
    {
        _fixture = new Fixture();
        _fixture.Customizations.Add(new DateOnlySpecimenBuilder());
        _fixture.Customizations.Add(new NullableDateOnlySpecimenBuilder());
        _faker = new Faker();
        ConfigureCustomTypes();
    }

    /// <summary>
    /// Creates a random valid <see cref="BookEntity"/>.
    /// </summary>
    /// <returns>The created <see cref="BookEntity"/>.</returns>
    public BookEntity CreateBookModel()
    {
        int releaseYear = _random.Next(2000, 2010);
        int reReleaseYear = _random.Next(2010, 2020);

        return new Faker<BookEntity>()
            .RuleFor(x => x.Id, f => f.Random.Guid())
            .RuleFor(x => x.Title, f => f.Random.String2(f.Random.Number(1, 255)))
            .RuleFor(x => x.OriginalTitle, f => f.Random.String2(f.Random.Number(1, 255)))
            .RuleFor(x => x.Description, f => f.Random.String2(f.Random.Number(1, 2000)))
            .RuleFor(x => x.OriginalReleaseDate, _faker.DateOnlyBetween(new DateOnly(releaseYear, 1, 1), new DateOnly(releaseYear, 12, 31)))
            .RuleFor(x => x.OriginalReleaseYear, releaseYear)
            .RuleFor(x => x.ReReleaseDate, _faker.DateOnlyBetween(new DateOnly(reReleaseYear, 1, 1), new DateOnly(reReleaseYear, 12, 31)))
            .RuleFor(x => x.ReReleaseYear, reReleaseYear)
            .RuleFor(x => x.ReleaseCountry, f => f.Random.String2(2))
            .RuleFor(x => x.ReleaseVersion, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.LanguageCode, f => f.Random.String2(2))
            .RuleFor(x => x.LanguageName, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.LanguageNativeName, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.OriginalLanguageCode, f => f.Random.String2(2))
            .RuleFor(x => x.OriginalLanguageName, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.OriginalLanguageNativeName, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.Tags, f => new HashSet<TagEntity>(CreateTags(f.Random.Number(1, 5))))
            .RuleFor(x => x.Genres, f => new HashSet<GenreEntity>(CreateGenres(f.Random.Number(1, 5))))
            .RuleFor(x => x.Publisher, f => f.Random.String2(f.Random.Number(1, 100)))
            .RuleFor(x => x.PageCount, _random.Next(100, 300))
            .RuleFor(x => x.Format, f => f.PickRandom<BookFormat>())
            .RuleFor(x => x.Edition, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.VolumeNumber, _random.Next(1, 3))
            .RuleFor(x => x.ASIN, f => f.Random.String2(10))
            .RuleFor(x => x.GoodreadsId, _random.Next(100000, 500000).ToString())
            .RuleFor(x => x.LCCN, CreateLCCN)
            .RuleFor(x => x.OCLCNumber, CreateOCLCNumber)
            .RuleFor(x => x.OpenLibraryId, CreateOpenLibraryId)
            .RuleFor(x => x.LibraryThingId, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.GoogleBooksId, CreateGoogleBooksId)
            .RuleFor(x => x.BarnesAndNobleId, f => f.Random.String2(10, "0123456789"))
            .RuleFor(x => x.AppleBooksId, f => $"id{f.Random.Number(1, 999999)}")
            .RuleFor(x => x.ISBNs, f => CreateIsbns(f.Random.Number(1, 5)))
            .RuleFor(x => x.Ratings, f => CreateBookRatings(f.Random.Number(1, 5)))
            .RuleFor(x => x.CreatedOnUtc, f => f.Date.Past())
            .RuleFor(x => x.UpdatedOnUtc, f => f.Date.Recent())
            .Generate();
    }

    /// <summary>
    /// Creates a list of tag entities with random data.
    /// </summary>
    /// <param name="count">The number of tags to create.</param>
    /// <returns>A list of randomly generated tag entities.</returns>
    private static List<TagEntity> CreateTags(int count)
    {
        return new Faker<TagEntity>()
            .CustomInstantiator(f => new TagEntity(f.Random.String2(f.Random.Number(1, 50))))
            .Generate(count);
    }

    /// <summary>
    /// Creates a list of genre entities with random data.
    /// </summary>
    /// <param name="count">The number of genres to create.</param>
    /// <returns>A list of randomly generated genre entities.</returns>
    private static List<GenreEntity> CreateGenres(int count)
    {
        return new Faker<GenreEntity>()
            .CustomInstantiator(f => new GenreEntity(f.Random.String2(f.Random.Number(1, 50))))
            .Generate(count);
    }

    /// <summary>
    /// Creates a valid Library of Congress Control Number (LCCN).
    /// </summary>
    /// <param name="f">The Faker instance used for generating random data.</param>
    /// <returns>A properly formatted LCCN string.</returns>
    private string CreateLCCN(Faker f)
    {
        string letters = new(Enumerable.Range(0, f.Random.Number(0, 3))
            .Select(_ => f.Random.Char('a', 'z'))
            .ToArray());
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
        char suffix = f.Random.ArrayElement(new[] { 'A', 'M', 'W' });
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
        return new string(Enumerable.Repeat(VALID_CHARS, 12)
            .Select(s => s[f.Random.Number(VALID_CHARS.Length - 1)])
            .ToArray());
    }

    /// <summary>
    /// Creates a list of ISBN entities with random data.
    /// </summary>
    /// <param name="count">The number of ISBNs to create.</param>
    /// <returns>A list of randomly generated ISBN entities.</returns>
    private static List<IsbnEntity> CreateIsbns(int count)
    {
        return new Faker<IsbnEntity>()
            .CustomInstantiator(f => new IsbnEntity(CreateIsbn(f), f.PickRandom<IsbnFormat>()))
            .Generate(count);
    }

    /// <summary>
    /// Creates a valid ISBN (International Standard Book Number).
    /// </summary>
    /// <param name="f">The Faker instance used for generating random data.</param>
    /// <returns>A properly formatted ISBN-10 or ISBN-13 string with valid checksum.</returns>
    private static string CreateIsbn(Faker f)
    {
        bool isIsbn13 = f.Random.Bool();
        if (isIsbn13)
        {
            string prefix = f.Random.Bool() ? "978" : "979";
            string group = f.Random.Number(0, 99999).ToString().PadLeft(5, '0');
            string publisher = f.Random.Number(0, 999999).ToString().PadLeft(6, '0');
            string title = f.Random.Number(0, 99).ToString().PadLeft(2, '0');
            string isbn = $"{prefix}{group[..1]}{publisher}{title}";
            int sum = 0;
            for (int i = 0; i < 12; i++)
                sum += (i % 2 == 0 ? 1 : 3) * int.Parse(isbn[i].ToString());
            int checkDigit = (10 - (sum % 10)) % 10;
            return $"{prefix}-{group[..1]}-{publisher}-{title}-{checkDigit}";
        }
        else
        {
            int[] digits = new int[9];
            for (int i = 0; i < 9; i++)
                digits[i] = f.Random.Number(0, 9);
            int sum = 0;
            for (int i = 0; i < 9; i++)
                sum += (10 - i) * digits[i];
            int checkDigit = (11 - (sum % 11)) % 11;
            string checkChar = checkDigit == 10 ? "X" : checkDigit.ToString();
            return $"{digits[0]}-{digits[1]}{digits[2]}-{digits[3]}{digits[4]}{digits[5]}{digits[6]}{digits[7]}{digits[8]}-{checkChar}";
        }
    }

    // <summary>
    /// Creates a list of book rating entities with random data.
    /// </summary>
    /// <param name="count">The number of ratings to create.</param>
    /// <returns>A list of randomly generated book rating entities.</returns>
    private static List<BookRatingEntity> CreateBookRatings(int count)
    {
        return new Faker<BookRatingEntity>()
            .CustomInstantiator(f => new BookRatingEntity(
                f.Random.Decimal(1, 5),
                5,
                f.PickRandom<BookRatingSource>(),
                f.Random.Number(1, 1000)
            ))
            .Generate(count);
    }

    /// <summary>
    /// Configures custom type generation rules for the AutoFixture instance.
    /// </summary>
    private void ConfigureCustomTypes()
    {
        _fixture.Register(() => new TagEntity(_fixture.Create<string>()));
        _fixture.Register(() => new GenreEntity(_fixture.Create<string>()));
        _fixture.Register(() => new IsbnEntity(_fixture.Create<string>(), _fixture.Create<IsbnFormat>()));
        _fixture.Register(() => new BookRatingEntity(
            _fixture.Create<decimal>(),
            _fixture.Create<decimal>(),
            _fixture.Create<BookRatingSource>(),
            _fixture.Create<int>()
        ));
    }
}
