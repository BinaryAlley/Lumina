#region ========================================================================= USING =====================================================================================
using AutoFixture;
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.UnitTests.Common.Setup;
using Lumina.Domain.Common.Enums.BookLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Commands.AddBook.Fixtures;

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
    }

    /// <summary>
    /// Creates a random valid book entity.
    /// </summary>
    /// <returns>The created book entity.</returns>
    public BookEntity CreateBook()
    {
        int releaseYear = _random.Next(2000, 2010);
        int reReleaseYear = _random.Next(2010, 2020);

        Faker<GenreEntity> genre = new Faker<GenreEntity>()
            .CustomInstantiator(f => new GenreEntity(
                default!
            ))
            .RuleFor(e => e.Name, f => f.Random.String2(f.Random.Number(1, 50)));

        Faker<TagEntity> tag = new Faker<TagEntity>()
            .CustomInstantiator(f => new TagEntity(
                default!
            ))
            .RuleFor(e => e.Name, f => f.Random.String2(f.Random.Number(1, 50)));

        Faker<BookRatingEntity> rating = new Faker<BookRatingEntity>()
           .CustomInstantiator(f => new BookRatingEntity(
                default,
                default,
                default,
                default
            ))
            .RuleFor(e => e.Value, _random.Next(1, 5))
           .RuleFor(e => e.MaxValue, 5)
           .RuleFor(e => e.Source, _fixture.Create<BookRatingSource>())
           .RuleFor(e => e.VoteCount, _random.Next(1, 1000));

        Faker<IsbnEntity> isbn = new Faker<IsbnEntity>()
            .CustomInstantiator(f => new IsbnEntity(
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

        return new BookEntity
        {
            Id = Guid.NewGuid(),
            Title = _faker.Random.String2(_faker.Random.Number(1, 255)),
            OriginalTitle = _faker.Random.Bool() ? _faker.Random.String2(_faker.Random.Number(1, 255)) : null,
            Description = _faker.Random.Bool() ? _faker.Random.String2(_faker.Random.Number(1, 2000)) : null,
            OriginalReleaseDate = _faker.DateOnlyBetween(new DateOnly(releaseYear, 1, 1), new DateOnly(releaseYear, 12, 31)),
            OriginalReleaseYear = releaseYear,
            ReReleaseDate = _faker.DateOnlyBetween(new DateOnly(reReleaseYear, 1, 1), new DateOnly(reReleaseYear, 12, 31)),
            ReReleaseYear = reReleaseYear,
            ReleaseCountry = _faker.Random.String2(2),
            ReleaseVersion = _faker.Random.Bool() ? _faker.Random.String2(_faker.Random.Number(1, 50)) : null,
            LanguageCode = _faker.Random.String2(2),
            LanguageName = _faker.Random.String2(_faker.Random.Number(1, 50)),
            LanguageNativeName = _faker.Random.Bool() ? _faker.Random.String2(_faker.Random.Number(1, 50)) : null,
            OriginalLanguageCode = _faker.Random.String2(2),
            OriginalLanguageName = _faker.Random.String2(_faker.Random.Number(1, 50)),
            OriginalLanguageNativeName = _faker.Random.Bool() ? _faker.Random.String2(_faker.Random.Number(1, 50)) : null,
            Tags = new HashSet<TagEntity>(tag.Generate(_faker.Random.Number(1, 5))),
            Genres = new HashSet<GenreEntity>(genre.Generate(_faker.Random.Number(1, 5))),
            Publisher = _faker.Random.Bool() ? _faker.Random.String2(_faker.Random.Number(1, 100)) : null,
            PageCount = _random.Next(100, 1000),
            Format = _faker.PickRandom<BookFormat>(),
            Edition = _faker.Random.Bool() ? _faker.Random.String2(_faker.Random.Number(1, 50)) : null,
            VolumeNumber = _faker.Random.Bool() ? _random.Next(1, 20) : null,
            ASIN = _faker.Random.String2(10),
            GoodreadsId = _random.Next(100000, 500000).ToString(),
            LCCN = _faker.Random.String2(_faker.Random.Number(8, 12)),
            OCLCNumber = _faker.Random.String2(_faker.Random.Number(8, 12)),
            OpenLibraryId = $"OL{_random.Next(1000000, 9999999)}M",
            LibraryThingId = _faker.Random.String2(_faker.Random.Number(1, 50)),
            GoogleBooksId = _faker.Random.String2(12),
            BarnesAndNobleId = _faker.Random.String2(10),
            AppleBooksId = $"id{_random.Next(1, 999999)}",
            ISBNs = isbn.Generate(_faker.Random.Number(1, 5)),
            Ratings = rating.Generate(_faker.Random.Number(1, 5)),
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid(),
            UpdatedOnUtc = _faker.Random.Bool() ? DateTime.UtcNow.AddDays(-_random.Next(1, 100)) : null,
            UpdatedBy = Guid.NewGuid()
        };
    }
}
