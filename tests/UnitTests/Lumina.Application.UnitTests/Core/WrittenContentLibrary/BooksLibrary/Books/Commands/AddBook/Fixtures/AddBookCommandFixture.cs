#region ========================================================================= USING =====================================================================================
using AutoFixture;
using Bogus;
using Lumina.Application.Core.WrittenContentLibrary.BooksLibrary.Books.Commands.AddBook;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Contracts.Enums.BookLibrary;
using Lumina.Contracts.Models.Common;
using Lumina.Contracts.Models.MediaContributors;
using Lumina.Contracts.Models.WrittenContentLibrary;
using Lumina.Contracts.Models.WrittenContentLibrary.BookLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Core.WrittenContentLibrary.BooksLibrary.Books.Commands.AddBook.Fixtures;

/// <summary>
/// Fixture class for the <see cref="AddBookCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddBookCommandFixture
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly Fixture _fixture;
    private readonly Random _random = new();
    private readonly Faker _faker;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="AddBookCommandFixture"/> class.
    /// </summary>
    public AddBookCommandFixture()
    {
        _fixture = new Fixture();
        _fixture.Customizations.Add(new DateOnlySpecimenBuilder());
        _fixture.Customizations.Add(new NullableDateOnlySpecimenBuilder());
        _faker = new Faker();
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a random valid command to add a book.
    /// </summary>
    /// <returns>The created command to add a book.</returns>
    public AddBookCommand CreateCommandBook()
    {
        int releaseYear = _random.Next(2000, 2010);
        int reReleaseYear = _random.Next(2010, 2020);

        ReleaseInfoModel releaseInfo = new Faker<ReleaseInfoModel>()
            .CustomInstantiator(f => new ReleaseInfoModel(
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

        Faker<GenreModel> genre = new Faker<GenreModel>()
            .CustomInstantiator(f => new GenreModel(
                default!
            ))
            .RuleFor(e => e.Name, f => f.Random.String2(f.Random.Number(1, 50)));

        Faker<TagModel> tag = new Faker<TagModel>()
            .CustomInstantiator(f => new TagModel(
                default!
            ))
            .RuleFor(e => e.Name, f => f.Random.String2(f.Random.Number(1, 50)));

        Faker<MediaContributorRoleModel> mediaContributorRole = new Faker<MediaContributorRoleModel>()
            .CustomInstantiator(f => new MediaContributorRoleModel(
                default!,
                default!
            ))
            .RuleFor(e => e.Name, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(e => e.Category, f => f.Random.String2(f.Random.Number(1, 50)));
        Faker<MediaContributorNameModel> mediaContributorName = new Faker<MediaContributorNameModel>()
            .CustomInstantiator(f => new MediaContributorNameModel(
                default!,
                default!
            ))
            .RuleFor(e => e.DisplayName, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(e => e.LegalName, f => f.Random.String2(f.Random.Number(1, 50)));
        Faker<MediaContributorModel> mediaContributor = new Faker<MediaContributorModel>()
            .CustomInstantiator(f => new MediaContributorModel(
                default!,
                default!
            ))
            .RuleFor(e => e.Name, mediaContributorName)
            .RuleFor(e => e.Role, mediaContributorRole);

        Faker<BookRatingModel> rating = new Faker<BookRatingModel>()
           .CustomInstantiator(f => new BookRatingModel(
                default,
                default,
                default,
                default
            ))
            .RuleFor(e => e.Value, _random.Next(1, 5))
           .RuleFor(e => e.MaxValue, 5)
           .RuleFor(e => e.Source, _fixture.Create<BookRatingSource>())
           .RuleFor(e => e.VoteCount, _random.Next(1, 1000));

        Faker<LanguageInfoModel> language = new Faker<LanguageInfoModel>()
            .CustomInstantiator(f => new LanguageInfoModel(
                default!,
                default!,
                default
            ))
            .RuleFor(e => e.LanguageName, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(e => e.LanguageCode, f => f.Random.String2(2))
            .RuleFor(e => e.NativeName, f => f.Random.String2(f.Random.Number(1, 50)));

        Faker<LanguageInfoModel> originalLanguage = new Faker<LanguageInfoModel>()
            .CustomInstantiator(f => new LanguageInfoModel(
                default!,
                default!,
                default
            ))
            .RuleFor(e => e.LanguageName, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(e => e.LanguageCode, f => f.Random.String2(2))
            .RuleFor(e => e.NativeName, f => f.Random.String2(f.Random.Number(1, 50)));

        Faker<WrittenContentMetadataModel> metadata = new Faker<WrittenContentMetadataModel>()
            .CustomInstantiator(f => new WrittenContentMetadataModel(
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

        Faker<IsbnModel> isbn = new Faker<IsbnModel>()
            .CustomInstantiator(f => new IsbnModel(
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

        return new Faker<AddBookCommand>()
            .CustomInstantiator(f => new AddBookCommand(
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
            .RuleFor(x => x.Metadata, metadata)
            .RuleFor(x => x.Format, _fixture.Create<BookFormat>())
            .RuleFor(x => x.Edition, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.VolumeNumber, _random.Next(1, 3))
            .RuleFor(x => x.ASIN, f => f.Random.String2(10))
            .RuleFor(x => x.GoodreadsId, _random.Next(100000, 500000).ToString())
            .RuleFor(x => x.LCCN, f =>
            {
                string letters = new(Enumerable.Range(0, f.Random.Number(0, 3))
                    .Select(_ => f.Random.Char('a', 'z'))
                    .ToArray());
                string digits = f.Random.String2(f.Random.Number(8, 10), "0123456789");
                return letters + digits;
            })
            .RuleFor(x => x.OCLCNumber, f =>
            {
                string[] prefixes = ["ocm", "ocn", "on", "(OCoLC)"];
                string prefix = f.Random.ArrayElement(prefixes);
                string number;
                switch (prefix)
                {
                    case "ocm":
                        number = f.Random.String2(8, "0123456789");
                        break;
                    case "ocn":
                        number = f.Random.String2(f.Random.Number(9, 11), "0123456789");
                        break;
                    case "on":
                        number = f.Random.String2(10, "0123456789");
                        break;
                    case "(OCoLC)":
                        number = f.Random.String2(f.Random.Number(8, 10), "0123456789");
                        break;
                    default:
                        number = f.Random.String2(f.Random.Number(8, 10), "0123456789");
                        return number;
                }
                return prefix + number;
            })
            .RuleFor(x => x.OpenLibraryId, f =>
            {
                int firstDigit = f.Random.Number(1, 9);
                string remainingDigits = f.Random.String2(f.Random.Number(0, 6), "0123456789");
                char suffix = f.Random.ArrayElement(new[] { 'A', 'M', 'W' });
                return $"OL{firstDigit}{remainingDigits}{suffix}";
            })
            .RuleFor(x => x.LibraryThingId, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.GoogleBooksId, f =>
            {
                const string VALID_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
                return new string(Enumerable.Repeat(VALID_CHARS, 12)
                    .Select(s => s[f.Random.Number(VALID_CHARS.Length - 1)])
                    .ToArray());
            })
            .RuleFor(x => x.BarnesAndNobleId, f => f.Random.String2(10, "0123456789"))
            .RuleFor(x => x.AppleBooksId, f => $"id{f.Random.Number(1, 999999)}")
            .RuleFor(x => x.AppleBooksId, f => $"id{f.Random.Number(1, 999999)}")
            .RuleFor(p => p.ISBNs, f => isbn.Generate(f.Random.Number(1, 5)))
            .RuleFor(p => p.Ratings, f => rating.Generate(f.Random.Number(1, 5)))
            .RuleFor(x => x.Contributors, f => mediaContributor.Generate(f.Random.Number(1, 5)));
    }

    private void ConfigureCustomRequestTypes()
    {
        int releaseYear = _random.Next(2000, 2010);
        int reReleaseYear = _random.Next(2010, 2020);

        _fixture.Register(() => new ReleaseInfoModel(
            _faker.DateOnlyBetween(new DateOnly(releaseYear, 1, 1), new DateOnly(releaseYear, 12, 31)),
            releaseYear,
            _faker.DateOnlyBetween(new DateOnly(reReleaseYear, 1, 1), new DateOnly(reReleaseYear, 12, 31)),
            reReleaseYear,
            _fixture.Create<string>()[2..],
            _fixture.Create<string?>()));

        _fixture.Register(() => new GenreModel(
            _fixture.Create<string>()
        ));
        _fixture.Register(() => new TagModel(
            _fixture.Create<string>()
        ));
        _fixture.Register(() => new LanguageInfoModel(
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string?>()
        ));
        _fixture.Register(() => new IsbnModel(
            _fixture.Create<string>(),
            _fixture.Create<IsbnFormat>()
        ));
        _fixture.Register(() => new BookRatingModel(
            _fixture.Create<decimal>(),
            _fixture.Create<decimal>(),
            _fixture.Create<BookRatingSource?>(),
            _fixture.Create<int?>()
        ));
        _fixture.Register(() => new WrittenContentMetadataModel(
             _fixture.Create<string>()[255..],
             LimitStringLength(_fixture.Create<string?>(), 255),
             LimitStringLength(_fixture.Create<string?>(), 2000),
             _fixture.Create<ReleaseInfoModel>(),
             _fixture.Create<List<GenreModel>>(),
             _fixture.Create<List<TagModel>>(),
             _fixture.Create<LanguageInfoModel?>(),
             _fixture.Create<LanguageInfoModel?>(),
             _fixture.Create<string?>(),
             _fixture.Create<int?>()
        ));
        _fixture.Register(() => new BookSeriesModel(
            _fixture.Create<string>()
        ));
        _fixture.Register(() => new MediaContributorRoleModel(
            _fixture.Create<string>(),
            _fixture.Create<string>()
        ));
        _fixture.Register(() => new MediaContributorNameModel(
            _fixture.Create<string>(),
            _fixture.Create<string?>()
        ));
        _fixture.Register(() => new MediaContributorModel(
            _fixture.Create<MediaContributorNameModel>(),
            _fixture.Create<MediaContributorRoleModel>()
        ));
    }

    private static string? LimitStringLength(string? input, int maxLength)
    {
        return input?.Length > maxLength ? input[..maxLength] : input;
    }
    #endregion
}
