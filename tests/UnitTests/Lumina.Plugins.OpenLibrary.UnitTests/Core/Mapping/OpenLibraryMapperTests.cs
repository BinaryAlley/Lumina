#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Plugins.OpenLibrary.Common.Models.Contracts.Responses;
using Lumina.Plugins.OpenLibrary.Core.Mapping;
using Lumina.Plugins.OpenLibrary.Fixtures.Common.Models.Contracts.Responses;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
#endregion

namespace Lumina.Plugins.OpenLibrary.UnitTests.Core.Mapping;

/// <summary>
/// Contains unit tests for the <see cref="OpenLibraryMapper"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class OpenLibraryMapperTests
{
    private readonly BookMetadataLookupDtoFixture _bookMetadataLookupDtoFixture = new();
    private readonly OpenLibraryEditionResponseFixture _editionResponseFixture = new();
    private readonly OpenLibraryWorkResponseFixture _workResponseFixture = new();
    private readonly OpenLibrarySearchDocumentResponseFixture _searchDocumentResponseFixture = new();
    private readonly OpenLibraryAuthorResponseFixture _authorResponseFixture = new();
    private readonly OpenLibraryRatingsResponseFixture _ratingsResponseFixture = new();
    private readonly OpenLibraryRatingSummaryResponseFixture _ratingSummaryResponseFixture = new();
    private readonly OpenLibraryKeyReferenceResponseFixture _keyReferenceResponseFixture = new();
    private readonly OpenLibraryWorkAuthorResponseFixture _workAuthorResponseFixture = new();

    [Theory]
    [InlineData("978-0-306-40615-7", "9780306406157")]
    [InlineData("0-306-40615-2", "0306406152")]
    [InlineData("978 0 306 40615 7", "9780306406157")]
    [InlineData("9780306406157", "9780306406157")]
    [InlineData("0-306-40615-X", "030640615X")]
    [InlineData("0-306-40615-x", "030640615X")]
    public void NormalizeIsbn_WhenIsbnIsValid_ShouldReturnTheCanonicalForm(string isbn, string expected)
    {
        // Act
        string result = OpenLibraryMapper.NormalizeIsbn(isbn);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]
    [InlineData("12345678901")]
    [InlineData("1234567890123456")]
    [InlineData("ABCDEFGHIJ")]
    [InlineData("978030640615A")]
    public void NormalizeIsbn_WhenIsbnIsInvalid_ShouldThrowArgumentException(string isbn)
    {
        // Act
        Action act = () => OpenLibraryMapper.NormalizeIsbn(isbn);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData("/works/OL12345W", "W", "OL12345W")]
    [InlineData("/books/OL1M", "M", "OL1M")]
    [InlineData("OL999W", "W", "OL999W")]
    [InlineData("https://openlibrary.org/works/OL2W", "W", "OL2W")]
    [InlineData("/works/ol3w", "W", "OL3W")]
    [InlineData("/works/OL123W/", "W", "OL123W")]
    public void ExtractOlid_WhenKeyContainsAMatchingOlid_ShouldReturnTheCanonicalOlid(string key, string expectedSuffix, string expected)
    {
        // Act
        string? result = OpenLibraryMapper.ExtractOlid(key, expectedSuffix[0]);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, "W")]
    [InlineData("", "W")]
    [InlineData("   ", "W")]
    [InlineData("/works/OL1A", "W")]
    [InlineData("/works/not-an-olid", "W")]
    [InlineData("OL1", "W")]
    [InlineData("/books/OL1M", "W")]
    public void ExtractOlid_WhenKeyDoesNotContainAMatchingOlid_ShouldReturnNull(string? key, string expectedSuffix)
    {
        // Act
        string? result = OpenLibraryMapper.ExtractOlid(key, expectedSuffix[0]);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("Mass Market Paperback", BookFormat.MassMarketPaperback)]
    [InlineData("mass market paperback", BookFormat.MassMarketPaperback)]
    [InlineData("Trade Paperback", BookFormat.TradePaperback)]
    [InlineData("Paperback", BookFormat.Paperback)]
    [InlineData("Softcover", BookFormat.Paperback)]
    [InlineData("Hardcover", BookFormat.Hardcover)]
    [InlineData("Hard cover", BookFormat.Hardcover)]
    [InlineData("Hardback", BookFormat.Hardcover)]
    [InlineData("Clothbound", BookFormat.Hardcover)]
    [InlineData("eBook", BookFormat.eBook)]
    [InlineData("E-book", BookFormat.eBook)]
    [InlineData("Electronic", BookFormat.eBook)]
    [InlineData("Audiobook", BookFormat.Audiobook)]
    [InlineData("Audio Book", BookFormat.Audiobook)]
    [InlineData("Audio CD", BookFormat.Audiobook)]
    [InlineData("Large Print", BookFormat.LargePrint)]
    [InlineData("Board Book", BookFormat.BoardBook)]
    [InlineData("Spiral", BookFormat.SpiralBound)]
    [InlineData("Library Binding", BookFormat.LibraryBinding)]
    [InlineData("Leather", BookFormat.LeatherBound)]
    [InlineData("Pop-up", BookFormat.PopupBook)]
    [InlineData("Popup", BookFormat.PopupBook)]
    public void MapFormat_WhenPhysicalFormatIsRecognized_ShouldReturnTheMappedBookFormat(string physicalFormat, BookFormat expected)
    {
        // Act
        BookFormat? result = OpenLibraryMapper.MapFormat(physicalFormat);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("3D Printed")]
    [InlineData("Unknown Format")]
    public void MapFormat_WhenPhysicalFormatIsNotRecognized_ShouldReturnNull(string? physicalFormat)
    {
        // Act
        BookFormat? result = OpenLibraryMapper.MapFormat(physicalFormat);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void MapSearchCandidate_WhenCalledWithASearchDocument_ShouldMapTheDocumentFields()
    {
        // Arrange
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create();
        OpenLibrarySearchDocumentResponse document = _searchDocumentResponseFixture.Create(
            key: "/works/OL12345W",
            title: "Search Title",
            authorNames: ["Search Author"],
            firstPublishYear: 2003,
            publishers: ["Search Publisher"],
            publishPlaces: ["London"],
            subjects: ["Science fiction", "Space"],
            isbns: ["978-0-306-40615-7"],
            numberOfPagesMedian: 250,
            ratingsAverage: 4.1m,
            ratingsCount: 42,
            lccn: ["79042755"],
            oclc: ["123456789"]);

        // Act
        AddBookRequest result = OpenLibraryMapper.MapSearchCandidate(lookup, document);

        // Assert
        Assert.Equal(lookup.LibraryId, result.LibraryId);
        Assert.Equal(lookup.Path, result.Path);
        Assert.Equal("Search Title", result.Metadata!.Title);
        Assert.Equal(2003, result.Metadata.ReleaseInfo!.OriginalReleaseYear);
        Assert.Equal("Search Publisher", result.Metadata.Publisher);
        Assert.Equal(250, result.Metadata.PageCount);
        Assert.Contains(result.Metadata.Genres!, genre => genre.Name == "Science fiction");
        Assert.DoesNotContain(result.Metadata.Genres!, genre => genre.Name == "Space");
        Assert.Contains(result.Metadata.Tags!, tag => tag.Name == "Science fiction");
        Assert.Contains(result.Metadata.Tags!, tag => tag.Name == "Space");
        Assert.Equal("London", result.Metadata.ReleaseInfo.ReleaseCountry);
        BookRatingDto rating = Assert.Single(result.Ratings!);
        Assert.Equal(4.1m, rating.Value);
        Assert.Equal(5m, rating.MaxValue);
        Assert.Equal(BookRatingSource.OpenLibrary, rating.Source);
        Assert.Equal(42, rating.VoteCount);
        Assert.Equal("79042755", result.LCCN);
        Assert.Equal("123456789", result.OCLCNumber);
        Assert.Equal("OL12345W", result.OpenLibraryId);
        Assert.Contains(result.ISBNs!, isbn => isbn.Value == "9780306406157" && isbn.Format == IsbnFormat.Isbn13);
        Assert.Contains(result.Contributors!, contributor => contributor.Name!.DisplayName == "Search Author" && contributor.Role!.Name == "Author");
    }

    [Fact]
    public void MapDetailed_WhenEditionAndWorkAreProvided_ShouldMapAllBookRequestFields()
    {
        // Arrange
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(isbn: "978-0-306-40615-7");
        JsonElement identifiers = JsonDocument.Parse("""{"amazon":["B000001"],"goodreads":["12345"],"google":["gid"],"librarything":["ltid"],"barnesandnoble":["bnid"],"apple":["apid"]}""").RootElement;
        OpenLibraryEditionResponse edition = _editionResponseFixture.Create(
            key: "/books/OL100M",
            title: "Edition Title",
            publishDate: "2010-05-01",
            publishers: ["Publisher Co"],
            publishPlaces: ["New York"],
            numberOfPages: 320,
            physicalFormat: "Hardcover",
            editionName: "First Edition",
            series: ["The Series"],
            volume: "Vol. 3",
            isbn10: ["0306406152"],
            isbn13: ["9780306406157"],
            lccn: ["79042755"],
            oclcNumbers: ["1234567"],
            identifiers: identifiers,
            languages: [_keyReferenceResponseFixture.Create(key: "/languages/eng")],
            works: [_keyReferenceResponseFixture.Create(key: "/works/OL200W")],
            contributions: ["John Smith (Illustrator)"]);
        OpenLibraryWorkResponse work = _workResponseFixture.Create(
            key: "/works/OL200W",
            title: "Work Title",
            originalTitle: "Original Title",
            description: JsonDocument.Parse("""{"value":"A work description"}""").RootElement,
            firstPublishDate: "1987-06-01",
            subjects: ["Science fiction", "History", "Space"],
            genres: ["Fantasy"],
            authors: [_workAuthorResponseFixture.Create(author: _keyReferenceResponseFixture.Create(key: "/authors/OL1A"))],
            originalLanguages: [_keyReferenceResponseFixture.Create(key: "/languages/fre")]);
        OpenLibraryAuthorResponse author = _authorResponseFixture.Create(key: "/authors/OL1A", name: "Test Author", personalName: "Test Personal Name");
        OpenLibraryRatingsResponse ratings = _ratingsResponseFixture.Create(_ratingSummaryResponseFixture.Create(average: 4.2m, count: 100));

        // Act
        AddBookRequest result = OpenLibraryMapper.MapDetailed(lookup, edition, work, [author], ratings);

        // Assert
        Assert.Equal(lookup.LibraryId, result.LibraryId);
        Assert.Equal(lookup.Path, result.Path);
        Assert.Equal("Edition Title", result.Metadata!.Title);
        Assert.Equal("Original Title", result.Metadata.OriginalTitle);
        Assert.Equal("A work description", result.Metadata.Description);
        Assert.Equal(new DateOnly(1987, 6, 1), result.Metadata.ReleaseInfo!.OriginalReleaseDate);
        Assert.Equal(1987, result.Metadata.ReleaseInfo.OriginalReleaseYear);
        Assert.Equal(new DateOnly(2010, 5, 1), result.Metadata.ReleaseInfo.ReReleaseDate);
        Assert.Equal(2010, result.Metadata.ReleaseInfo.ReReleaseYear);
        Assert.Equal("New York", result.Metadata.ReleaseInfo.ReleaseCountry);
        Assert.Equal("First Edition", result.Metadata.ReleaseInfo.ReleaseVersion);
        GenreDto genre = Assert.Single(result.Metadata.Genres!);
        Assert.Equal("Fantasy", genre.Name);
        Assert.Equal(3, result.Metadata.Tags!.Count);
        Assert.Contains(result.Metadata.Tags, tag => tag.Name == "Science fiction");
        Assert.Contains(result.Metadata.Tags, tag => tag.Name == "History");
        Assert.Contains(result.Metadata.Tags, tag => tag.Name == "Space");
        Assert.Equal("en", result.Metadata.Language!.LanguageCode);
        Assert.False(string.IsNullOrWhiteSpace(result.Metadata.Language.LanguageName));
        Assert.Equal("fre", result.Metadata.OriginalLanguage!.LanguageCode);
        Assert.Equal("fre", result.Metadata.OriginalLanguage.LanguageName);
        Assert.Null(result.Metadata.OriginalLanguage.NativeName);
        Assert.Equal("Publisher Co", result.Metadata.Publisher);
        Assert.Equal(320, result.Metadata.PageCount);
        Assert.Equal(BookFormat.Hardcover, result.Format);
        Assert.Equal("First Edition", result.Edition);
        Assert.Equal(3, result.VolumeNumber);
        Assert.Equal("The Series", result.Series!.Title);
        Assert.Equal("B000001", result.ASIN);
        Assert.Equal("12345", result.GoodreadsId);
        Assert.Equal("79042755", result.LCCN);
        Assert.Equal("1234567", result.OCLCNumber);
        Assert.Equal("OL100M", result.OpenLibraryId);
        Assert.Equal("ltid", result.LibraryThingId);
        Assert.Equal("gid", result.GoogleBooksId);
        Assert.Equal("bnid", result.BarnesAndNobleId);
        Assert.Equal("apid", result.AppleBooksId);
        Assert.Contains(result.ISBNs!, isbn => isbn.Value == "9780306406157" && isbn.Format == IsbnFormat.Isbn13);
        Assert.Contains(result.ISBNs!, isbn => isbn.Value == "0306406152" && isbn.Format == IsbnFormat.Isbn10);
        Assert.Contains(result.Contributors!, contributor => contributor.Name!.DisplayName == "Test Author" && contributor.Name.LegalName == "Test Personal Name" && contributor.Role!.Name == "Author");
        Assert.Contains(result.Contributors!, contributor => contributor.Name!.DisplayName == "John Smith" && contributor.Role!.Name == "Illustrator" && contributor.Role.Category == "Art");
        BookRatingDto mappedRating = Assert.Single(result.Ratings!);
        Assert.Equal(4.2m, mappedRating.Value);
        Assert.Equal(5m, mappedRating.MaxValue);
        Assert.Equal(BookRatingSource.OpenLibrary, mappedRating.Source);
        Assert.Equal(100, mappedRating.VoteCount);
    }

    [Fact]
    public void MapDetailed_WhenWorkHasNoExplicitGenres_ShouldDeriveGenresFromTheGenreLikeSubjects()
    {
        // Arrange
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create();
        OpenLibraryWorkResponse work = _workResponseFixture.Create(subjects: ["Science fiction", "History", "Space"]);

        // Act
        AddBookRequest result = OpenLibraryMapper.MapDetailed(lookup, null, work, [], null);

        // Assert
        Assert.Equal(2, result.Metadata!.Genres!.Count);
        Assert.Contains(result.Metadata.Genres, genre => genre.Name == "Science fiction");
        Assert.Contains(result.Metadata.Genres, genre => genre.Name == "History");
        Assert.DoesNotContain(result.Metadata.Genres, genre => genre.Name == "Space");
    }

    [Fact]
    public void MapDetailed_WhenWorkHasNoFirstPublishDate_ShouldUseTheFallbackFirstPublishYear()
    {
        // Arrange
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create();
        OpenLibrarySearchDocumentResponse fallback = _searchDocumentResponseFixture.Create(firstPublishYear: 2001);

        // Act
        AddBookRequest result = OpenLibraryMapper.MapDetailed(lookup, null, null, [], null, fallback);

        // Assert
        Assert.Equal(2001, result.Metadata!.ReleaseInfo!.OriginalReleaseYear);
        Assert.Null(result.Metadata.ReleaseInfo.OriginalReleaseDate);
    }

    [Fact]
    public void MapDetailed_WhenLanguageKeyIsUnknown_ShouldUseTheRawCodeAsTheLanguage()
    {
        // Arrange
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create();
        OpenLibraryEditionResponse edition = _editionResponseFixture.Create(languages: [_keyReferenceResponseFixture.Create(key: "/languages/xxq")]);

        // Act
        AddBookRequest result = OpenLibraryMapper.MapDetailed(lookup, edition, null, [], null);

        // Assert
        Assert.NotNull(result.Metadata!.Language);
        Assert.Equal("xxq", result.Metadata.Language!.LanguageCode);
        Assert.Equal("xxq", result.Metadata.Language.LanguageName);
        Assert.Null(result.Metadata.Language.NativeName);
    }

    [Fact]
    public void MapDetailed_WhenDescriptionIsAPlainString_ShouldReadItDirectly()
    {
        // Arrange
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create();
        OpenLibraryWorkResponse work = _workResponseFixture.Create(description: JsonDocument.Parse("\"A plain description\"").RootElement);

        // Act
        AddBookRequest result = OpenLibraryMapper.MapDetailed(lookup, null, work, [], null);

        // Assert
        Assert.Equal("A plain description", result.Metadata!.Description);
    }

    [Fact]
    public void MapDetailed_WhenNoExternalDataIsAvailable_ShouldMapTheLookupItselfAndLeaveTheRestNull()
    {
        // Arrange
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(isbn: "978-0-306-40615-7");

        // Act
        AddBookRequest result = OpenLibraryMapper.MapDetailed(lookup, null, null, [], null, null);

        // Assert
        Assert.Equal(lookup.LibraryId, result.LibraryId);
        Assert.Equal(lookup.Path, result.Path);
        Assert.NotNull(result.Metadata);
        Assert.Null(result.Metadata!.Title);
        Assert.Null(result.Format);
        Assert.Null(result.Edition);
        Assert.Null(result.OpenLibraryId);
        Assert.Contains(result.ISBNs!, isbn => isbn.Value == "9780306406157");
        Assert.Empty(result.Ratings!);
        Assert.Empty(result.Contributors!);
    }

    [Fact]
    public void MapDetailed_WhenRatingsAreOnlyAvailableInTheFallbackDocument_ShouldUseTheFallbackRatings()
    {
        // Arrange
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create();
        OpenLibrarySearchDocumentResponse fallback = _searchDocumentResponseFixture.Create(ratingsAverage: 3.5m, ratingsCount: 10);

        // Act
        AddBookRequest result = OpenLibraryMapper.MapDetailed(lookup, null, null, [], null, fallback);

        // Assert
        BookRatingDto rating = Assert.Single(result.Ratings!);
        Assert.Equal(3.5m, rating.Value);
        Assert.Equal(10, rating.VoteCount);
    }

    [Fact]
    public void MapDetailed_WhenVolumeIsContainedInTheEditionSeries_ShouldParseTheVolumeNumber()
    {
        // Arrange
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create();
        OpenLibraryEditionResponse edition = _editionResponseFixture.Create(series: ["Book 2 of the trilogy"]);

        // Act
        AddBookRequest result = OpenLibraryMapper.MapDetailed(lookup, edition, null, [], null);

        // Assert
        Assert.Equal(2, result.VolumeNumber);
        Assert.Equal("Book 2 of the trilogy", result.Series!.Title);
    }

    [Fact]
    public void MapDetailed_WhenExternalIdsComeFromSourceRecordsAndFallback_ShouldMapThem()
    {
        // Arrange
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create();
        OpenLibraryEditionResponse edition = _editionResponseFixture.Create(
            sourceRecords: ["amazon:B000002", "google:gid2"],
            oclcNumbers: ["999999"],
            isbn13: ["9780306406157"]);
        OpenLibrarySearchDocumentResponse fallback = _searchDocumentResponseFixture.Create(
            amazonIds: ["B000003"],
            goodreadsIds: ["777"],
            googleIds: ["gid3"],
            libraryThingIds: ["lt3"],
            lccn: ["79042756"],
            oclc: ["888888"]);

        // Act
        AddBookRequest result = OpenLibraryMapper.MapDetailed(lookup, edition, null, [], null, fallback);

        // Assert
        Assert.Equal("B000002", result.ASIN);
        Assert.Equal("777", result.GoodreadsId);
        Assert.Equal("gid2", result.GoogleBooksId);
        Assert.Equal("lt3", result.LibraryThingId);
        Assert.Equal("79042756", result.LCCN);
        Assert.Equal("999999", result.OCLCNumber);
    }
}
