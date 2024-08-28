#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate;
using Lumina.Presentation.Api.Common.Contracts.Books;
using Lumina.Presentation.Api.IntegrationTests.Common.Converters;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Lumina.Presentation.Api.IntegrationTests.Core.Controllers.Books.Fixtures;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Controllers.Books;

/// <summary>
/// Contains integration tests for the <see cref="BooksController"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BooksControllerTests : IClassFixture<LuminaApiFactory>
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly LuminaApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly RequestBookFixture _requestBookFixture;
    private readonly JsonSerializerOptions _jsonOptions = new();
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="BooksControllerTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public BooksControllerTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = _apiFactory.CreateClient();
        _requestBookFixture = new RequestBookFixture();
        _jsonOptions.Converters.Add(new BookJsonConverter());
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public async Task AddsBook_WhenCalled_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var bookResponse = await response.Content.ReadFromJsonAsync<Book>(_jsonOptions);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        bookResponse.Should().NotBeNull();
        
        // metadata checks
        bookResponse!.Metadata.Title.Should().Be(bookRequest.Metadata.Title);
        bookResponse.Metadata.OriginalTitle.Value.Should().Be(bookRequest.Metadata.OriginalTitle);
        bookResponse.Metadata.Description.Value.Should().Be(bookRequest.Metadata.Description);
        bookResponse.Metadata.Publisher.Value.Should().Be(bookRequest.Metadata.Publisher);
        bookResponse.Metadata.PageCount.Value.Should().Be(bookRequest.Metadata.PageCount);

        // releaseInfo checks
        bookResponse.Metadata.ReleaseInfo.OriginalReleaseDate.Value.Should().Be(bookRequest.Metadata.ReleaseInfo.OriginalReleaseDate);
        bookResponse.Metadata.ReleaseInfo.OriginalReleaseYear.Value.Should().Be(bookRequest.Metadata.ReleaseInfo.OriginalReleaseYear);
        bookResponse.Metadata.ReleaseInfo.ReReleaseDate.Value.Should().Be(bookRequest.Metadata.ReleaseInfo.ReReleaseDate);
        bookResponse.Metadata.ReleaseInfo.ReReleaseYear.Value.Should().Be(bookRequest.Metadata.ReleaseInfo.ReReleaseYear);
        bookResponse.Metadata.ReleaseInfo.ReleaseCountry.Value.Should().Be(bookRequest.Metadata.ReleaseInfo.ReleaseCountry);
        bookResponse.Metadata.ReleaseInfo.ReleaseVersion.Value.Should().Be(bookRequest.Metadata.ReleaseInfo.ReleaseVersion);

        // language checks
        bookResponse.Metadata.Language.Value.LanguageCode.Should().Be(bookRequest.Metadata.Language!.LanguageCode);
        bookResponse.Metadata.Language.Value.LanguageName.Should().Be(bookRequest.Metadata.Language.LanguageName);
        bookResponse.Metadata.Language.Value.NativeName.Value.Should().Be(bookRequest.Metadata.Language.NativeName);

        // original language checks
        bookResponse.Metadata.OriginalLanguage.Value.LanguageCode.Should().Be(bookRequest.Metadata.OriginalLanguage!.LanguageCode);
        bookResponse.Metadata.OriginalLanguage.Value.LanguageName.Should().Be(bookRequest.Metadata.OriginalLanguage.LanguageName);
        bookResponse.Metadata.OriginalLanguage.Value.NativeName.Value.Should().Be(bookRequest.Metadata.OriginalLanguage.NativeName);

        // genres checks
        bookResponse.Metadata.Genres.Should().HaveCount(bookRequest.Metadata.Genres.Count);
        bookResponse.Metadata.Genres.Select(genre => genre.Name).Should().BeEquivalentTo(bookRequest.Metadata.Genres.Select(genre => genre.Name));

        // tags checks
        bookResponse.Metadata.Tags.Should().HaveCount(bookRequest.Metadata.Tags.Count);
        bookResponse.Metadata.Tags.Select(tag => tag.Name).Should().BeEquivalentTo(bookRequest.Metadata.Tags.Select(tag => tag.Name));

        // book specific properties
        bookResponse.Format.Should().Be(bookRequest.Format);
        bookResponse.Edition.Value.Should().Be(bookRequest.Edition);
        bookResponse.VolumeNumber.Value.Should().Be(bookRequest.VolumeNumber);
        bookResponse.ASIN.Value.Should().Be(bookRequest.ASIN);
        bookResponse.GoodreadsId.Value.Should().Be(bookRequest.GoodreadsId);
        bookResponse.LCCN.Value.Should().Be(bookRequest.LCCN);
        bookResponse.OCLCNumber.Value.Should().Be(bookRequest.OCLCNumber);
        bookResponse.OpenLibraryId.Value.Should().Be(bookRequest.OpenLibraryId);
        bookResponse.LibraryThingId.Value.Should().Be(bookRequest.LibraryThingId);
        bookResponse.GoogleBooksId.Value.Should().Be(bookRequest.GoogleBooksId);
        bookResponse.BarnesAndNobleId.Value.Should().Be(bookRequest.BarnesAndNobleId);
        bookResponse.AppleBooksId.Value.Should().Be(bookRequest.AppleBooksId);

        // ISBNs checks
        bookResponse.ISBNs.Should().HaveCount(bookRequest.ISBNs.Count);
        bookResponse.ISBNs.Should().HaveCount(bookRequest.ISBNs.Count);
        bookResponse.ISBNs.Select(isbn => new { isbn.Value, isbn.Format })
            .Should().BeEquivalentTo(bookRequest.ISBNs.Select(isbn => new { isbn.Value, isbn.Format }));

        // contributors checks
        // TODO: update when contributors are implemented
        // bookResponse.Contributors.Should().HaveCount(bookRequest.Contributors.Count);

        // ratings checks
        bookResponse.Ratings.Should().HaveCount(bookRequest.Ratings.Count);
        bookResponse.Ratings.Should().HaveCount(bookRequest.Ratings.Count);
        bookResponse.Ratings.Select(bookRating => new { Source = bookRating.Source.Value, bookRating.Value, bookRating.MaxValue, VoteCount = bookRating.VoteCount.Value })
            .Should().BeEquivalentTo(bookRequest.Ratings.Select(bookRating => new { bookRating.Source, bookRating.Value, bookRating.MaxValue, bookRating.VoteCount }));

        // series checks
        if (bookRequest.Series is not null)
        {
            bookResponse.Series.Should().NotBeNull();
            bookResponse.Series.Value.Metadata.Title.Should().Be(bookRequest.Series.Title);            
        }
        else
            bookResponse.Series.Value.Should().BeNull();

        // check Location header
        response.Headers.Location.Should().NotBeNull();
        var locationUri = response.Headers.Location!.ToString();
        locationUri.Should().StartWith("/api/v1/books/");

        // extract ID from Location header
        var idFromHeader = locationUri.Split('/').Last();

        // compare with bookResponse id
        bookResponse!.Id.ToString().Should().Be(idFromHeader);
    }

    [Fact]
    public async Task GetBooks_WhenCalled_ShouldReturnBooks()
    {
        // Arrange
        var response = await _client.GetAsync("/api/v1/books");

        // Act
        response.EnsureSuccessStatusCode();

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var bookResponse = await response.Content.ReadFromJsonAsync<Book[]>(_jsonOptions);
        bookResponse.Should().NotBeNull();
    }
    #endregion
}