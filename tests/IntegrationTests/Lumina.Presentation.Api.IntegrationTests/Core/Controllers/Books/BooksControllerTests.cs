#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate;
using Lumina.Presentation.Api.Common.Contracts.Books;
using Lumina.Presentation.Api.IntegrationTests.Common.Converters;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Lumina.Presentation.Api.IntegrationTests.Core.Controllers.Books.Fixtures;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Lumina.Domain.Common.Errors;
using Bogus;
using Lumina.Domain.Common.Enums;
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

    [Fact]
    public async Task AddBook_WhenCalledWithValidData_ShouldAddBook()
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
        bookResponse!.Metadata.Title.Should().Be(bookRequest.Metadata!.Title);
        bookResponse.Metadata.OriginalTitle.Value.Should().Be(bookRequest.Metadata.OriginalTitle);
        bookResponse.Metadata.Description.Value.Should().Be(bookRequest.Metadata.Description);
        bookResponse.Metadata.Publisher.Value.Should().Be(bookRequest.Metadata.Publisher);
        bookResponse.Metadata.PageCount.Value.Should().Be(bookRequest.Metadata.PageCount);

        // releaseInfo checks
        bookResponse.Metadata.ReleaseInfo.OriginalReleaseDate.Value.Should().Be(bookRequest.Metadata.ReleaseInfo!.OriginalReleaseDate);
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
        bookResponse.Metadata.Genres.Should().HaveCount(bookRequest.Metadata.Genres!.Count);
        bookResponse.Metadata.Genres.Select(genre => genre.Name).Should().BeEquivalentTo(bookRequest.Metadata.Genres.Select(genre => genre.Name));

        // tags checks
        bookResponse.Metadata.Tags.Should().HaveCount(bookRequest.Metadata.Tags!.Count);
        bookResponse.Metadata.Tags.Select(tag => tag.Name).Should().BeEquivalentTo(bookRequest.Metadata.Tags.Select(tag => tag.Name));

        // book specific properties
        bookResponse.Format.Value.Should().Be(bookRequest.Format);
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
        bookResponse.ISBNs.Should().HaveCount(bookRequest.ISBNs!.Count);
        bookResponse.ISBNs.Should().HaveCount(bookRequest.ISBNs.Count);
        bookResponse.ISBNs.Select(isbn => new { isbn.Value, isbn.Format })
            .Should().BeEquivalentTo(bookRequest.ISBNs.Select(isbn => new { isbn.Value, isbn.Format }));

        // contributors checks
        // TODO: update when contributors are implemented
        // bookResponse.Contributors.Should().HaveCount(bookRequest.Contributors.Count);

        // ratings checks
        bookResponse.Ratings.Should().HaveCount(bookRequest.Ratings!.Count);
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
    public async Task AddBook_WhenCalledWithEmptyTitle_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { Title = null! } };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.Metadata.TitleCannotBeEmpty.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthTitle_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { Title = new Faker().Random.String2(300) } };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.Metadata.TitleMustBeMaximum255CharactersLong.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyOriginalTitle_ShouldAddBook()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { OriginalTitle = null! } };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthOriginalTitle_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { OriginalTitle = new Faker().Random.String2(300) } };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyDescription_ShouldAddBook()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { Description = null! } };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthDescription_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { Description = new Faker().Random.String2(2001) } };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNullReleaseInfo_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { ReleaseInfo = null! } };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.Metadata.ReleaseInfoCannotBeNull.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyOriginalReleaseYear_ShouldAddBook()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { ReleaseInfo = bookRequest.Metadata.ReleaseInfo! with { OriginalReleaseYear = null! } } };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidValueOriginalReleaseYear_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { ReleaseInfo = bookRequest.Metadata.ReleaseInfo! with { OriginalReleaseYear = 10000 } } };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyReReleaseYear_ShouldAddBook()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { ReleaseInfo = bookRequest.Metadata.ReleaseInfo! with { ReReleaseYear = null!, ReReleaseDate = null } } };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidValueReReleaseYear_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Metadata = bookRequest.Metadata! with
            {
                ReleaseInfo = bookRequest.Metadata.ReleaseInfo!
            with
                { ReReleaseYear = 10000 }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.Metadata.ReReleaseYearMustBeBetween1And9999.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyReleaseCountry_ShouldAddBook()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { ReleaseInfo = bookRequest.Metadata.ReleaseInfo! with { ReleaseCountry = null! } } };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidValueReleaseCountry_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Metadata = bookRequest.Metadata! with
            {
                ReleaseInfo = bookRequest.Metadata.ReleaseInfo! with { ReleaseCountry = "test" }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.Metadata.CountryCodeMustBe2CharactersLong.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyReleaseVersion_ShouldAddBook()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { ReleaseInfo = bookRequest.Metadata.ReleaseInfo! with { ReleaseVersion = null! } } };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidValueReleasVersion_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Metadata = bookRequest.Metadata! with
            {
                ReleaseInfo = bookRequest.Metadata.ReleaseInfo! with { ReleaseVersion = new Faker().Random.String2(100) }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.Metadata.ReleaseVersionMustBeMaximum50CharactersLong.Code);
    }

    [Fact]
    public async Task AddBook_WhenReReleaseYearIsAfterOriginalReleaseYear_ShouldAddBook()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Metadata = bookRequest.Metadata! with
            {
                ReleaseInfo = bookRequest.Metadata.ReleaseInfo!
            with
                { OriginalReleaseYear = 2000, OriginalReleaseDate = new DateOnly(2000, 1, 1), ReReleaseYear = 2001, ReReleaseDate = new DateOnly(2001, 1, 1) }
            }
        };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenReReleaseYearIsBeforeReleaseYear_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Metadata = bookRequest.Metadata! with
            {
                ReleaseInfo = bookRequest.Metadata.ReleaseInfo!
                with
                { OriginalReleaseYear = 2001, OriginalReleaseDate = new DateOnly(2001, 1, 1), ReReleaseYear = 2000, ReReleaseDate = new DateOnly(2000, 1, 1) }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear.Code);
    }

    [Fact]
    public async Task AddBook_WhenReReleaseDateIsAfterOriginalReleaseDate_ShouldAddBook()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Metadata = bookRequest.Metadata! with
            {
                ReleaseInfo = bookRequest.Metadata.ReleaseInfo!
            with
                { OriginalReleaseYear = 2000, OriginalReleaseDate = new DateOnly(2000, 1, 1), ReReleaseYear = 2001, ReReleaseDate = new DateOnly(2001, 1, 1) }
            }
        };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenReReleaseDateIsBeforeReleaseDate_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Metadata = bookRequest.Metadata! with
            {
                ReleaseInfo = bookRequest.Metadata.ReleaseInfo!
                with
                { OriginalReleaseYear = 2001, OriginalReleaseDate = new DateOnly(2001, 1, 1), ReReleaseYear = 2000, ReReleaseDate = new DateOnly(2000, 1, 1) }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNullGenres_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { Genres = null! } };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.Metadata.GenresListCannotBeNull.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyGenreName_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Metadata = bookRequest.Metadata! with
            {
                Genres = bookRequest.Metadata.Genres!.Select((genre, index) => index == 0 ? genre with { Name = null } : genre).ToList()
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.Metadata.GenreNameCannotBeEmpty.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthGenreName_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Metadata = bookRequest.Metadata! with
            {
                Genres = bookRequest.Metadata.Genres!.Select((genre, index) => index == 0 ? genre with { Name = new Faker().Random.String2(100) } : genre).ToList()
            }
        };
        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.Metadata.GenreNameMustBeMaximum50CharactersLong.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNullTags_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { Tags = null! } };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.Metadata.TagsListCannotBeNull.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyTagName_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Metadata = bookRequest.Metadata! with
            {
                Tags = bookRequest.Metadata.Tags!.Select((tag, index) => index == 0 ? tag with { Name = null } : tag).ToList()
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.Metadata.TagNameCannotBeEmpty.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthTagName_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Metadata = bookRequest.Metadata! with
            {
                Tags = bookRequest.Metadata.Tags!.Select((tag, index) => index == 0 ? tag with { Name = new Faker().Random.String2(100) } : tag).ToList()
            }
        };
        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.Metadata.TagNameMustBeMaximum50CharactersLong.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyLanguage_ShouldAddBook()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { Language = null! } };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyLanguageCode_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { Language = bookRequest.Metadata.Language! with { LanguageCode = null } } };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.Metadata.LanguageCodeCannotBeEmpty.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthLanguageCode_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with 
        { 
            Metadata = bookRequest.Metadata! with 
            {
                Language = bookRequest.Metadata.Language! with 
                {
                    LanguageCode = new Faker().Random.String2(10) 
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.Metadata.LanguageCodeMustBe2CharactersLong.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyLanguageName_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { Language = bookRequest.Metadata.Language! with { LanguageName = null } } };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.Metadata.LanguageNameCannotBeEmpty.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthLanguageName_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Metadata = bookRequest.Metadata! with
            {
                Language = bookRequest.Metadata.Language! with
                {
                    LanguageName = new Faker().Random.String2(100)
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyLanguageNativeName_ShouldAddBook()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { Language = bookRequest.Metadata.Language! with { NativeName = null } } };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthNativeLanguageName_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Metadata = bookRequest.Metadata! with
            {
                Language = bookRequest.Metadata.Language! with
                {
                    NativeName = new Faker().Random.String2(100)
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyOriginalLanguage_ShouldAddBook()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { OriginalLanguage = null! } };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyOriginalLanguageCode_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { OriginalLanguage = bookRequest.Metadata.Language! with { LanguageCode = null } } };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.Metadata.LanguageCodeCannotBeEmpty.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthOriginalLanguageCode_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Metadata = bookRequest.Metadata! with
            {
                OriginalLanguage = bookRequest.Metadata.Language! with
                {
                    LanguageCode = new Faker().Random.String2(10)
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.Metadata.LanguageCodeMustBe2CharactersLong.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyOriginalLanguageName_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { OriginalLanguage = bookRequest.Metadata.Language! with { LanguageName = null } } };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.Metadata.LanguageNameCannotBeEmpty.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthOriginalLanguageName_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Metadata = bookRequest.Metadata! with
            {
                OriginalLanguage = bookRequest.Metadata.Language! with
                {
                    LanguageName = new Faker().Random.String2(100)
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthOriginalNativeLanguageName_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Metadata = bookRequest.Metadata! with
            {
                OriginalLanguage = bookRequest.Metadata.Language! with
                {
                    NativeName = new Faker().Random.String2(100)
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyOriginalLanguageNativeName_ShouldAddBook()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { OriginalLanguage = bookRequest.Metadata.Language! with { NativeName = null } } };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyPublisher_ShouldAddBook()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { Publisher = null } };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthPublisher_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { Publisher = new Faker().Random.String2(150) } };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.WrittenContent.PublisherMustBeMaximum100CharactersLong.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyPageCount_ShouldAddBook()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { PageCount = null } };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNegativePageCount_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { PageCount = -1 } };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.WrittenContent.PageCountMustBeGreaterThanZero.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyFormat_ShouldAddBook()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Format = null };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidFormat_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Format = (BookFormat)99 };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.WrittenContent.UnknownBookFormat.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyEdition_ShouldAddBook()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Edition = null };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthEdition_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Edition = new Faker().Random.String2(100) };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.WrittenContent.EditionMustBeMaximum50CharactersLong.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyVolumeNumber_ShouldAddBook()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { VolumeNumber = null };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNegativeVolumeNumber_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { VolumeNumber = -1 };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.WrittenContent.VolumeNumberMustBeGreaterThanZero.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptySeries_ShouldAddBook()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Series = null };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    //[Fact]
    //public async Task AddBook_WhenCalledWithEmptySeriesTitle_ShouldReturnBadRequest()
    //{
    //    // Arrange
    //    var bookRequest = _requestBookFixture.CreateRequestBook();
    //    bookRequest = bookRequest with
    //    {
    //        Series = bookRequest.Series! with { Title = null }
    //    };

    //    // Act
    //    var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

    //    // Assert
    //    await AssertBadRequestWithValidationErrors(response, Errors.Metadata.TitleCannotBeEmpty.Code);
    //}

    //[Fact]
    //public async Task AddBook_WhenCalledWithInvalidLengthSeriesTitle_ShouldReturnBadRequest()
    //{
    //    // Arrange
    //    var bookRequest = _requestBookFixture.CreateRequestBook();
    //    bookRequest = bookRequest with { Series = bookRequest.Series! with { Title = new Faker().Random.String2(300) } };

    //    // Act
    //    var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

    //    // Assert
    //    await AssertBadRequestWithValidationErrors(response, Errors.Metadata.TitleMustBeMaximum255CharactersLong.Code);
    //}

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyAsin_ShouldAddBook()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { ASIN = null };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthAsin_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { ASIN = new Faker().Random.String2(15) };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.WrittenContent.AsinMustBe10CharactersLong.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyGoodreadsId_ShouldAddBook()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { GoodreadsId = null };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidGoodreadsId_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { GoodreadsId = new Faker().Random.String2(2) };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.WrittenContent.GoodreadsIdMustBeNumeric.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyLccn_ShouldAddBook()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { LCCN = null };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLccn_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { LCCN = new Faker().Random.String2(200) };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.WrittenContent.InvalidLccnFormat.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyOclcNumber_ShouldAddBook()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { OCLCNumber = null };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidOclcNumber_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { OCLCNumber = new Faker().Random.String2(200) };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.WrittenContent.InvalidOclcFormat.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyOpenLibraryId_ShouldAddBook()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { OpenLibraryId = null };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidOpenLibraryId_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { OpenLibraryId = new Faker().Random.String2(200) };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.WrittenContent.InvalidOpenLibraryId.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyLibraryThingId_ShouldAddBook()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { LibraryThingId = null };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthLibraryThingId_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { LibraryThingId = new Faker().Random.String2(200) };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.WrittenContent.LibraryThingIdMustBeMaximum50CharactersLong.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyGoogleBooksId_ShouldAddBook()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { GoogleBooksId = null };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthGoogleBooksId_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { GoogleBooksId = new Faker().Random.String2(20) };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.WrittenContent.GoogleBooksIdMustBe12CharactersLong.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidGoogleBooksId_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { GoogleBooksId = new Faker().Random.String2(11) + " " };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.WrittenContent.InvalidGoogleBooksIdFormat.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyBarnesAndNobleId_ShouldAddBook()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { BarnesAndNobleId = null };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthBarnesAndNobleId_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { BarnesAndNobleId = new Faker().Random.String2(20) };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.WrittenContent.BarnesAndNoblesIdMustBe10CharactersLong.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidBarnesAndNobleId_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { BarnesAndNobleId = new Faker().Random.String2(10) };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.WrittenContent.InvalidBarnesAndNoblesIdFormat.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyAppleBooksId_ShouldAddBook()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { AppleBooksId = null };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidAppleBooksId_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { AppleBooksId = new Faker().Random.String2(10) };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.WrittenContent.InvalidAppleBooksIdFormat.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNullIsbns_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { ISBNs = null! };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.WrittenContent.IsbnListCannotBeNull.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyIsbnValue_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            ISBNs = bookRequest.ISBNs!.Select((isbn, index) => index == 0 ? isbn with { Value = null } : isbn).ToList()
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.WrittenContent.IsbnValueCannotBeEmpty.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidIsbn10Value_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            ISBNs = bookRequest.ISBNs!.Select((isbn, index) => index == 0 ? isbn with { Value = new Faker().Random.String2(5), Format = IsbnFormat.Isbn10 } : isbn).ToList()
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.WrittenContent.InvalidIsbn10Format.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidIsbn13Value_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            ISBNs = bookRequest.ISBNs!.Select((isbn, index) => index == 0 ? isbn with { Value = new Faker().Random.String2(5), Format = IsbnFormat.Isbn13 } : isbn).ToList()
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.WrittenContent.InvalidIsbn13Format.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidIsbnFormat_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            ISBNs = bookRequest.ISBNs!.Select((isbn, index) => index == 0 ? isbn with { Format = (IsbnFormat)99 } : isbn).ToList()
        };
        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.WrittenContent.UnknownIsbnFormat.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNullContributors_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Contributors = null! };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.MediaContributor.ContributorsListCannotBeNull.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyContributorName_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Contributors = bookRequest.Contributors!.Select((contributor, index) => index == 0 ? contributor with { Name = null } : contributor).ToList()
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.MediaContributor.ContributorNameCannotBeEmpty.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthContributorName_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Contributors = bookRequest.Contributors!.Select((contributor, index) => index == 0 ? contributor with { Name = new Faker().Random.String2(150) } : contributor).ToList()
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.MediaContributor.ContributorNameMustBeMaximum100CharactersLong.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyContributorRole_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Contributors = bookRequest.Contributors!.Select((contributor, index) => index == 0 ? contributor with { Role = null } : contributor).ToList()
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.MediaContributor.ContributorRoleCannotBeNull.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyContributorRoleName_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Contributors = bookRequest.Contributors!.Select((contributor, index) => index == 0 ? contributor with 
                { Role = contributor.Role! with { Name = null } } : contributor).ToList()
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.MediaContributor.RoleNameCannotBeEmpty.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthContributorRoleName_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Contributors = bookRequest.Contributors!.Select((contributor, index) => index == 0 ? contributor with
            { Role = contributor.Role! with { Name = new Faker().Random.String2(100) } } : contributor).ToList()
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.MediaContributor.RoleNameMustBeMaximum50CharactersLong.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyContributorRoleCategory_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Contributors = bookRequest.Contributors!.Select((contributor, index) => index == 0 ? contributor with
            { Role = contributor.Role! with { Category = null } } : contributor).ToList()
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.MediaContributor.RoleCategoryCannotBeEmpty.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthContributorRoleCategory_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Contributors = bookRequest.Contributors!.Select((contributor, index) => index == 0 ? contributor with
            { Role = contributor.Role! with { Category = new Faker().Random.String2(100) } } : contributor).ToList()
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.MediaContributor.RoleCategoryMustBeMaximum50CharactersLong.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNullRatings_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Ratings = null! };

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.Metadata.RatingsListCannotBeNull.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNegativeRatingValue_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Ratings = bookRequest.Ratings!.Select((rating, index) => index == 0 ? rating with { Value = -3 } : rating).ToList()
        };
        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.Metadata.RatingValueMustBePositive.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithRatingValueGreaterThanMaxValue_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Ratings = bookRequest.Ratings!.Select((rating, index) => index == 0 ? rating with { Value = 3, MaxValue = 2 } : rating).ToList()
        };
        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.Metadata.RatingValueCannotBeGreaterThanMaxValue.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNegativeMaxRatingValue_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Ratings = bookRequest.Ratings!.Select((rating, index) => index == 0 ? rating with { MaxValue = -3 } : rating).ToList()
        };
        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.Metadata.RatingMaxValueMustBePositive.Code);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyRatingVoteCount_ShouldAddBook()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Ratings = bookRequest.Ratings!.Select((rating, index) => index == 0 ? rating with { VoteCount = null } : rating).ToList()
        };
        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNegativeVoteCount_ShouldReturnBadRequest()
    {
        // Arrange
        var bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Ratings = bookRequest.Ratings!.Select((rating, index) => index == 0 ? rating with { VoteCount = -3 } : rating).ToList()
        };
        // Act
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);

        // Assert
        await AssertBadRequestWithValidationErrors(response, Errors.Metadata.RatingVoteCountMustBePositive.Code);
    }

    private static async Task AssertBadRequestWithValidationErrors(HttpResponseMessage response, params string[] expectedErrorCodes)
    {
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Type.Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.1");
        problemDetails.Title.Should().Be("One or more validation errors occurred."); // TODO: update when localization is implemented
        problemDetails.Status.Should().Be(400);

        var errors = problemDetails.Extensions["errors"] as JsonElement?;
        errors.Should().NotBeNull();

        var actualErrorCodes = new List<string>();
        foreach (var property in errors!.Value.EnumerateObject())
            actualErrorCodes.AddRange(property.Value.EnumerateArray().Select(e => e.GetString()!));
        actualErrorCodes.Should().Contain(expectedErrorCodes, because: "all expected error codes should be present");
        Console.WriteLine($"All error codes: {string.Join(", ", actualErrorCodes)}");

        problemDetails.Extensions.Should().ContainKey("traceId");
        problemDetails.Extensions["traceId"]!.ToString().Should().MatchRegex(@"^[\da-f]{2}-[\da-f]{32}-[\da-f]{16}-[\da-f]{2}$");
    }
    
    private async Task AssertCreated(AddBookRequest bookRequest)
    {
        var response = await _client.PostAsJsonAsync("api/v1/books", bookRequest);
        response.EnsureSuccessStatusCode();
        var bookResponse = await response.Content.ReadFromJsonAsync<Book>(_jsonOptions);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        bookResponse.Should().NotBeNull();
    }
    #endregion
}