#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Plugins.OpenLibrary.Common.Models.DTO.Settings;
using Lumina.Plugins.OpenLibrary.Core;
using Lumina.Plugins.OpenLibrary.Core.Api;
using Lumina.Plugins.OpenLibrary.Core.Settings;
using Lumina.Plugins.OpenLibrary.Fixtures.Common.Models.DTO.Settings;
using Lumina.Plugins.OpenLibrary.Fixtures.Common.Setup;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Plugins.OpenLibrary.UnitTests.Core;

/// <summary>
/// Contains unit tests for the <see cref="OpenLibraryBookMetadataProvider"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class OpenLibraryBookMetadataProviderTests
{
    private readonly OpenLibrarySettingsDtoFixture _settingsFixture = new();
    private readonly BookMetadataLookupDtoFixture _bookMetadataLookupDtoFixture = new();

    private const string EDITION_JSON = """{"key":"/books/OL100M","title":"Edition Title","publish_date":"2010-05-01","number_of_pages":320,"physical_format":"Hardcover","edition_name":"First Edition","series":["The Series"],"volume":"Vol. 3","publishers":["Publisher Co"],"isbn_13":["9780306406157"],"languages":[{"key":"/languages/eng"}],"works":[{"key":"/works/OL200W"}]}""";
    private const string WORK_JSON = """{"key":"/works/OL200W","title":"Work Title","first_publish_date":"2001-01-01","description":{"value":"A work description"},"subjects":["Science fiction"],"authors":[{"author":{"key":"/authors/OL1A"}}]}""";
    private const string AUTHOR_JSON = """{"key":"/authors/OL1A","name":"Test Author"}""";
    private const string RATINGS_JSON = """{"summary":{"average":4.2,"count":100}}""";
    private const string EDITIONS_JSON = """{"entries":[{"key":"/books/OL100M","title":"Edition Title","languages":[{"key":"/languages/eng"}]}]}""";

    [Fact]
    public void Name_WhenCalled_ShouldReturnTheProviderDisplayName()
    {
        // Arrange
        OpenLibraryBookMetadataProvider sut = CreateProvider(new StubOpenLibraryHttpMessageHandler());

        // Act
        string result = sut.Name;

        // Assert
        Assert.Equal("Open Library", result);
    }

    [Fact]
    public void SupportedLibraryTypes_WhenCalled_ShouldReturnBook()
    {
        // Arrange
        OpenLibraryBookMetadataProvider sut = CreateProvider(new StubOpenLibraryHttpMessageHandler());

        // Act
        IReadOnlyList<LibraryType> result = sut.SupportedLibraryTypes;

        // Assert
        Assert.Single(result);
        Assert.Equal(LibraryType.Book, result[0]);
    }

    [Fact]
    public void RequiresWebAccess_WhenCalled_ShouldReturnTrue()
    {
        // Arrange
        OpenLibraryBookMetadataProvider sut = CreateProvider(new StubOpenLibraryHttpMessageHandler());

        // Act
        bool result = sut.RequiresWebAccess;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetSearchResultsAsync_WhenLookupHasAnIsbn_ShouldResolveTheExactEditionWithoutSearching()
    {
        // Arrange
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(
            isbn: "978-0-306-40615-7",
            openLibraryId: null,
            title: null,
            author: null,
            languageCode: null);
        StubOpenLibraryHttpMessageHandler handler = new();
        handler.MapPath("/isbn/9780306406157.json", EDITION_JSON);
        handler.MapPath("/works/OL200W.json", WORK_JSON);
        handler.MapPath("/authors/OL1A.json", AUTHOR_JSON);
        handler.MapPath("/works/OL200W/ratings.json", RATINGS_JSON);
        OpenLibraryBookMetadataProvider sut = CreateProvider(handler);

        // Act
        IReadOnlyList<BookMetadataDto> result = await sut.GetSearchResultsAsync(lookup, CancellationToken.None);

        // Assert
        BookMetadataDto metadata = Assert.IsType<BookMetadataDto>(Assert.Single(result));
        Assert.Equal("Edition Title", metadata.Title);
        Assert.Equal("Publisher Co", metadata.Publisher);
        Assert.Equal(320, metadata.PageCount);
        Assert.Equal(BookFormat.Hardcover, metadata.Format);
        Assert.Equal("First Edition", metadata.Edition);
        Assert.Equal(3, metadata.VolumeNumber);
        Assert.Equal("The Series", metadata.Series!.Title);
        Assert.Equal("OL100M", metadata.OpenLibraryId);
        Assert.Contains(metadata.Isbns!, isbn => isbn.Value == "9780306406157");
        Assert.DoesNotContain(handler.Requests, request => request.RequestUri!.AbsolutePath == "/search.json");
    }

    [Fact]
    public async Task GetSearchResultsAsync_WhenLookupHasAnIsbnButNoMatchExists_ShouldReturnAnEmptyList()
    {
        // Arrange
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(
            isbn: "978-0-306-40615-7",
            openLibraryId: null,
            title: null,
            author: null,
            languageCode: null);
        OpenLibraryBookMetadataProvider sut = CreateProvider(new StubOpenLibraryHttpMessageHandler());

        // Act
        IReadOnlyList<BookMetadataDto> result = await sut.GetSearchResultsAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetSearchResultsAsync_WhenLookupHasAWorkOpenLibraryId_ShouldResolveTheWorkAndItsBestEdition()
    {
        // Arrange
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(
            isbn: null,
            openLibraryId: "OL200W",
            title: null,
            author: null,
            languageCode: null);
        StubOpenLibraryHttpMessageHandler handler = new();
        handler.MapPath("/works/OL200W.json", WORK_JSON);
        handler.MapPath("/works/OL200W/editions.json", EDITIONS_JSON);
        handler.MapPath("/authors/OL1A.json", AUTHOR_JSON);
        handler.MapPath("/works/OL200W/ratings.json", RATINGS_JSON);
        OpenLibraryBookMetadataProvider sut = CreateProvider(handler);

        // Act
        IReadOnlyList<BookMetadataDto> result = await sut.GetSearchResultsAsync(lookup, CancellationToken.None);

        // Assert
        BookMetadataDto metadata = Assert.IsType<BookMetadataDto>(Assert.Single(result));
        Assert.Equal("Edition Title", metadata.Title);
        Assert.Equal("OL100M", metadata.OpenLibraryId);
        Assert.Contains(handler.Requests, request => request.RequestUri!.AbsolutePath == "/works/OL200W/editions.json");
        Assert.Contains(handler.Requests, request => request.RequestUri!.AbsolutePath == "/works/OL200W/ratings.json");
    }

    [Fact]
    public async Task GetSearchResultsAsync_WhenLookupHasOnlyATitle_ShouldSearchAndMapTheSearchCandidates()
    {
        // Arrange
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(path: "/books/title.epub", title: "Search Title");
        StubOpenLibraryHttpMessageHandler handler = new();
        handler.MapPath("/search.json", """{"docs":[{"key":"/works/OL1W","title":"Search Title","author_name":["Search Author"],"first_publish_year":2001,"publisher":["Search Publisher"],"subject":["Science fiction"],"isbn":["9780306406157"],"number_of_pages_median":250},{"key":"/works/OL999W"}]}""");
        OpenLibraryBookMetadataProvider sut = CreateProvider(handler);

        // Act
        IReadOnlyList<BookMetadataDto> result = await sut.GetSearchResultsAsync(lookup, CancellationToken.None);

        // Assert
        BookMetadataDto metadata = Assert.IsType<BookMetadataDto>(Assert.Single(result));
        Assert.Equal("Search Title", metadata.Title);
        Assert.Equal(2001, metadata.ReleaseInfo!.OriginalReleaseYear);
        Assert.Equal("Search Publisher", metadata.Publisher);
        Assert.Equal(250, metadata.PageCount);
        Assert.Contains(handler.Requests, request => request.RequestUri!.AbsolutePath == "/search.json");
    }

    [Fact]
    public async Task GetSearchResultsAsync_WhenSearchReturnsNoResults_ShouldReturnAnEmptyList()
    {
        // Arrange
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(path: "/books/title.epub", title: "Search Title");
        StubOpenLibraryHttpMessageHandler handler = new();
        handler.MapPath("/search.json", """{"docs":[]}""");
        OpenLibraryBookMetadataProvider sut = CreateProvider(handler);

        // Act
        IReadOnlyList<BookMetadataDto> result = await sut.GetSearchResultsAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetSearchResultsAsync_WhenLookupIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        OpenLibraryBookMetadataProvider sut = CreateProvider(new StubOpenLibraryHttpMessageHandler());

        // Act
        async Task Act()
        {
            await sut.GetSearchResultsAsync(null!, CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(Act);
    }

    [Fact]
    public async Task GetSearchResultsAsync_WhenLookupHasAnEmptyLibraryId_ShouldThrowArgumentException()
    {
        // Arrange
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(libraryId: Guid.Empty);
        OpenLibraryBookMetadataProvider sut = CreateProvider(new StubOpenLibraryHttpMessageHandler());

        // Act
        async Task Act()
        {
            await sut.GetSearchResultsAsync(lookup, CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(Act);
    }

    [Fact]
    public async Task GetSearchResultsAsync_WhenLookupHasNoPath_ShouldThrowArgumentNullException()
    {
        // Arrange
        BookMetadataLookupDto lookup = new(Guid.NewGuid(), null!, Isbn: "978-0-306-40615-7");
        OpenLibraryBookMetadataProvider sut = CreateProvider(new StubOpenLibraryHttpMessageHandler());

        // Act
        async Task Act()
        {
            await sut.GetSearchResultsAsync(lookup, CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(Act);
    }

    [Fact]
    public async Task GetSearchResultsAsync_WhenLookupPathIsWhiteSpace_ShouldThrowArgumentException()
    {
        // Arrange
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(path: "   ", isbn: "978-0-306-40615-7");
        OpenLibraryBookMetadataProvider sut = CreateProvider(new StubOpenLibraryHttpMessageHandler());

        // Act
        async Task Act()
        {
            await sut.GetSearchResultsAsync(lookup, CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(Act);
    }

    [Fact]
    public async Task GetSearchResultsAsync_WhenLookupHasNoSearchCriteria_ShouldThrowArgumentException()
    {
        // Arrange
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(path: "/books/title.epub", isbn: null, openLibraryId: null, title: null, author: null, languageCode: null);
        OpenLibraryBookMetadataProvider sut = CreateProvider(new StubOpenLibraryHttpMessageHandler());

        // Act
        async Task Act()
        {
            await sut.GetSearchResultsAsync(lookup, CancellationToken.None);
        }

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(Act);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenAnExactMatchExists_ShouldReturnTheMappedMetadata()
    {
        // Arrange
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(
            isbn: "978-0-306-40615-7",
            openLibraryId: null,
            title: null,
            author: null,
            languageCode: null);
        StubOpenLibraryHttpMessageHandler handler = new();
        handler.MapPath("/isbn/9780306406157.json", EDITION_JSON);
        handler.MapPath("/works/OL200W.json", WORK_JSON);
        handler.MapPath("/authors/OL1A.json", AUTHOR_JSON);
        handler.MapPath("/works/OL200W/ratings.json", RATINGS_JSON);
        OpenLibraryBookMetadataProvider sut = CreateProvider(handler);

        // Act
        BookMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Edition Title", result!.Title);
    }

    [Fact]
    public async Task GetMetadataAsync_WhenNoMatchExists_ShouldReturnNull()
    {
        // Arrange
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(
            isbn: "978-0-306-40615-7",
            openLibraryId: null,
            title: null,
            author: null,
            languageCode: null);
        OpenLibraryBookMetadataProvider sut = CreateProvider(new StubOpenLibraryHttpMessageHandler());

        // Act
        BookMetadataDto? result = await sut.GetMetadataAsync(lookup, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetBookAsync_WhenLookupHasAnEditionOpenLibraryId_ShouldResolveTheEditionWorkAuthorsAndRatings()
    {
        // Arrange
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(
            isbn: null,
            openLibraryId: "OL100M",
            title: null,
            author: null,
            languageCode: null);
        StubOpenLibraryHttpMessageHandler handler = new();
        handler.MapPath("/books/OL100M.json", EDITION_JSON);
        handler.MapPath("/works/OL200W.json", WORK_JSON);
        handler.MapPath("/authors/OL1A.json", AUTHOR_JSON);
        handler.MapPath("/works/OL200W/ratings.json", RATINGS_JSON);
        OpenLibraryBookMetadataProvider sut = CreateProvider(handler);

        // Act
        AddBookRequest? result = await sut.GetBookAsync(lookup);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Edition Title", result!.Metadata!.Title);
        Assert.Equal("A work description", result.Metadata.Description);
        Assert.Equal("OL100M", result.OpenLibraryId);
        Assert.Equal(BookFormat.Hardcover, result.Format);
        Assert.Equal(3, result.VolumeNumber);
        Assert.Contains(result.Contributors!, contributor => contributor.Name!.DisplayName == "Test Author");
        Assert.Contains(result.Ratings!, rating => rating.Value == 4.2m);
    }

    [Fact]
    public async Task GetBookAsync_WhenLookupHasAWorkOpenLibraryIdAndNoMatchingEditions_ShouldReturnABookFromTheWorkDataOnly()
    {
        // Arrange
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(
            isbn: null,
            openLibraryId: "OL200W",
            title: null,
            author: null,
            languageCode: null);
        StubOpenLibraryHttpMessageHandler handler = new();
        handler.MapPath("/works/OL200W.json", WORK_JSON);
        handler.MapPath("/works/OL200W/editions.json", """{"entries":[]}""");
        handler.MapPath("/authors/OL1A.json", AUTHOR_JSON);
        handler.MapPath("/works/OL200W/ratings.json", RATINGS_JSON);
        OpenLibraryBookMetadataProvider sut = CreateProvider(handler);

        // Act
        AddBookRequest? result = await sut.GetBookAsync(lookup);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Work Title", result!.Metadata!.Title);
        Assert.Equal("OL200W", result.OpenLibraryId);
    }

    [Fact]
    public async Task GetBookAsync_WhenSearchResultHasOnlyAnEditionKey_ShouldResolveTheEditionByItsFirstEditionKey()
    {
        // Arrange
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(path: "/books/title.epub", title: "Search Title");
        StubOpenLibraryHttpMessageHandler handler = new();
        handler.MapPath("/search.json", """{"docs":[{"key":"/books/OL500M","edition_key":["OL600M"]}]}""");
        handler.MapPath("/books/OL600M.json", """{"key":"/books/OL600M","title":"Fallback Edition"}""");
        OpenLibraryBookMetadataProvider sut = CreateProvider(handler);

        // Act
        AddBookRequest? result = await sut.GetBookAsync(lookup);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Fallback Edition", result!.Metadata!.Title);
        Assert.Equal("OL600M", result.OpenLibraryId);
        Assert.Contains(handler.Requests, request => request.RequestUri!.AbsolutePath == "/books/OL600M.json");
    }

    [Fact]
    public async Task GetBookAsync_WhenSearchReturnsNoResults_ShouldReturnNull()
    {
        // Arrange
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(path: "/books/title.epub", title: "Search Title");
        StubOpenLibraryHttpMessageHandler handler = new();
        handler.MapPath("/search.json", """{"docs":[]}""");
        OpenLibraryBookMetadataProvider sut = CreateProvider(handler);

        // Act
        AddBookRequest? result = await sut.GetBookAsync(lookup);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetBookAsync_WhenOpenLibraryIdHasAnUnknownSuffix_ShouldThrowArgumentException()
    {
        // Arrange
        BookMetadataLookupDto lookup = _bookMetadataLookupDtoFixture.Create(
            isbn: null,
            openLibraryId: "OL123X",
            title: null,
            author: null,
            languageCode: null);
        OpenLibraryBookMetadataProvider sut = CreateProvider(new StubOpenLibraryHttpMessageHandler());

        // Act
        async Task Act()
        {
            await sut.GetBookAsync(lookup);
        }

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(Act);
    }

    private OpenLibraryBookMetadataProvider CreateProvider(StubOpenLibraryHttpMessageHandler handler, OpenLibrarySettingsDto? settings = null)
    {
        OpenLibrarySettingsDto runtimeSettings = settings ?? _settingsFixture.Create(minimumRequestInterval: TimeSpan.Zero);
        OpenLibrarySettingsProvider settingsProvider = new(null, Guid.NewGuid(), runtimeSettings);
        OpenLibraryHttpClient openLibraryHttpClient = new(new HttpClient(handler), settingsProvider);
        return new OpenLibraryBookMetadataProvider(openLibraryHttpClient, settingsProvider);
    }
}
