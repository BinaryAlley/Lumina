#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.AddBook;

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
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly AddBookRequestFixture _requestBookFixture = new();
    private readonly WrittenContentMetadataDtoFixture _writtenContentMetadataDtoFixture = new();
    private readonly ReleaseInfoDtoFixture _releaseInfoDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly TagDtoFixture _tagDtoFixture = new();
    private readonly LanguageInfoDtoFixture _languageInfoDtoFixture = new();
    private readonly IsbnDtoFixture _isbnDtoFixture = new();
    private readonly BookRatingDtoFixture _bookRatingDtoFixture = new();
    private readonly MediaContributorReferenceDtoFixture _mediaContributorReferenceDtoFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly MediaContributorEntityFixture _mediaContributorEntityFixture = new();
    private readonly string _libraryContentLocation = Path.GetTempPath();
    private Guid _libraryId;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddBookEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public AddBookEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        _apiFactory = apiFactory;
    }

    /// <summary>
    /// Initializes authenticated API client, along with a media library owned by the authenticated user that the added books belong to.
    /// </summary>
    public async Task InitializeAsync()
    {
        _client = await _apiFactory.CreateAuthenticatedClientAsync();

        // The handler only allows adding books to a library the current user owns, so each test seeds its own owned library.
        _libraryId = Guid.NewGuid();
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid userId = dbContext.Users.Single(user => user.Username == _apiFactory.TestUsername).Id;
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: _libraryId, userId: userId, title: "Test Library", libraryType: LibraryType.EBook, contentLocations: [_libraryContentLocation]));
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task AddBook_WhenCalledWithValidData_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create();

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        BookResponse? bookResponse = await response.Content.ReadFromJsonAsync<BookResponse>(_jsonOptions);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(bookResponse);

        // metadata checks
        Assert.Equal(bookRequest.Metadata!.Title, bookResponse!.Metadata.Title);
        Assert.Equal(bookRequest.Metadata.OriginalTitle, bookResponse.Metadata.OriginalTitle);
        Assert.Equal(bookRequest.Metadata.Description, bookResponse.Metadata.Description);
        Assert.Equal(bookRequest.Metadata.Publisher, bookResponse.Metadata.Publisher);
        Assert.Equal(bookRequest.Metadata.PageCount, bookResponse.Metadata.PageCount);

        Assert.Equal(bookRequest.Metadata.ReleaseInfo!.OriginalReleaseDate, bookResponse.Metadata.ReleaseInfo!.OriginalReleaseDate);
        Assert.Equal(bookRequest.Metadata.ReleaseInfo.OriginalReleaseYear, bookResponse.Metadata.ReleaseInfo.OriginalReleaseYear);
        Assert.Equal(bookRequest.Metadata.ReleaseInfo.ReReleaseDate, bookResponse.Metadata.ReleaseInfo.ReReleaseDate);
        Assert.Equal(bookRequest.Metadata.ReleaseInfo.ReReleaseYear, bookResponse.Metadata.ReleaseInfo.ReReleaseYear);
        Assert.Equal(bookRequest.Metadata.ReleaseInfo.ReleaseCountry, bookResponse.Metadata.ReleaseInfo.ReleaseCountry);
        Assert.Equal(bookRequest.Metadata.ReleaseInfo.ReleaseVersion, bookResponse.Metadata.ReleaseInfo.ReleaseVersion);

        // language checks
        Assert.Equal(bookRequest.Metadata.Language!.LanguageCode, bookResponse.Metadata.Language!.LanguageCode);
        Assert.Equal(bookRequest.Metadata.Language.LanguageName, bookResponse.Metadata.Language.LanguageName);
        Assert.Equal(bookRequest.Metadata.Language.NativeName, bookResponse.Metadata.Language.NativeName);

        // original language checks
        Assert.Equal(bookRequest.Metadata.OriginalLanguage!.LanguageCode, bookResponse.Metadata.OriginalLanguage!.LanguageCode);
        Assert.Equal(bookRequest.Metadata.OriginalLanguage.LanguageName, bookResponse.Metadata.OriginalLanguage.LanguageName);
        Assert.Equal(bookRequest.Metadata.OriginalLanguage.NativeName, bookResponse.Metadata.OriginalLanguage.NativeName);

        // genres checks
        Assert.Equal(bookRequest.Metadata.Genres!.Count, bookResponse.Metadata.Genres!.Count);
        Assert.Equal(
            bookRequest.Metadata.Genres.Select(genre => genre.Name).OrderBy(x => x),
            bookResponse.Metadata.Genres.Select(genre => genre.Name).OrderBy(x => x));

        // tags checks
        Assert.Equal(bookRequest.Metadata.Tags!.Count, bookResponse.Metadata.Tags!.Count);
        Assert.Equal(
            bookRequest.Metadata.Tags.Select(tag => tag.Name).OrderBy(x => x),
            bookResponse.Metadata.Tags.Select(tag => tag.Name).OrderBy(x => x));

        // book specific properties
        Assert.Equal(bookRequest.Format, bookResponse.Format);
        Assert.Equal(bookRequest.Edition, bookResponse.Edition);
        Assert.Equal(bookRequest.VolumeNumber, bookResponse.VolumeNumber);
        Assert.Equal(bookRequest.ASIN, bookResponse.ASIN);
        Assert.Equal(bookRequest.GoodreadsId, bookResponse.GoodreadsId);
        Assert.Equal(bookRequest.LCCN, bookResponse.LCCN);
        Assert.Equal(bookRequest.OCLCNumber, bookResponse.OCLCNumber);
        Assert.Equal(bookRequest.OpenLibraryId, bookResponse.OpenLibraryId);
        Assert.Equal(bookRequest.LibraryThingId, bookResponse.LibraryThingId);
        Assert.Equal(bookRequest.GoogleBooksId, bookResponse.GoogleBooksId);
        Assert.Equal(bookRequest.BarnesAndNobleId, bookResponse.BarnesAndNobleId);
        Assert.Equal(bookRequest.AppleBooksId, bookResponse.AppleBooksId);

        // ISBNs checks
        Assert.Equal(bookRequest.ISBNs!.Count, bookResponse.ISBNs!.Count);
        var requestIsbnData = bookRequest.ISBNs.Select(isbn => new { isbn.Value, isbn.Format }).OrderBy(x => x.Value).ToList();
        var responseIsbnData = bookResponse.ISBNs.Select(isbn => new { isbn.Value, isbn.Format }).OrderBy(x => x.Value).ToList();
        Assert.Equal(requestIsbnData.Count, responseIsbnData.Count);
        for (int i = 0; i < requestIsbnData.Count; i++)
        {
            Assert.Equal(requestIsbnData[i].Value, responseIsbnData[i].Value);
            Assert.Equal(requestIsbnData[i].Format, responseIsbnData[i].Format);
        }

        // ratings checks
        Assert.Equal(bookRequest.Ratings!.Count, bookResponse.Ratings!.Count);
        var requestRatingData = bookRequest.Ratings.Select(r => new { r.Source, r.Value, r.MaxValue, r.VoteCount }).OrderBy(x => x.Source).ToList();
        var responseRatingData = bookResponse.Ratings.Select(r => new { r.Source, r.Value, r.MaxValue, r.VoteCount }).OrderBy(x => x.Source).ToList();
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
            Assert.Equal(bookRequest.Series.Title, bookResponse.Series!.Title);
        else
            Assert.Null(bookResponse.Series);

        // check Location header
        Assert.NotNull(response.Headers.Location);
        string locationUri = response.Headers.Location!.ToString();
        Assert.EndsWith($"/api/v1/libraries/{_libraryId}/books/{bookResponse.Id}", locationUri);

        // extract ID from Location header and compare
        string idFromHeader = locationUri.Split('/').Last();
        Assert.Equal(idFromHeader, bookResponse.Id.ToString());
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyTitle_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includeTitle: false));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.TitleCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthTitle_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(title: new Faker().Random.String2(300)));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.TitleMustBeMaximum255CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyOriginalTitle_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includeOriginalTitle: false));

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthOriginalTitle_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(originalTitle: new Faker().Random.String2(300)));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.OriginalTitleMustBeMaximum255CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyDescription_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includeDescription: false));

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthDescription_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(description: new Faker().Random.String2(2001)));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.DescriptionMustBeMaximum2000CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNullReleaseInfo_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includeReleaseInfo: false));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.ReleaseInfoCannotBeNull.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyOriginalReleaseYear_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(includeOriginalReleaseYear: false)));

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidValueOriginalReleaseYear_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 10000)));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.OriginalReleaseYearMustBeBetween1And9999.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyReReleaseYear_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(includeReReleaseYear: false, includeReReleaseDate: false)));

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidValueReReleaseYear_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(reReleaseYear: 10000)));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.ReReleaseYearMustBeBetween1And9999.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyReleaseCountry_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(includeReleaseCountry: false)));

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyReleaseVersion_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(includeReleaseVersion: false)));

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidValueReleasVersion_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(releaseVersion: new Faker().Random.String2(100))));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.ReleaseVersionMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenReReleaseYearIsAfterOriginalReleaseYear_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2000, 1, 1), originalReleaseYear: 2000, reReleaseDate: new DateOnly(2001, 1, 1), reReleaseYear: 2001)));

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenReReleaseYearIsBeforeReleaseYear_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2001, 1, 1), originalReleaseYear: 2001, reReleaseDate: new DateOnly(2000, 1, 1), reReleaseYear: 2000)));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.ReReleaseYearCannotBeEarlierThanOriginalReleaseYear.Description);
    }

    [Fact]
    public async Task AddBook_WhenReReleaseDateIsAfterOriginalReleaseDate_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2000, 1, 1), originalReleaseYear: 2000, reReleaseDate: new DateOnly(2001, 1, 1), reReleaseYear: 2001)));

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenReReleaseDateIsBeforeReleaseDate_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2001, 1, 1), originalReleaseYear: 2001, reReleaseDate: new DateOnly(2000, 1, 1), reReleaseYear: 2000)));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.ReReleaseDateCannotBeEarlierThanOriginalReleaseDate.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNullGenres_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includeGenres: false));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.GenresListCannotBeNull.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyGenreName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(includeName: false)]));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.GenreNameCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthGenreName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: new Faker().Random.String2(100))]));
        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.GenreNameMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNullTags_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includeTags: false));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.TagsListCannotBeNull.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyTagName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(includeName: false)]));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.TagNameCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthTagName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: new Faker().Random.String2(100))]));
        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.TagNameMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyLanguage_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includeLanguage: false));

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyLanguageCode_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(includeLanguageCode: false)));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.LanguageCodeCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthLanguageCode_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(10))));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.LanguageCodeMustBe2CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyLanguageName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(includeLanguageName: false)));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.LanguageNameCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthLanguageName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(100))));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyLanguageNativeName_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(includeNativeName: false)));

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthNativeLanguageName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(100))));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyOriginalLanguage_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includeOriginalLanguage: false));

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyOriginalLanguageCode_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(includeLanguageCode: false)));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.LanguageCodeCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthOriginalLanguageCode_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(10))));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.LanguageCodeMustBe2CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyOriginalLanguageName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(includeLanguageName: false)));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.LanguageNameCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthOriginalLanguageName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(100))));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.LanguageNameMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthOriginalNativeLanguageName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(100))));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.LanguageNativeNameMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyOriginalLanguageNativeName_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(includeNativeName: false)));

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyPublisher_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includePublisher: false));

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthPublisher_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(publisher: new Faker().Random.String2(150)));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.PublisherMustBeMaximum100CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyPageCount_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includePageCount: false));

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNegativePageCount_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(pageCount: -1));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.PageCountMustBeGreaterThanZero.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyFormat_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(isbns: [], contributors: [], ratings: [], includeFormat: false, includeEdition: false, includeVolumeNumber: false, includeAsin: false, includeGoodreadsId: false, includeLccn: false, includeOclcNumber: false, includeOpenLibraryId: false, includeLibraryThingId: false, includeGoogleBooksId: false, includeBarnesAndNobleId: false, includeAppleBooksId: false);

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidFormat_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(format: (BookFormat)99);

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.UnknownBookFormat.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyEdition_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(isbns: [], contributors: [], ratings: [], includeFormat: false, includeEdition: false, includeVolumeNumber: false, includeAsin: false, includeGoodreadsId: false, includeLccn: false, includeOclcNumber: false, includeOpenLibraryId: false, includeLibraryThingId: false, includeGoogleBooksId: false, includeBarnesAndNobleId: false, includeAppleBooksId: false);

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthEdition_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(edition: new Faker().Random.String2(100));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.EditionMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyVolumeNumber_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(isbns: [], contributors: [], ratings: [], includeFormat: false, includeEdition: false, includeVolumeNumber: false, includeAsin: false, includeGoodreadsId: false, includeLccn: false, includeOclcNumber: false, includeOpenLibraryId: false, includeLibraryThingId: false, includeGoogleBooksId: false, includeBarnesAndNobleId: false, includeAppleBooksId: false);

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNegativeVolumeNumber_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(volumeNumber: -1);

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.VolumeNumberMustBeGreaterThanZero.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptySeries_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create();

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    //[Fact]
    //public async Task AddBook_WhenCalledWithEmptySeriesTitle_ShouldReturnUnprocessableEntity()
    //{
    //    // Arrange
    //    var bookRequest = _requestBookFixture.Create();
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
    //    var bookRequest = _requestBookFixture.Create();
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
        AddBookRequest bookRequest = _requestBookFixture.Create(isbns: [], contributors: [], ratings: [], includeFormat: false, includeEdition: false, includeVolumeNumber: false, includeAsin: false, includeGoodreadsId: false, includeLccn: false, includeOclcNumber: false, includeOpenLibraryId: false, includeLibraryThingId: false, includeGoogleBooksId: false, includeBarnesAndNobleId: false, includeAppleBooksId: false);

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthAsin_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(asin: new Faker().Random.String2(15));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.AsinMustBe10CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyGoodreadsId_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(isbns: [], contributors: [], ratings: [], includeFormat: false, includeEdition: false, includeVolumeNumber: false, includeAsin: false, includeGoodreadsId: false, includeLccn: false, includeOclcNumber: false, includeOpenLibraryId: false, includeLibraryThingId: false, includeGoogleBooksId: false, includeBarnesAndNobleId: false, includeAppleBooksId: false);

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidGoodreadsId_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(goodreadsId: new Faker().Random.String2(2));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.GoodreadsIdMustBeNumeric.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyLccn_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(isbns: [], contributors: [], ratings: [], includeFormat: false, includeEdition: false, includeVolumeNumber: false, includeAsin: false, includeGoodreadsId: false, includeLccn: false, includeOclcNumber: false, includeOpenLibraryId: false, includeLibraryThingId: false, includeGoogleBooksId: false, includeBarnesAndNobleId: false, includeAppleBooksId: false);

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLccn_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(lccn: new Faker().Random.String2(200));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.InvalidLccnFormat.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyOclcNumber_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(isbns: [], contributors: [], ratings: [], includeFormat: false, includeEdition: false, includeVolumeNumber: false, includeAsin: false, includeGoodreadsId: false, includeLccn: false, includeOclcNumber: false, includeOpenLibraryId: false, includeLibraryThingId: false, includeGoogleBooksId: false, includeBarnesAndNobleId: false, includeAppleBooksId: false);

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidOclcNumber_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(oclcNumber: new Faker().Random.String2(200));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.InvalidOclcFormat.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyOpenLibraryId_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(isbns: [], contributors: [], ratings: [], includeFormat: false, includeEdition: false, includeVolumeNumber: false, includeAsin: false, includeGoodreadsId: false, includeLccn: false, includeOclcNumber: false, includeOpenLibraryId: false, includeLibraryThingId: false, includeGoogleBooksId: false, includeBarnesAndNobleId: false, includeAppleBooksId: false);

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidOpenLibraryId_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(openLibraryId: new Faker().Random.String2(200));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.InvalidOpenLibraryId.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyLibraryThingId_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(isbns: [], contributors: [], ratings: [], includeFormat: false, includeEdition: false, includeVolumeNumber: false, includeAsin: false, includeGoodreadsId: false, includeLccn: false, includeOclcNumber: false, includeOpenLibraryId: false, includeLibraryThingId: false, includeGoogleBooksId: false, includeBarnesAndNobleId: false, includeAppleBooksId: false);

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthLibraryThingId_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(libraryThingId: new Faker().Random.String2(200));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.LibraryThingIdMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyGoogleBooksId_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(isbns: [], contributors: [], ratings: [], includeFormat: false, includeEdition: false, includeVolumeNumber: false, includeAsin: false, includeGoodreadsId: false, includeLccn: false, includeOclcNumber: false, includeOpenLibraryId: false, includeLibraryThingId: false, includeGoogleBooksId: false, includeBarnesAndNobleId: false, includeAppleBooksId: false);

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthGoogleBooksId_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(googleBooksId: new Faker().Random.String2(20));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.GoogleBooksIdMustBe12CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidGoogleBooksId_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(googleBooksId: new Faker().Random.String2(11) + " ");

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.InvalidGoogleBooksIdFormat.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyBarnesAndNobleId_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(isbns: [], contributors: [], ratings: [], includeFormat: false, includeEdition: false, includeVolumeNumber: false, includeAsin: false, includeGoodreadsId: false, includeLccn: false, includeOclcNumber: false, includeOpenLibraryId: false, includeLibraryThingId: false, includeGoogleBooksId: false, includeBarnesAndNobleId: false, includeAppleBooksId: false);

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthBarnesAndNobleId_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(barnesAndNobleId: new Faker().Random.String2(20));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.BarnesAndNoblesIdMustBe10CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidBarnesAndNobleId_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(barnesAndNobleId: new Faker().Random.String2(10));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.InvalidBarnesAndNoblesIdFormat.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyAppleBooksId_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(isbns: [], contributors: [], ratings: [], includeFormat: false, includeEdition: false, includeVolumeNumber: false, includeAsin: false, includeGoodreadsId: false, includeLccn: false, includeOclcNumber: false, includeOpenLibraryId: false, includeLibraryThingId: false, includeGoogleBooksId: false, includeBarnesAndNobleId: false, includeAppleBooksId: false);

        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidAppleBooksId_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(appleBooksId: new Faker().Random.String2(10));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.InvalidAppleBooksIdFormat.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNullIsbns_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(includeFormat: false, includeEdition: false, includeVolumeNumber: false, includeAsin: false, includeGoodreadsId: false, includeLccn: false, includeOclcNumber: false, includeOpenLibraryId: false, includeLibraryThingId: false, includeGoogleBooksId: false, includeBarnesAndNobleId: false, includeAppleBooksId: false, includeIsbns: false, includeRatings: false, includeContributors: false);

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.IsbnListCannotBeNull.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyIsbnValue_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(isbns: [_isbnDtoFixture.Create(value: string.Empty)]);

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.IsbnValueCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidIsbn10Value_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(isbns: [_isbnDtoFixture.Create(value: new Faker().Random.String2(5), format: IsbnFormat.Isbn10)]);

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.InvalidIsbn10Format.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidIsbn13Value_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(isbns: [_isbnDtoFixture.Create(value: new Faker().Random.String2(5), format: IsbnFormat.Isbn13)]);

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.InvalidIsbn13Format.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidIsbnFormat_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(isbns: [_isbnDtoFixture.Create(format: (IsbnFormat)99)]);
        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.UnknownIsbnFormat.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNullContributors_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create() with { Contributors = null };

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.MediaContributor.ContributorsListCannotBeNull.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyContributorId_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(contributorId: Guid.Empty, role: MediaContributorRole.Author)]);

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.MediaContributor.MediaContributorIdCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidContributorRole_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(contributorId: Guid.NewGuid(), role: (MediaContributorRole)999)]);

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.MediaContributor.UnknownMediaContributorRole.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNullRatings_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(includeFormat: false, includeEdition: false, includeVolumeNumber: false, includeAsin: false, includeGoodreadsId: false, includeLccn: false, includeOclcNumber: false, includeOpenLibraryId: false, includeLibraryThingId: false, includeGoogleBooksId: false, includeBarnesAndNobleId: false, includeAppleBooksId: false, includeIsbns: false, includeRatings: false, includeContributors: false);

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.RatingsListCannotBeNull.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNegativeRatingValue_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(ratings: [_bookRatingDtoFixture.Create(value: -3)]);
        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.RatingValueMustBePositive.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithRatingValueGreaterThanMaxValue_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(ratings: [_bookRatingDtoFixture.Create(value: 3, maxValue: 2)]);
        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.RatingValueCannotBeGreaterThanMaxValue.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNegativeMaxRatingValue_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(ratings: [_bookRatingDtoFixture.Create(maxValue: -3)]);
        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.RatingMaxValueMustBePositive.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyRatingVoteCount_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(ratings: [_bookRatingDtoFixture.Create(source: BookRatingSource.Goodreads, includeVoteCount: false)]);
        // Act & Assert
        await AssertCreated(bookRequest);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNegativeVoteCount_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(ratings: [_bookRatingDtoFixture.Create(voteCount: -3)]);
        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.Metadata.RatingVoteCountMustBePositive.Description);
    }

    [Fact]
    public async Task AddBook_WhenTwoContributorsHaveTheSameIdWithDifferentRoles_ShouldCreateTwoBookLinks()
    {
        // Arrange
        Guid contributorId = Guid.NewGuid();
        AddBookRequest bookRequest = _requestBookFixture.Create(contributors:
            [
                _mediaContributorReferenceDtoFixture.Create(contributorId: contributorId, role: MediaContributorRole.Author),
                _mediaContributorReferenceDtoFixture.Create(contributorId: contributorId, role: MediaContributorRole.Illustrator)
            ]);

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        BookResponse? bookResponse = await response.Content.ReadFromJsonAsync<BookResponse>(_jsonOptions);
        Assert.NotNull(bookResponse);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        List<BookContributorEntity> links = [.. dbContext.Books
            .Include(book => book.Contributors)
            .Single(book => book.Id == bookResponse!.Id)
            .Contributors];
        Assert.Equal(2, links.Count);
        Assert.All(links, link => Assert.Equal(contributorId, link.MediaContributorId));
        Assert.Contains(links, link => link.Role == MediaContributorRole.Author);
        Assert.Contains(links, link => link.Role == MediaContributorRole.Illustrator);
    }

    [Fact]
    public async Task AddBook_WhenBookPathIsNotWithinTheLibraryContentLocations_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        string pathOutsideTheLibrary = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "..", "lumina-books-outside", $"{Guid.NewGuid():N}.epub"));
        AddBookRequest bookRequest = _requestBookFixture.Create(path: pathOutsideTheLibrary);

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.BookPathMustBeWithinLibraryContentLocations.Description);
    }

    [Fact]
    public async Task AddBook_WhenBookPathAlreadyExistsInTheLibrary_ShouldReturnConflict()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create();

        // Act
        HttpResponseMessage firstResponse = await PostBookAsync(bookRequest);
        HttpResponseMessage secondResponse = await PostBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        Assert.Equal("application/problem+json", secondResponse.Content.Headers.ContentType?.MediaType);

        string content = await secondResponse.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status409Conflict, problemDetails!["status"].GetInt32());
        Assert.Equal(Errors.WrittenContent.BookAlreadyExists.Description, problemDetails["detail"].GetString());
    }

    [Fact]
    public async Task AddBook_WhenAReferencedContributorDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create()]);

        // Act
        HttpResponseMessage response = await PostBookWithoutSeedingContributorsAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails!["status"].GetInt32());
        Assert.Equal(Errors.MediaContributor.MediaContributorNotFound.Description, problemDetails["detail"].GetString());
    }

    [Fact]
    public async Task AddBook_WhenCalledWithTooLongPath_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(path: "/" + new Faker().Random.String2(2048) + ".epub");

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.WrittenContent.BookPathMustBeMaximum2048CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenTheLibraryDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        // only an admin reaches the library lookup of a library that does not exist, because a regular user is rejected by the ownership policy before it
        HttpClient adminClient = await _apiFactory.CreateAuthenticatedAdminClientAsync();
        Guid missingLibraryId = Guid.NewGuid();
        AddBookRequest bookRequest = _requestBookFixture.Create(contributors: []);

        // Act
        HttpResponseMessage response = await adminClient.PostAsJsonAsync($"/api/v1/libraries/{missingLibraryId}/books", bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails!["status"].GetInt32());
        Assert.Equal(Errors.Library.LibraryNotFound.Description, problemDetails["detail"].GetString());

        await _apiFactory.RemoveTestUserAsync();
    }


    [Fact]
    public async Task AddBook_WhenTitleIsValid_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(title: new Faker().Random.String2(200)));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddBook_WhenOriginalTitleIsValid_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(originalTitle: new Faker().Random.String2(200)));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddBook_WhenDescriptionIsValid_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(description: new Faker().Random.String2(1500)));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddBook_WhenOriginalReleaseYearIsValid_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(includeOriginalReleaseDate: false, originalReleaseYear: new Faker().Random.Int(2000, 2005), reReleaseYear: new Faker().Random.Int(2005, 2010), includeReReleaseDate: false)));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddBook_WhenReReleaseYearIsValid_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: new Faker().Random.Int(2000, 2005), reReleaseYear: new Faker().Random.Int(2005, 2010), includeReReleaseDate: false)));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddBook_WhenReleaseVersionIsValid_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(releaseVersion: new Faker().Random.String2(50))));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddBook_WhenGenresAreValid_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: new Faker().Random.String2(50))]));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddBook_WhenTagsAreValid_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: new Faker().Random.String2(50))]));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddBook_WhenCalledWithValidPublisher_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(publisher: new Faker().Random.String2(100)));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddBook_WhenPageCountIsPositive_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(pageCount: 100));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddBook_WhenFormatIsValid_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(format: BookFormat.Hardcover);

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddBook_WhenEditionIsValid_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(edition: new Faker().Random.String2(50));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddBook_WhenVolumeNumberIsPositive_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(volumeNumber: 1);

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddBook_WhenAsinIsValid_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(asin: new Faker().Random.String2(10));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddBook_WhenGoodreadsIdIsValid_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(goodreadsId: "123456789");

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddBook_WhenLccnIsValid_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(lccn: "n78890351");

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddBook_WhenCalledWithValidOclcNumberFormat1_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(oclcNumber: "ocm12345678");

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddBook_WhenCalledWithValidOclcNumberFormat2_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(oclcNumber: "ocn123456789");

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddBook_WhenCalledWithValidOclcNumberFormat3_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(oclcNumber: "on1234567890");

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddBook_WhenCalledWithValidOclcNumberFormat4_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(oclcNumber: "(OCoLC)1234567890");

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddBook_WhenCalledWithValidOclcNumberFormat5_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(oclcNumber: "12345678");

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddBook_WhenOpenLibraryIdIsValid_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(openLibraryId: "OL123456M");

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddBook_WhenCalledWithValidLibraryThingId_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(libraryThingId: new Faker().Random.String2(50));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddBook_WhenCalledWithValidGoogleBooksId_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(googleBooksId: new Faker().Random.String2(12, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-"));

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddBook_WhenCalledWithValidBarnesAndNobleId_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(barnesAndNobleId: new Faker().Random.Number(1000000000, 999999999).ToString());

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddBook_WhenCalledWithValidAppleBooksId_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(appleBooksId: "id123456");

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddBook_WhenCalledWithValidIsbn10_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(isbns: [_isbnDtoFixture.Create(value: "0-306-40615-2", format: IsbnFormat.Isbn10)]);

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddBook_WhenCalledWithValidIsbn13_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(isbns: [_isbnDtoFixture.Create(value: "978-3-16-148410-0", format: IsbnFormat.Isbn13)]);

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddBook_WhenContributorsAreValid_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(role: MediaContributorRole.Author)]);

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddBook_WhenCalledWithValidRatings_ShouldAddBook()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(ratings: [_bookRatingDtoFixture.Create(value: 4, maxValue: 5, voteCount: 100)]);

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddBook_WhenLibraryIdIsNotParseable_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(contributors: []);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/libraries/not-a-guid/books", bookRequest);

        // Assert
        // The route value is kept as a raw string, so the unparseable Id becomes an empty Guid in the command mapping and the
        // command validator reports a clean validation error instead of failing the request binding.
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.Validation", problemDetails.RootElement.GetProperty("title").GetString());
        Assert.Contains("LibraryIdCannotBeEmpty", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AddBook_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();
        Guid libraryId = Guid.NewGuid();
        AddBookRequest request = _requestBookFixture.Create(contributors: []);

        // Act
        HttpResponseMessage response = await unauthenticatedClient.PostAsJsonAsync($"/api/v1/libraries/{libraryId}/books", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithCancellationToken_ShouldCompleteSuccessfully()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(contributors: []);
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));

        // Act & Assert
        Exception? exception = await Record.ExceptionAsync(async () =>
            await _client.PostAsJsonAsync($"/api/v1/libraries/{_libraryId}/books", bookRequest, cts.Token)
        );
        Assert.Null(exception);
    }

    [Fact]
    public async Task AddBook_WhenCancellationTokenIsCanceled_ShouldThrowTaskCanceledException()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(contributors: []);
        using CancellationTokenSource cts = new();

        // Act & Assert
        Exception? exception = await Record.ExceptionAsync(async () =>
        {
            cts.Cancel();
            await _client.PostAsJsonAsync($"/api/v1/libraries/{_libraryId}/books", bookRequest, cts.Token);
        });
        Assert.IsType<TaskCanceledException>(exception);
    }

    /// <summary>
    /// Posts a request to add a book to the media library owned by the authenticated user of the current test.
    /// </summary>
    /// <param name="request">The request to post.</param>
    /// <returns>The HTTP response of the POST request.</returns>
    private async Task<HttpResponseMessage> PostBookAsync(AddBookRequest request)
    {
        await SeedContributorsAsync(request.Contributors);
        return await _client.PostAsJsonAsync($"/api/v1/libraries/{_libraryId}/books", request);
    }

    /// <summary>
    /// Posts a request to add a book to the media library owned by the authenticated user of the current test, without seeding the contributors it references.
    /// </summary>
    /// <param name="request">The request to post.</param>
    /// <returns>The HTTP response of the POST request.</returns>
    private Task<HttpResponseMessage> PostBookWithoutSeedingContributorsAsync(AddBookRequest request)
    {
        return _client.PostAsJsonAsync($"/api/v1/libraries/{_libraryId}/books", request);
    }

    /// <summary>
    /// Seeds the media contributors referenced by the provided book contributors, so that the handler finds them already existing.
    /// </summary>
    /// <param name="contributors">The contributors of the book request.</param>
    private async Task SeedContributorsAsync(List<MediaContributorReferenceDto>? contributors)
    {
        if (contributors is null || contributors.Count == 0)
            return;

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        foreach (Guid contributorId in contributors.Select(contributor => contributor.ContributorId).Distinct())
        {
            if (contributorId == Guid.Empty || await dbContext.MediaContributors.AnyAsync(contributor => contributor.Id == contributorId))
                continue;
            dbContext.MediaContributors.Add(_mediaContributorEntityFixture.Create(id: contributorId, displayName: contributorId.ToString()));
        }
        await dbContext.SaveChangesAsync();
    }

    private async Task AssertUnprocessableEntityWithValidationErrors(HttpResponseMessage response, params string[] expectedErrorCodes)
    {
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal($"/api/v1/libraries/{_libraryId}/books", problemDetails["instance"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", problemDetails["type"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        Assert.NotNull(errors);
        Assert.Contains("General.Validation", errors.Keys);
        Assert.All(expectedErrorCodes, code => Assert.Contains(code, errors["General.Validation"]));
    }

    private async Task AssertCreated(AddBookRequest bookRequest)
    {
        HttpResponseMessage response = await PostBookAsync(bookRequest);
        response.EnsureSuccessStatusCode();
        BookResponse? bookResponse = await response.Content.ReadFromJsonAsync<BookResponse>(_jsonOptions);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(bookResponse);
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _apiFactory.RemoveTestUserAsync();
    }
}