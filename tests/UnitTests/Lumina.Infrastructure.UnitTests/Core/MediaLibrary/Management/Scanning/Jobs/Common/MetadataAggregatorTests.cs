#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Common;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.Management.Scanning.Jobs.Common;

/// <summary>
/// Contains unit tests for the <see cref="MetadataAggregator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MetadataAggregatorTests
{
    private readonly BookMetadataDtoFixture _bookMetadataDtoFixture = new();
    private readonly MediaContributorDtoFixture _mediaContributorDtoFixture = new();
    private readonly IsbnDtoFixture _isbnDtoFixture = new();
    private readonly BookRatingDtoFixture _bookRatingDtoFixture = new();
    private readonly ReleaseInfoDtoFixture _releaseInfoDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly TagDtoFixture _tagDtoFixture = new();
    private readonly LanguageInfoDtoFixture _languageInfoDtoFixture = new();

    [Fact]
    public void Merge_WhenBothMetadataHaveScalarValues_ShouldKeepTheFirstValues()
    {
        // Arrange
        BookMetadataDto first = _bookMetadataDtoFixture.Create(includeOptionalProperties: false, 
            title: "First Title",
            originalTitle: "First Original Title",
            description: "First Description",
            publisher: "First Publisher",
            pageCount: 100,
            format: BookFormat.Hardcover,
            edition: "First Edition",
            volumeNumber: 1,
            asin: "FIRST-ASIN",
            goodreadsId: "FIRST-GOODREADS",
            lccn: "FIRST-LCCN",
            oclcNumber: "FIRST-OCLC",
            openLibraryId: "FIRST-OL",
            libraryThingId: "FIRST-LT",
            googleBooksId: "FIRST-GOOGLE",
            barnesAndNobleId: "FIRST-BN",
            appleBooksId: "FIRST-APPLE",
            coverImagePath: "First Cover");
        BookMetadataDto second = _bookMetadataDtoFixture.Create(includeOptionalProperties: false, 
            title: "Second Title",
            originalTitle: "Second Original Title",
            description: "Second Description",
            publisher: "Second Publisher",
            pageCount: 200,
            format: BookFormat.Paperback,
            edition: "Second Edition",
            volumeNumber: 2,
            asin: "SECOND-ASIN",
            goodreadsId: "SECOND-GOODREADS",
            lccn: "SECOND-LCCN",
            oclcNumber: "SECOND-OCLC",
            openLibraryId: "SECOND-OL",
            libraryThingId: "SECOND-LT",
            googleBooksId: "SECOND-GOOGLE",
            barnesAndNobleId: "SECOND-BN",
            appleBooksId: "SECOND-APPLE",
            coverImagePath: "First Cover");

        // Act
        BookMetadataDto result = MetadataAggregator.Merge(first, second);

        // Assert
        Assert.Equal("First Title", result.Title);
        Assert.Equal("First Original Title", result.OriginalTitle);
        Assert.Equal("First Description", result.Description);
        Assert.Equal("First Publisher", result.Publisher);
        Assert.Equal(100, result.PageCount);
        Assert.Equal(BookFormat.Hardcover, result.Format);
        Assert.Equal("First Edition", result.Edition);
        Assert.Equal(1, result.VolumeNumber);
        Assert.Equal("FIRST-ASIN", result.ASIN);
        Assert.Equal("FIRST-GOODREADS", result.GoodreadsId);
        Assert.Equal("FIRST-LCCN", result.LCCN);
        Assert.Equal("FIRST-OCLC", result.OCLCNumber);
        Assert.Equal("FIRST-OL", result.OpenLibraryId);
        Assert.Equal("FIRST-LT", result.LibraryThingId);
        Assert.Equal("FIRST-GOOGLE", result.GoogleBooksId);
        Assert.Equal("FIRST-BN", result.BarnesAndNobleId);
        Assert.Equal("FIRST-APPLE", result.AppleBooksId);
        Assert.Equal("First Cover", result.CoverImagePath);
    }

    [Fact]
    public void Merge_WhenFirstHasNullScalars_ShouldFallBackToTheSecondValues()
    {
        // Arrange
        BookMetadataDto first = _bookMetadataDtoFixture.Create(includeOptionalProperties: false, title: null, description: null, publisher: null, pageCount: null);
        BookMetadataDto second = _bookMetadataDtoFixture.Create(includeOptionalProperties: false, title: "Second Title", description: "Second Description", publisher: "Second Publisher", pageCount: 200);

        // Act
        BookMetadataDto result = MetadataAggregator.Merge(first, second);

        // Assert
        Assert.Equal("Second Title", result.Title);
        Assert.Equal("Second Description", result.Description);
        Assert.Equal("Second Publisher", result.Publisher);
        Assert.Equal(200, result.PageCount);
    }

    [Fact]
    public void Merge_WhenBothHaveNullScalars_ShouldReturnNullScalars()
    {
        // Arrange
        BookMetadataDto first = _bookMetadataDtoFixture.Create(includeOptionalProperties: false, title: null, coverImagePath: null);
        BookMetadataDto second = _bookMetadataDtoFixture.Create(includeOptionalProperties: false, title: null, coverImagePath: null);

        // Act
        BookMetadataDto result = MetadataAggregator.Merge(first, second);

        // Assert
        Assert.Null(result.Title);
        Assert.Null(result.Description);
        Assert.Null(result.Publisher);
        Assert.Null(result.PageCount);
        Assert.Null(result.Format);
        Assert.Null(result.Edition);
        Assert.Null(result.VolumeNumber);
        Assert.Null(result.Series);
    }

    [Fact]
    public void Merge_WhenBothHaveReleaseInfo_ShouldCoalesceEachField()
    {
        // Arrange
        BookMetadataDto first = _bookMetadataDtoFixture.Create(title: "Title", coverImagePath: "First Cover", includeOptionalProperties: false);
        BookMetadataDto second = _bookMetadataDtoFixture.Create(title: "Title", coverImagePath: "First Cover", includeOptionalProperties: false);
        first = first with { ReleaseInfo = _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2001, 1, 1), originalReleaseYear: 2001, releaseCountry: "US") };
        second = second with { ReleaseInfo = _releaseInfoDtoFixture.Create(reReleaseDate: new DateOnly(2010, 5, 5), reReleaseYear: 2010, releaseCountry: "UK", releaseVersion: "2.0") };

        // Act
        BookMetadataDto result = MetadataAggregator.Merge(first, second);

        // Assert
        Assert.Equal(new DateOnly(2001, 1, 1), result.ReleaseInfo!.OriginalReleaseDate);
        Assert.Equal(2001, result.ReleaseInfo.OriginalReleaseYear);
        Assert.Equal(new DateOnly(2010, 5, 5), result.ReleaseInfo.ReReleaseDate);
        Assert.Equal(2010, result.ReleaseInfo.ReReleaseYear);
        Assert.Equal("US", result.ReleaseInfo.ReleaseCountry);
        Assert.Equal("2.0", result.ReleaseInfo.ReleaseVersion);
    }

    [Fact]
    public void Merge_WhenFirstReleaseInfoIsNull_ShouldUseTheSecondReleaseInfo()
    {
        // Arrange
        BookMetadataDto first = _bookMetadataDtoFixture.Create(title: "Title", coverImagePath: "First Cover", includeOptionalProperties: false) with { ReleaseInfo = null };
        BookMetadataDto second = _bookMetadataDtoFixture.Create(title: "Title", coverImagePath: "First Cover", includeOptionalProperties: false) with { ReleaseInfo = _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2001, 1, 1), originalReleaseYear: 2001) };

        // Act
        BookMetadataDto result = MetadataAggregator.Merge(first, second);

        // Assert
        Assert.Equal(new DateOnly(2001, 1, 1), result.ReleaseInfo!.OriginalReleaseDate);
    }

    [Fact]
    public void Merge_WhenBothReleaseInfosAreNull_ShouldReturnNullReleaseInfo()
    {
        // Arrange
        BookMetadataDto first = _bookMetadataDtoFixture.Create(title: "Title", coverImagePath: "First Cover", includeOptionalProperties: false) with { ReleaseInfo = null };
        BookMetadataDto second = _bookMetadataDtoFixture.Create(title: "Title", coverImagePath: "First Cover", includeOptionalProperties: false) with { ReleaseInfo = null };

        // Act
        BookMetadataDto result = MetadataAggregator.Merge(first, second);

        // Assert
        Assert.Null(result.ReleaseInfo);
    }

    [Fact]
    public void Merge_WhenBothHaveGenres_ShouldUnionAndDeDuplicateByName()
    {
        // Arrange
        BookMetadataDto first = _bookMetadataDtoFixture.Create(title: "Title", coverImagePath: "First Cover", includeOptionalProperties: false) with { Genres = [_genreDtoFixture.Create(name: "Science fiction"), _genreDtoFixture.Create(name: "Space opera")] };
        BookMetadataDto second = _bookMetadataDtoFixture.Create(title: "Title", coverImagePath: "First Cover", includeOptionalProperties: false) with { Genres = [_genreDtoFixture.Create(name: "Science fiction"), _genreDtoFixture.Create(name: "Fantasy")] };

        // Act
        BookMetadataDto result = MetadataAggregator.Merge(first, second);

        // Assert
        Assert.Equal(3, result.Genres!.Count);
        Assert.Contains(result.Genres, genre => genre.Name == "Science fiction");
        Assert.Contains(result.Genres, genre => genre.Name == "Space opera");
        Assert.Contains(result.Genres, genre => genre.Name == "Fantasy");
    }

    [Fact]
    public void Merge_WhenBothHaveTags_ShouldUnionAndDeDuplicateByName()
    {
        // Arrange
        BookMetadataDto first = _bookMetadataDtoFixture.Create(title: "Title", coverImagePath: "First Cover", includeOptionalProperties: false) with { Tags = [_tagDtoFixture.Create(name: "Tag A"), _tagDtoFixture.Create(name: "Tag B")] };
        BookMetadataDto second = _bookMetadataDtoFixture.Create(title: "Title", coverImagePath: "First Cover", includeOptionalProperties: false) with { Tags = [_tagDtoFixture.Create(name: "Tag A"), _tagDtoFixture.Create(name: "Tag C")] };

        // Act
        BookMetadataDto result = MetadataAggregator.Merge(first, second);

        // Assert
        Assert.Equal(3, result.Tags!.Count);
        Assert.Contains(result.Tags, tag => tag.Name == "Tag A");
        Assert.Contains(result.Tags, tag => tag.Name == "Tag B");
        Assert.Contains(result.Tags, tag => tag.Name == "Tag C");
    }

    [Fact]
    public void Merge_WhenBothHaveIsbns_ShouldUnionAndDeDuplicateByValue()
    {
        // Arrange
        BookMetadataDto first = _bookMetadataDtoFixture.Create(title: "Title", coverImagePath: "First Cover", includeOptionalProperties: false) with { Isbns = [_isbnDtoFixture.Create(value: "9780306406157", format: IsbnFormat.Isbn13)] };
        BookMetadataDto second = _bookMetadataDtoFixture.Create(title: "Title", coverImagePath: "First Cover", includeOptionalProperties: false) with { Isbns = [_isbnDtoFixture.Create(value: "9780306406157", format: IsbnFormat.Isbn13), _isbnDtoFixture.Create(value: "0306406152", format: IsbnFormat.Isbn10)] };

        // Act
        BookMetadataDto result = MetadataAggregator.Merge(first, second);

        // Assert
        Assert.Equal(2, result.Isbns!.Count);
        Assert.Contains(result.Isbns, isbn => isbn.Value == "9780306406157");
        Assert.Contains(result.Isbns, isbn => isbn.Value == "0306406152");
    }

    [Fact]
    public void Merge_WhenBothHaveContributors_ShouldUnionAndDeDuplicateByDisplayNameAndRole()
    {
        // Arrange
        BookMetadataDto first = _bookMetadataDtoFixture.Create(title: "Title", coverImagePath: "First Cover", includeOptionalProperties: false) with { Contributors = [_mediaContributorDtoFixture.Create("Author A", "Author")] };
        BookMetadataDto second = _bookMetadataDtoFixture.Create(title: "Title", coverImagePath: "First Cover", includeOptionalProperties: false) with { Contributors = [_mediaContributorDtoFixture.Create("Author A", "Author"), _mediaContributorDtoFixture.Create("Translator B", "Translator")] };

        // Act
        BookMetadataDto result = MetadataAggregator.Merge(first, second);

        // Assert
        Assert.Equal(2, result.Contributors!.Count);
        Assert.Contains(result.Contributors, contributor => contributor.Name!.DisplayName == "Author A");
        Assert.Contains(result.Contributors, contributor => contributor.Name!.DisplayName == "Translator B");
    }

    [Fact]
    public void Merge_WhenBothHaveRatings_ShouldUnionAndDeDuplicateBySource()
    {
        // Arrange
        BookMetadataDto first = _bookMetadataDtoFixture.Create(title: "Title", coverImagePath: "First Cover", includeOptionalProperties: false) with { Ratings = [_bookRatingDtoFixture.Create(value: 8m, maxValue: 10m, source: BookRatingSource.Calibre, voteCount: null)] };
        BookMetadataDto second = _bookMetadataDtoFixture.Create(title: "Title", coverImagePath: "First Cover", includeOptionalProperties: false) with { Ratings = [_bookRatingDtoFixture.Create(value: 8m, maxValue: 10m, source: BookRatingSource.Calibre, voteCount: null), _bookRatingDtoFixture.Create(value: 4.2m, maxValue: 5m, source: BookRatingSource.OpenLibrary, voteCount: 100)] };

        // Act
        BookMetadataDto result = MetadataAggregator.Merge(first, second);

        // Assert
        Assert.Equal(2, result.Ratings!.Count);
        Assert.Contains(result.Ratings, rating => rating.Source == BookRatingSource.Calibre);
        Assert.Contains(result.Ratings, rating => rating.Source == BookRatingSource.OpenLibrary);
    }

    [Fact]
    public void Merge_WhenOnlyFirstHasCollections_ShouldReturnTheFirstCollections()
    {
        // Arrange
        BookMetadataDto first = _bookMetadataDtoFixture.Create(title: "Title", coverImagePath: "First Cover", includeOptionalProperties: false) with { Tags = [_tagDtoFixture.Create(name: "Tag A")], Genres = [_genreDtoFixture.Create(name: "Genre A")], Isbns = [_isbnDtoFixture.Create(value: "9780306406157", format: IsbnFormat.Isbn13)], Contributors = [_mediaContributorDtoFixture.Create("Author A", "Author")], Ratings = [_bookRatingDtoFixture.Create(value: 8m, maxValue: 10m, source: BookRatingSource.Calibre, voteCount: null)] };
        BookMetadataDto second = _bookMetadataDtoFixture.Create(title: "Title", coverImagePath: "First Cover", includeOptionalProperties: false) with { Tags = null, Genres = null, Isbns = null, Contributors = null, Ratings = null };

        // Act
        BookMetadataDto result = MetadataAggregator.Merge(first, second);

        // Assert
        Assert.Equal("Tag A", Assert.Single(result.Tags!).Name);
        Assert.Equal("Genre A", Assert.Single(result.Genres!).Name);
        Assert.Equal("9780306406157", Assert.Single(result.Isbns!).Value);
        Assert.Equal("Author A", Assert.Single(result.Contributors!).Name!.DisplayName);
        Assert.Equal(BookRatingSource.Calibre, Assert.Single(result.Ratings!).Source);
    }

    [Fact]
    public void Merge_WhenOnlySecondHasCollections_ShouldReturnTheSecondCollections()
    {
        // Arrange
        BookMetadataDto first = _bookMetadataDtoFixture.Create(title: "Title", coverImagePath: "First Cover", includeOptionalProperties: false) with { Tags = null, Genres = null, Isbns = null, Contributors = null, Ratings = null };
        BookMetadataDto second = _bookMetadataDtoFixture.Create(title: "Title", coverImagePath: "First Cover", includeOptionalProperties: false) with { Tags = [_tagDtoFixture.Create(name: "Tag B")], Genres = [_genreDtoFixture.Create(name: "Genre B")], Isbns = [_isbnDtoFixture.Create(value: "0306406152", format: IsbnFormat.Isbn10)], Contributors = [_mediaContributorDtoFixture.Create("Author B", "Author")], Ratings = [_bookRatingDtoFixture.Create(value: 4.2m, maxValue: 5m, source: BookRatingSource.OpenLibrary, voteCount: 100)] };

        // Act
        BookMetadataDto result = MetadataAggregator.Merge(first, second);

        // Assert
        Assert.Equal("Tag B", Assert.Single(result.Tags!).Name);
        Assert.Equal("Genre B", Assert.Single(result.Genres!).Name);
        Assert.Equal("0306406152", Assert.Single(result.Isbns!).Value);
        Assert.Equal("Author B", Assert.Single(result.Contributors!).Name!.DisplayName);
        Assert.Equal(BookRatingSource.OpenLibrary, Assert.Single(result.Ratings!).Source);
    }

    [Fact]
    public void Merge_WhenBothHaveNullCollections_ShouldReturnNullCollections()
    {
        // Arrange
        BookMetadataDto first = _bookMetadataDtoFixture.Create(title: "Title", coverImagePath: "First Cover", includeOptionalProperties: false) with { Tags = null, Genres = null, Isbns = null, Contributors = null, Ratings = null };
        BookMetadataDto second = _bookMetadataDtoFixture.Create(title: "Title", coverImagePath: "First Cover", includeOptionalProperties: false) with { Tags = null, Genres = null, Isbns = null, Contributors = null, Ratings = null };

        // Act
        BookMetadataDto result = MetadataAggregator.Merge(first, second);

        // Assert
        Assert.Null(result.Genres);
        Assert.Null(result.Tags);
        Assert.Null(result.Isbns);
        Assert.Null(result.Contributors);
        Assert.Null(result.Ratings);
    }

    [Fact]
    public void Merge_WhenBothHaveLanguages_ShouldKeepTheFirstLanguage()
    {
        // Arrange
        BookMetadataDto first = _bookMetadataDtoFixture.Create(title: "Title", coverImagePath: "First Cover", includeOptionalProperties: false) with { Language = _languageInfoDtoFixture.Create(languageCode: "en", languageName: "English", nativeName: "English") };
        BookMetadataDto second = _bookMetadataDtoFixture.Create(title: "Title", coverImagePath: "First Cover", includeOptionalProperties: false) with { Language = _languageInfoDtoFixture.Create(languageCode: "de", languageName: "German", nativeName: "Deutsch") };

        // Act
        BookMetadataDto result = MetadataAggregator.Merge(first, second);

        // Assert
        Assert.Equal("en", result.Language!.LanguageCode);
    }
}
