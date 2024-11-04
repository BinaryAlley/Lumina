#region ========================================================================= USING =====================================================================================
using AutoFixture;
using Bogus;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Commands.AddBook;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Common.Enums.BookLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Commands.AddBook.Fixtures;

/// <summary>
/// Fixture class for the <see cref="AddBookCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddBookCommandFixture
{
    private readonly Fixture _fixture;
    private readonly Random _random = new();
    private readonly Faker _faker;

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

    /// <summary>
    /// Creates a random valid command to add a book.
    /// </summary>
    /// <returns>The created command to add a book.</returns>
    public AddBookCommand CreateCommandBook()
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

        Faker<WrittenContentMetadataDto> metadata = new Faker<WrittenContentMetadataDto>()
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

        _fixture.Register(() => new ReleaseInfoDto(
            _faker.DateOnlyBetween(new DateOnly(releaseYear, 1, 1), new DateOnly(releaseYear, 12, 31)),
            releaseYear,
            _faker.DateOnlyBetween(new DateOnly(reReleaseYear, 1, 1), new DateOnly(reReleaseYear, 12, 31)),
            reReleaseYear,
            _fixture.Create<string>()[2..],
            _fixture.Create<string?>()));

        _fixture.Register(() => new GenreDto(
            _fixture.Create<string>()
        ));
        _fixture.Register(() => new TagDto(
            _fixture.Create<string>()
        ));
        _fixture.Register(() => new LanguageInfoDto(
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string?>()
        ));
        _fixture.Register(() => new IsbnDto(
            _fixture.Create<string>(),
            _fixture.Create<IsbnFormat>()
        ));
        _fixture.Register(() => new BookRatingDto(
            _fixture.Create<decimal>(),
            _fixture.Create<decimal>(),
            _fixture.Create<BookRatingSource?>(),
            _fixture.Create<int?>()
        ));
        _fixture.Register(() => new WrittenContentMetadataDto(
             _fixture.Create<string>()[255..],
             LimitStringLength(_fixture.Create<string?>(), 255),
             LimitStringLength(_fixture.Create<string?>(), 2000),
             _fixture.Create<ReleaseInfoDto>(),
             _fixture.Create<List<GenreDto>>(),
             _fixture.Create<List<TagDto>>(),
             _fixture.Create<LanguageInfoDto?>(),
             _fixture.Create<LanguageInfoDto?>(),
             _fixture.Create<string?>(),
             _fixture.Create<int?>()
        ));
        _fixture.Register(() => new BookSeriesDto(
            _fixture.Create<string>()
        ));
        _fixture.Register(() => new MediaContributorRoleDto(
            _fixture.Create<string>(),
            _fixture.Create<string>()
        ));
        _fixture.Register(() => new MediaContributorNameDto(
            _fixture.Create<string>(),
            _fixture.Create<string?>()
        ));
        _fixture.Register(() => new MediaContributorDto(
            _fixture.Create<MediaContributorNameDto>(),
            _fixture.Create<MediaContributorRoleDto>()
        ));
    }

    private static string? LimitStringLength(string? input, int maxLength)
    {
        return input?.Length > maxLength ? input[..maxLength] : input;
    }
}
