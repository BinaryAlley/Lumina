#region ========================================================================= USING =====================================================================================
using AutoFixture;
using Bogus;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Fixtures.Common.Setup;
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
    private readonly Fixture _fixture = new();
    private readonly Random _random = new();
    private readonly Faker _faker = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AddBookRequestFixture"/> class.
    /// </summary>
    public AddBookRequestFixture()
    {
        _fixture.Customizations.Add(new DateOnlySpecimenBuilder());
        _fixture.Customizations.Add(new NullableDateOnlySpecimenBuilder());
    }

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
    /// <param name="includeOptionalProperties">Whether the properties that are not explicitly provided should be randomized, or left <see langword="null"/>.</param>
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
        int releaseYear = _random.Next(2000, 2010);
        int reReleaseYear = _random.Next(2010, 2020);

        ReleaseInfoDto releaseInfo = new Faker<ReleaseInfoDto>()
            .CustomInstantiator(f => new ReleaseInfoDto(
                default,
                default,
                default,
                default,
                default,
                default
            ))
            .RuleFor(x => x.OriginalReleaseDate, _faker.DateOnlyBetween(new DateOnly(releaseYear, 1, 1), new DateOnly(releaseYear, 12, 31)))
            .RuleFor(x => x.OriginalReleaseYear, releaseYear)
            .RuleFor(x => x.ReReleaseDate, _faker.DateOnlyBetween(new DateOnly(reReleaseYear, 1, 1), new DateOnly(reReleaseYear, 12, 31)))
            .RuleFor(x => x.ReReleaseYear, reReleaseYear)
            .RuleFor(x => x.ReleaseCountry, f => f.Random.String2(2))
            .RuleFor(x => x.ReleaseVersion, f => f.Random.String2(f.Random.Number(1, 50)))
            .Generate();

        Faker<GenreDto> genre = new Faker<GenreDto>()
            .CustomInstantiator(f => new GenreDto(
                default!
            ))
            .RuleFor(e => e.Name, f => f.Random.String2(f.Random.Number(1, 50)));

        Faker<TagDto> tag = new Faker<TagDto>()
            .CustomInstantiator(f => new TagDto(
                default!
            ))
            .RuleFor(e => e.Name, f => f.Random.String2(f.Random.Number(1, 50)));

        Faker<MediaContributorRoleDto> mediaContributorRole = new Faker<MediaContributorRoleDto>()
            .CustomInstantiator(f => new MediaContributorRoleDto(
                default!,
                default!
            ))
            .RuleFor(e => e.Name, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(e => e.Category, f => f.Random.String2(f.Random.Number(1, 50)));

        Faker<MediaContributorNameDto> mediaContributorName = new Faker<MediaContributorNameDto>()
            .CustomInstantiator(f => new MediaContributorNameDto(
                default!,
                default!
            ))
            .RuleFor(e => e.DisplayName, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(e => e.LegalName, f => f.Random.String2(f.Random.Number(1, 50)));

        Faker<MediaContributorDto> mediaContributor = new Faker<MediaContributorDto>()
            .CustomInstantiator(f => new MediaContributorDto(
                default!,
                default!
            ))
            .RuleFor(e => e.Name, mediaContributorName)
            .RuleFor(e => e.Role, mediaContributorRole);

        Faker<BookRatingDto> rating = new Faker<BookRatingDto>()
            .CustomInstantiator(f => new BookRatingDto(
                default,
                default,
                default,
                default
            ))
            .RuleFor(e => e.Value, _random.Next(1, 5))
            .RuleFor(e => e.MaxValue, 5)
            .RuleFor(e => e.Source, _fixture.Create<BookRatingSource>())
            .RuleFor(e => e.VoteCount, _random.Next(1, 1000));

        Faker<LanguageInfoDto> language = new Faker<LanguageInfoDto>()
            .CustomInstantiator(f => new LanguageInfoDto(
                default!,
                default!,
                default
            ))
            .RuleFor(e => e.LanguageName, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(e => e.LanguageCode, f => f.Random.String2(2))
            .RuleFor(e => e.NativeName, f => f.Random.String2(f.Random.Number(1, 50)));

        Faker<LanguageInfoDto> originalLanguage = new Faker<LanguageInfoDto>()
            .CustomInstantiator(f => new LanguageInfoDto(
                default!,
                default!,
                default
            ))
            .RuleFor(e => e.LanguageName, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(e => e.LanguageCode, f => f.Random.String2(2))
            .RuleFor(e => e.NativeName, f => f.Random.String2(f.Random.Number(1, 50)));

        Faker<WrittenContentMetadataDto> metadataFaker = new Faker<WrittenContentMetadataDto>()
            .CustomInstantiator(f => new WrittenContentMetadataDto(
                default!,
                default,
                default,
                default!,
                default!,
                default!,
                default,
                default,
                default,
                default
            ))
            .RuleFor(x => x.Title, f => f.Random.String2(f.Random.Number(1, 255)))
            .RuleFor(x => x.OriginalTitle, f => f.Random.String2(f.Random.Number(1, 255)))
            .RuleFor(x => x.Description, f => f.Random.String2(f.Random.Number(1, 2000)))
            .RuleFor(x => x.ReleaseInfo, releaseInfo)
            .RuleFor(p => p.Genres, f => genre.Generate(f.Random.Number(1, 5)))
            .RuleFor(x => x.Tags, f => tag.Generate(f.Random.Number(1, 5)))
            .RuleFor(x => x.Language, language)
            .RuleFor(x => x.OriginalLanguage, originalLanguage)
            .RuleFor(x => x.Publisher, f => f.Random.String2(f.Random.Number(1, 100)))
            .RuleFor(x => x.PageCount, _random.Next(100, 300));

        Faker<IsbnDto> isbn = new Faker<IsbnDto>()
            .CustomInstantiator(f => new IsbnDto(
                default!,
                default
            ))
            .RuleFor(i => i.Value, f =>
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
                    int checkDigit = (10 - sum % 10) % 10;
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
                    int checkDigit = (11 - sum % 11) % 11;
                    string checkChar = checkDigit == 10 ? "X" : checkDigit.ToString();
                    return $"{digits[0]}-{digits[1]}{digits[2]}-{digits[3]}{digits[4]}{digits[5]}{digits[6]}{digits[7]}{digits[8]}-{checkChar}";
                }
            })
            .RuleFor(i => i.Format, (f, i) => i.Value!.Length > 13 ? IsbnFormat.Isbn13 : IsbnFormat.Isbn10);

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
                default!,
                default!,
                default!,
                default!,
                default!
            ))
            .RuleFor(x => x.LibraryId, libraryId ?? _fixture.Create<Guid>())
            .RuleFor(x => x.Path, f => path ?? f.System.FilePath())
            .RuleFor(x => x.Metadata, metadata ?? metadataFaker)
            .RuleFor(x => x.Format, format ?? (includeOptionalProperties ? _fixture.Create<BookFormat>() : null))
            .RuleFor(x => x.Edition, f => edition ?? (includeOptionalProperties ? f.Random.String2(f.Random.Number(1, 50)) : null))
            .RuleFor(x => x.VolumeNumber, volumeNumber ?? (includeOptionalProperties ? (float?)_random.Next(1, 3) : null))
            .RuleFor(x => x.Series, series)
            .RuleFor(x => x.ASIN, f => asin ?? (includeOptionalProperties ? f.Random.String2(10) : null))
            .RuleFor(x => x.GoodreadsId, goodreadsId ?? (includeOptionalProperties ? _random.Next(100000, 500000).ToString() : null))
            .RuleFor(x => x.LCCN, f => lccn ?? (includeOptionalProperties ? CreateLccn(f) : null))
            .RuleFor(x => x.OCLCNumber, f => oclcNumber ?? (includeOptionalProperties ? CreateOclcNumber(f) : null))
            .RuleFor(x => x.OpenLibraryId, f => openLibraryId ?? (includeOptionalProperties ? CreateOpenLibraryId(f) : null))
            .RuleFor(x => x.LibraryThingId, f => libraryThingId ?? (includeOptionalProperties ? f.Random.String2(f.Random.Number(1, 50)) : null))
            .RuleFor(x => x.GoogleBooksId, f => googleBooksId ?? (includeOptionalProperties ? CreateGoogleBooksId(f) : null))
            .RuleFor(x => x.BarnesAndNobleId, f => barnesAndNobleId ?? (includeOptionalProperties ? f.Random.String2(10, "0123456789") : null))
            .RuleFor(x => x.AppleBooksId, f => appleBooksId ?? (includeOptionalProperties ? $"id{f.Random.Number(1, 999999)}" : null))
            .RuleFor(p => p.ISBNs, f => isbns ?? (includeOptionalProperties ? isbn.Generate(f.Random.Number(1, 5)) : null))
            .RuleFor(p => p.Ratings, f => ratings ?? (includeOptionalProperties ? rating.Generate(f.Random.Number(1, 5)) : null))
            .RuleFor(x => x.Contributors, f => contributors ?? (includeOptionalProperties ? mediaContributor.Generate(f.Random.Number(1, 5)) : null));
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
