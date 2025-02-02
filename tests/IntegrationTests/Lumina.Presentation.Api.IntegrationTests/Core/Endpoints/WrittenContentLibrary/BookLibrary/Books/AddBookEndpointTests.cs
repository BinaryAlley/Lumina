#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Common.Enums.BookLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using Lumina.Presentation.Api.IntegrationTests.Common.Converters;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.WrittenContentLibrary.BookLibrary.Books.Fixtures;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Contains integration tests for the <see cref="AddBookEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddBookEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        PropertyNameCaseInsensitive = true,
        Converters = {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
            new BookJsonConverter()
        }
    };
    private readonly AddBookRequestFixture _requestBookFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddBookEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public AddBookEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        _apiFactory = apiFactory;
        _requestBookFixture = new AddBookRequestFixture();
    }

    /// <summary>
    /// Initializes authenticated API client.
    /// </summary>
    public async Task InitializeAsync()
    {
        _client = await _apiFactory.CreateAuthenticatedClientAsync();
    }

    [Fact]
    public async Task AddBook_WhenCalledWithValidData_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        Book? bookResponse = await response.Content.ReadFromJsonAsync<Book>(_jsonOptions);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(bookResponse);

        // metadata checks
        Assert.Equal(bookRequest.Metadata!.Title, bookResponse!.Metadata.Title);
        Assert.Equal(bookRequest.Metadata.OriginalTitle, bookResponse.Metadata.OriginalTitle.Value);
        Assert.Equal(bookRequest.Metadata.Description, bookResponse.Metadata.Description.Value);
        Assert.Equal(bookRequest.Metadata.Publisher, bookResponse.Metadata.Publisher.Value);
        Assert.Equal(bookRequest.Metadata.PageCount, bookResponse.Metadata.PageCount.Value);

        Assert.Equal(bookRequest.Metadata.ReleaseInfo!.OriginalReleaseDate, bookResponse.Metadata.ReleaseInfo.OriginalReleaseDate.Value);
        Assert.Equal(bookRequest.Metadata.ReleaseInfo.OriginalReleaseYear, bookResponse.Metadata.ReleaseInfo.OriginalReleaseYear.Value);
        Assert.Equal(bookRequest.Metadata.ReleaseInfo.ReReleaseDate, bookResponse.Metadata.ReleaseInfo.ReReleaseDate.Value);
        Assert.Equal(bookRequest.Metadata.ReleaseInfo.ReReleaseYear, bookResponse.Metadata.ReleaseInfo.ReReleaseYear.Value);
        Assert.Equal(bookRequest.Metadata.ReleaseInfo.ReleaseCountry, bookResponse.Metadata.ReleaseInfo.ReleaseCountry.Value);
        Assert.Equal(bookRequest.Metadata.ReleaseInfo.ReleaseVersion, bookResponse.Metadata.ReleaseInfo.ReleaseVersion.Value);

        // language checks
        Assert.Equal(bookRequest.Metadata.Language!.LanguageCode, bookResponse.Metadata.Language.Value.LanguageCode);
        Assert.Equal(bookRequest.Metadata.Language.LanguageName, bookResponse.Metadata.Language.Value.LanguageName);
        Assert.Equal(bookRequest.Metadata.Language.NativeName, bookResponse.Metadata.Language.Value.NativeName.Value);

        // original language checks
        Assert.Equal(bookRequest.Metadata.OriginalLanguage!.LanguageCode, bookResponse.Metadata.OriginalLanguage.Value.LanguageCode);
        Assert.Equal(bookRequest.Metadata.OriginalLanguage.LanguageName, bookResponse.Metadata.OriginalLanguage.Value.LanguageName);
        Assert.Equal(bookRequest.Metadata.OriginalLanguage.NativeName, bookResponse.Metadata.OriginalLanguage.Value.NativeName.Value);

        // genres checks
        Assert.Equal(bookRequest.Metadata.Genres!.Count, bookResponse.Metadata.Genres.Count);
        Assert.Equal(
            bookRequest.Metadata.Genres.Select(genre => genre.Name).OrderBy(x => x),
            bookResponse.Metadata.Genres.Select(genre => genre.Name).OrderBy(x => x));

        // tags checks
        Assert.Equal(bookRequest.Metadata.Tags!.Count, bookResponse.Metadata.Tags.Count);
        Assert.Equal(
            bookRequest.Metadata.Tags.Select(tag => tag.Name).OrderBy(x => x),
            bookResponse.Metadata.Tags.Select(tag => tag.Name).OrderBy(x => x));

        // book specific properties
        Assert.Equal(bookRequest.Format, bookResponse.Format.Value);
        Assert.Equal(bookRequest.Edition, bookResponse.Edition.Value);
        Assert.Equal(bookRequest.VolumeNumber, bookResponse.VolumeNumber.Value);
        Assert.Equal(bookRequest.ASIN, bookResponse.ASIN.Value);
        Assert.Equal(bookRequest.GoodreadsId, bookResponse.GoodreadsId.Value);
        Assert.Equal(bookRequest.LCCN, bookResponse.LCCN.Value);
        Assert.Equal(bookRequest.OCLCNumber, bookResponse.OCLCNumber.Value);
        Assert.Equal(bookRequest.OpenLibraryId, bookResponse.OpenLibraryId.Value);
        Assert.Equal(bookRequest.LibraryThingId, bookResponse.LibraryThingId.Value);
        Assert.Equal(bookRequest.GoogleBooksId, bookResponse.GoogleBooksId.Value);
        Assert.Equal(bookRequest.BarnesAndNobleId, bookResponse.BarnesAndNobleId.Value);
        Assert.Equal(bookRequest.AppleBooksId, bookResponse.AppleBooksId.Value);

        // ISBNs checks
        Assert.Equal(bookRequest.ISBNs!.Count, bookResponse.ISBNs.Count);
        var requestIsbnData = bookRequest.ISBNs.Select(isbn => new { isbn.Value, isbn.Format }).OrderBy(x => x.Value).ToList();
        var responseIsbnData = bookResponse.ISBNs.Select(isbn => new { Value = isbn.Value, Format = isbn.Format }).OrderBy(x => x.Value).ToList();
        Assert.Equal(requestIsbnData.Count, responseIsbnData.Count);
        for (int i = 0; i < requestIsbnData.Count; i++)
        {
            Assert.Equal(requestIsbnData[i].Value, responseIsbnData[i].Value);
            Assert.Equal(requestIsbnData[i].Format, responseIsbnData[i].Format);
        }

        // ratings checks
        Assert.Equal(bookRequest.Ratings!.Count, bookResponse.Ratings.Count);
        var requestRatingData = bookRequest.Ratings.Select(r => new { Source = r.Source, r.Value, r.MaxValue, r.VoteCount }).OrderBy(x => x.Source).ToList();
        var responseRatingData = bookResponse.Ratings.Select(r => new { Source = r.Source.Value, r.Value, r.MaxValue, VoteCount = r.VoteCount.Value }).OrderBy(x => x.Source).ToList();
        Assert.Equal(requestRatingData.Count, responseRatingData.Count);
        for (int i = 0; i < requestRatingData.Count; i++)
        {
            Assert.Equal(requestRatingData[i].Source, responseRatingData[i].Source);
            Assert.Equal(requestRatingData[i].Value, responseRatingData[i].Value);
            Assert.Equal(requestRatingData[i].MaxValue, responseRatingData[i].MaxValue);
            Assert.Equal(requestRatingData[i].VoteCount, responseRatingData[i].VoteCount);
        }

        // series checks
        if (bookRequest.Series is not null)
            Assert.Equal(bookRequest.Series.Title, bookResponse.Series.Value.Metadata.Title);
        else
            Assert.Null(bookResponse.Series.Value);

        // check Location header
        Assert.NotNull(response.Headers.Location);
        string locationUri = response.Headers.Location!.ToString();
        Assert.EndsWith("/api/v1/books/" + bookResponse.Id.Value, locationUri);

        // extract ID from Location header and compare
        string idFromHeader = locationUri.Split('/').Last();
        Assert.Equal(idFromHeader, bookResponse!.Id.ToString());
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyTitle_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { Title = null! } };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.TitleCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthTitle_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { Title = new Faker().Random.String2(300) } };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.TitleMustBeMaximum255CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyOriginalTitle_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { OriginalTitle = null! } };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthOriginalTitle_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { OriginalTitle = new Faker().Random.String2(300) } };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyDescription_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { Description = null! } };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthDescription_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { Description = new Faker().Random.String2(2001) } };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNullReleaseInfo_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { ReleaseInfo = null! } };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.ReleaseInfoCannotBeNull.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyOriginalReleaseYear_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { ReleaseInfo = bookRequest.Metadata.ReleaseInfo! with { OriginalReleaseYear = null! } } };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidValueOriginalReleaseYear_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { ReleaseInfo = bookRequest.Metadata.ReleaseInfo! with { OriginalReleaseYear = 10000 } } };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyReReleaseYear_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { ReleaseInfo = bookRequest.Metadata.ReleaseInfo! with { ReReleaseYear = null!, ReReleaseDate = null } } };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidValueReReleaseYear_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
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
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.ReReleaseYearMustBeBetween1And9999.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyReleaseCountry_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { ReleaseInfo = bookRequest.Metadata.ReleaseInfo! with { ReleaseCountry = null! } } };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidValueReleaseCountry_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Metadata = bookRequest.Metadata! with
            {
                ReleaseInfo = bookRequest.Metadata.ReleaseInfo! with { ReleaseCountry = "test" }
            }
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.CountryCodeMustBe2CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyReleaseVersion_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { ReleaseInfo = bookRequest.Metadata.ReleaseInfo! with { ReleaseVersion = null! } } };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidValueReleasVersion_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Metadata = bookRequest.Metadata! with
            {
                ReleaseInfo = bookRequest.Metadata.ReleaseInfo! with { ReleaseVersion = new Faker().Random.String2(100) }
            }
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.ReleaseVersionMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenReReleaseYearIsAfterOriginalReleaseYear_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
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
    public async Task AddBook_WhenReReleaseYearIsBeforeReleaseYear_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
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
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear.Description);
    }

    [Fact]
    public async Task AddBook_WhenReReleaseDateIsAfterOriginalReleaseDate_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
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
    public async Task AddBook_WhenReReleaseDateIsBeforeReleaseDate_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
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
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNullGenres_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { Genres = null! } };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.GenresListCannotBeNull.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyGenreName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Metadata = bookRequest.Metadata! with
            {
                Genres = bookRequest.Metadata.Genres!.Select((genre, index) => index == 0 ? genre with { Name = null } : genre).ToList()
            }
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.GenreNameCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthGenreName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Metadata = bookRequest.Metadata! with
            {
                Genres = bookRequest.Metadata.Genres!.Select((genre, index) => index == 0 ? genre with { Name = new Faker().Random.String2(100) } : genre).ToList()
            }
        };
        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.GenreNameMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNullTags_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { Tags = null! } };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.TagsListCannotBeNull.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyTagName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Metadata = bookRequest.Metadata! with
            {
                Tags = bookRequest.Metadata.Tags!.Select((tag, index) => index == 0 ? tag with { Name = null } : tag).ToList()
            }
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.TagNameCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthTagName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Metadata = bookRequest.Metadata! with
            {
                Tags = bookRequest.Metadata.Tags!.Select((tag, index) => index == 0 ? tag with { Name = new Faker().Random.String2(100) } : tag).ToList()
            }
        };
        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.TagNameMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyLanguage_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { Language = null! } };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyLanguageCode_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { Language = bookRequest.Metadata.Language! with { LanguageCode = null } } };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.LanguageCodeCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthLanguageCode_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
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
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.LanguageCodeMustBe2CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyLanguageName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { Language = bookRequest.Metadata.Language! with { LanguageName = null } } };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.LanguageNameCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthLanguageName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
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
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyLanguageNativeName_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { Language = bookRequest.Metadata.Language! with { NativeName = null } } };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthNativeLanguageName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
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
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyOriginalLanguage_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { OriginalLanguage = null! } };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyOriginalLanguageCode_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { OriginalLanguage = bookRequest.Metadata.Language! with { LanguageCode = null } } };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.LanguageCodeCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthOriginalLanguageCode_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
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
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.LanguageCodeMustBe2CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyOriginalLanguageName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { OriginalLanguage = bookRequest.Metadata.Language! with { LanguageName = null } } };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.LanguageNameCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthOriginalLanguageName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
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
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthOriginalNativeLanguageName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
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
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyOriginalLanguageNativeName_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { OriginalLanguage = bookRequest.Metadata.Language! with { NativeName = null } } };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyPublisher_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { Publisher = null } };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthPublisher_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { Publisher = new Faker().Random.String2(150) } };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.PublisherMustBeMaximum100CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyPageCount_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { PageCount = null } };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNegativePageCount_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Metadata = bookRequest.Metadata! with { PageCount = -1 } };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.PageCountMustBeGreaterThanZero.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyFormat_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Format = null };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidFormat_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Format = (BookFormat)99 };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.UnknownBookFormat.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyEdition_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Edition = null };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthEdition_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Edition = new Faker().Random.String2(100) };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.EditionMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyVolumeNumber_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { VolumeNumber = null };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNegativeVolumeNumber_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { VolumeNumber = -1 };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.VolumeNumberMustBeGreaterThanZero.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptySeries_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Series = null };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    //[Fact]
    //public async Task AddBook_WhenCalledWithEmptySeriesTitle_ShouldReturnUnprocessableEntity()
    //{
    //    // Arrange
    //    var bookRequest = _requestBookFixture.CreateRequestBook();
    //    bookRequest = bookRequest with
    //    {
    //        Series = bookRequest.Series! with { Title = null }
    //    };

    //    // Act
    //    var response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

    //    // Assert
    //    await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.TitleCannotBeEmpty.Description);
    //}

    //[Fact]
    //public async Task AddBook_WhenCalledWithInvalidLengthSeriesTitle_ShouldReturnUnprocessableEntity()
    //{
    //    // Arrange
    //    var bookRequest = _requestBookFixture.CreateRequestBook();
    //    bookRequest = bookRequest with { Series = bookRequest.Series! with { Title = new Faker().Random.String2(300) } };

    //    // Act
    //    var response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

    //    // Assert
    //    await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.TitleMustBeMaximum255CharactersLong.Description);
    //}

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyAsin_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { ASIN = null };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthAsin_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { ASIN = new Faker().Random.String2(15) };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.AsinMustBe10CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyGoodreadsId_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { GoodreadsId = null };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidGoodreadsId_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { GoodreadsId = new Faker().Random.String2(2) };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.GoodreadsIdMustBeNumeric.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyLccn_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { LCCN = null };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLccn_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { LCCN = new Faker().Random.String2(200) };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.InvalidLccnFormat.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyOclcNumber_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { OCLCNumber = null };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidOclcNumber_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { OCLCNumber = new Faker().Random.String2(200) };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.InvalidOclcFormat.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyOpenLibraryId_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { OpenLibraryId = null };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidOpenLibraryId_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { OpenLibraryId = new Faker().Random.String2(200) };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.InvalidOpenLibraryId.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyLibraryThingId_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { LibraryThingId = null };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthLibraryThingId_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { LibraryThingId = new Faker().Random.String2(200) };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.LibraryThingIdMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyGoogleBooksId_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { GoogleBooksId = null };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthGoogleBooksId_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { GoogleBooksId = new Faker().Random.String2(20) };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.GoogleBooksIdMustBe12CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidGoogleBooksId_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { GoogleBooksId = new Faker().Random.String2(11) + " " };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.InvalidGoogleBooksIdFormat.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyBarnesAndNobleId_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { BarnesAndNobleId = null };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthBarnesAndNobleId_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { BarnesAndNobleId = new Faker().Random.String2(20) };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.BarnesAndNoblesIdMustBe10CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidBarnesAndNobleId_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { BarnesAndNobleId = new Faker().Random.String2(10) };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.InvalidBarnesAndNoblesIdFormat.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyAppleBooksId_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { AppleBooksId = null };

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidAppleBooksId_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { AppleBooksId = new Faker().Random.String2(10) };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.InvalidAppleBooksIdFormat.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNullIsbns_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { ISBNs = null! };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.IsbnListCannotBeNull.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyIsbnValue_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            ISBNs = bookRequest.ISBNs!.Select((isbn, index) => index == 0 ? isbn with { Value = null } : isbn).ToList()
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.IsbnValueCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidIsbn10Value_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            ISBNs = bookRequest.ISBNs!.Select((isbn, index) => index == 0 ? isbn with { Value = new Faker().Random.String2(5), Format = IsbnFormat.Isbn10 } : isbn).ToList()
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.InvalidIsbn10Format.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidIsbn13Value_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            ISBNs = bookRequest.ISBNs!.Select((isbn, index) => index == 0 ? isbn with { Value = new Faker().Random.String2(5), Format = IsbnFormat.Isbn13 } : isbn).ToList()
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.InvalidIsbn13Format.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidIsbnFormat_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            ISBNs = bookRequest.ISBNs!.Select((isbn, index) => index == 0 ? isbn with { Format = (IsbnFormat)99 } : isbn).ToList()
        };
        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.UnknownIsbnFormat.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNullContributors_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Contributors = null! };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.MediaContributor.ContributorsListCannotBeNull.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNullContributorName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Contributors = bookRequest.Contributors!.Select((contributor, index) => index == 0 ? contributor with { Name = null! } : contributor).ToList()
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.MediaContributor.ContributorNameCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyContributorDisplayName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Contributors = bookRequest.Contributors!.Select((contributor, index) => index == 0 ? contributor with { Name = contributor.Name! with { DisplayName = string.Empty } } : contributor).ToList()
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.MediaContributor.ContributorDisplayNameCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNullContributorDisplayName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Contributors = bookRequest.Contributors!.Select((contributor, index) => index == 0 ? contributor with { Name = contributor.Name! with { DisplayName = null! } } : contributor).ToList()
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.MediaContributor.ContributorDisplayNameCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthContributorDisplayName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Contributors = bookRequest.Contributors!.Select((contributor, index) => index == 0 ? contributor with { Name = contributor.Name! with { DisplayName = new Faker().Random.String2(150) } } : contributor).ToList()
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.MediaContributor.ContributorDisplayNameMustBeMaximum100CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthContributorLegalName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Contributors = bookRequest.Contributors!.Select((contributor, index) => index == 0 ? contributor with { Name = contributor.Name! with { LegalName = new Faker().Random.String2(150) } } : contributor).ToList()
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.MediaContributor.ContributorLegalNameMustBeMaximum100CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyContributorRole_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Contributors = bookRequest.Contributors!.Select((contributor, index) => index == 0 ? contributor with { Role = null } : contributor).ToList()
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.MediaContributor.ContributorRoleCannotBeNull.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyContributorRoleName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Contributors = bookRequest.Contributors!.Select((contributor, index) => index == 0 ? contributor with
            { Role = contributor.Role! with { Name = null } } : contributor).ToList()
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.MediaContributor.RoleNameCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthContributorRoleName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Contributors = bookRequest.Contributors!.Select((contributor, index) => index == 0 ? contributor with
            { Role = contributor.Role! with { Name = new Faker().Random.String2(100) } } : contributor).ToList()
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.MediaContributor.RoleNameMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyContributorRoleCategory_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Contributors = bookRequest.Contributors!.Select((contributor, index) => index == 0 ? contributor with
            { Role = contributor.Role! with { Category = null } } : contributor).ToList()
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.MediaContributor.RoleCategoryCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthContributorRoleCategory_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Contributors = bookRequest.Contributors!.Select((contributor, index) => index == 0 ? contributor with
            { Role = contributor.Role! with { Category = new Faker().Random.String2(100) } } : contributor).ToList()
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.MediaContributor.RoleCategoryMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNullRatings_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with { Ratings = null! };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.RatingsListCannotBeNull.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNegativeRatingValue_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Ratings = bookRequest.Ratings!.Select((rating, index) => index == 0 ? rating with { Value = -3 } : rating).ToList()
        };
        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.RatingValueMustBePositive.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithRatingValueGreaterThanMaxValue_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Ratings = bookRequest.Ratings!.Select((rating, index) => index == 0 ? rating with { Value = 3, MaxValue = 2 } : rating).ToList()
        };
        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.RatingValueCannotBeGreaterThanMaxValue.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNegativeMaxRatingValue_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Ratings = bookRequest.Ratings!.Select((rating, index) => index == 0 ? rating with { MaxValue = -3 } : rating).ToList()
        };
        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.RatingMaxValueMustBePositive.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyRatingVoteCount_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Ratings = bookRequest.Ratings!.Select((rating, index) => index == 0 ? rating with { VoteCount = null } : rating).ToList()
        };
        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNegativeVoteCount_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.CreateRequestBook();
        bookRequest = bookRequest with
        {
            Ratings = bookRequest.Ratings!.Select((rating, index) => index == 0 ? rating with { VoteCount = -3 } : rating).ToList()
        };
        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.RatingVoteCountMustBePositive.Description);
    }

    private async Task AssertUnprocessableEntityWithValidationErrors(HttpResponseMessage response, params string[] expectedErrorCodes)
    {
        Assert.Equal(HttpStatusCode.UnprocessableContent, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("/api/v1/books", problemDetails["instance"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", problemDetails["type"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString());

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        Assert.NotNull(errors);
        Assert.Contains("General.Validation", errors.Keys);
        Assert.All(expectedErrorCodes, code => Assert.Contains(code, errors["General.Validation"]));
    }

    private async Task AssertCreated(AddBookRequest bookRequest)
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/books", bookRequest);
        response.EnsureSuccessStatusCode();
        Book? bookResponse = await response.Content.ReadFromJsonAsync<Book>(_jsonOptions);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(bookResponse);
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public Task DisposeAsync()
    {
        _apiFactory.Dispose();
        return Task.CompletedTask;
    }
}
