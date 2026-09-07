#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.IntegrationTests.Common.Converters;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        ReferenceHandler = ReferenceHandler.Preserve,
        PropertyNameCaseInsensitive = true,
        Converters = {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
            new BookJsonConverter()
        }
    };
    private readonly AddBookRequestFixture _requestBookFixture = new();
    private readonly WrittenContentMetadataDtoFixture _writtenContentMetadataDtoFixture = new();
    private readonly ReleaseInfoDtoFixture _releaseInfoDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly TagDtoFixture _tagDtoFixture = new();
    private readonly LanguageInfoDtoFixture _languageInfoDtoFixture = new();
    private readonly IsbnDtoFixture _isbnDtoFixture = new();
    private readonly BookRatingDtoFixture _bookRatingDtoFixture = new();
    private readonly MediaContributorDtoFixture _mediaContributorDtoFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
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
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: _libraryId, userId: userId, title: "Test Library", libraryType: LibraryType.EBook, contentLocations: []));
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
        Assert.Equal(bookRequest.Metadata.ReleaseInfo!.ReleaseCountry, bookResponse.Metadata.ReleaseInfo.ReleaseCountry.Value);
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
        var responseIsbnData = bookResponse.ISBNs.Select(isbn => new { isbn.Value, isbn.Format }).OrderBy(x => x.Value).ToList();
        Assert.Equal(requestIsbnData.Count, responseIsbnData.Count);
        for (int i = 0; i < requestIsbnData.Count; i++)
        {
            Assert.Equal(requestIsbnData[i].Value, responseIsbnData[i].Value);
            Assert.Equal(requestIsbnData[i].Format, responseIsbnData[i].Format);
        }

        // ratings checks
        Assert.Equal(bookRequest.Ratings!.Count, bookResponse.Ratings.Count);
        var requestRatingData = bookRequest.Ratings.Select(r => new { r.Source, r.Value, r.MaxValue, r.VoteCount }).OrderBy(x => x.Source).ToList();
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
        AddBookRequest bookRequest = _requestBookFixture.Create(isbns: [], contributors: [], ratings: [], includeOptionalProperties: false);

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
        AddBookRequest bookRequest = _requestBookFixture.Create(isbns: [], contributors: [], ratings: [], includeOptionalProperties: false);

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
        AddBookRequest bookRequest = _requestBookFixture.Create(isbns: [], contributors: [], ratings: [], includeOptionalProperties: false);

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
        AddBookRequest bookRequest = _requestBookFixture.Create(isbns: [], contributors: [], ratings: [], includeOptionalProperties: false);

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
        AddBookRequest bookRequest = _requestBookFixture.Create(isbns: [], contributors: [], ratings: [], includeOptionalProperties: false);

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
        AddBookRequest bookRequest = _requestBookFixture.Create(isbns: [], contributors: [], ratings: [], includeOptionalProperties: false);

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
        AddBookRequest bookRequest = _requestBookFixture.Create(isbns: [], contributors: [], ratings: [], includeOptionalProperties: false);

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
        AddBookRequest bookRequest = _requestBookFixture.Create(isbns: [], contributors: [], ratings: [], includeOptionalProperties: false);

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
        AddBookRequest bookRequest = _requestBookFixture.Create(isbns: [], contributors: [], ratings: [], includeOptionalProperties: false);

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
        AddBookRequest bookRequest = _requestBookFixture.Create(isbns: [], contributors: [], ratings: [], includeOptionalProperties: false);

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
        AddBookRequest bookRequest = _requestBookFixture.Create(isbns: [], contributors: [], ratings: [], includeOptionalProperties: false);

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
        AddBookRequest bookRequest = _requestBookFixture.Create(isbns: [], contributors: [], ratings: [], includeOptionalProperties: false);

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
        AddBookRequest bookRequest = _requestBookFixture.Create(includeOptionalProperties: false);

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
        AddBookRequest bookRequest = _requestBookFixture.Create(includeOptionalProperties: false);

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.MediaContributor.ContributorsListCannotBeNull.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNullContributorName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(contributors: [_mediaContributorDtoFixture.Create(roleName: "author", roleCategory: MediaContributorRoleCategory.Author, includeName: false)]);

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.MediaContributor.ContributorNameCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyContributorDisplayName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(contributors: [_mediaContributorDtoFixture.Create(displayName: string.Empty)]);

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.MediaContributor.ContributorDisplayNameCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNullContributorDisplayName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(contributors: [_mediaContributorDtoFixture.Create(includeDisplayName: false, includeLegalName: false, roleName: "author", roleCategory: MediaContributorRoleCategory.Author)]);

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.MediaContributor.ContributorDisplayNameCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthContributorDisplayName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(contributors: [_mediaContributorDtoFixture.Create(displayName: new Faker().Random.String2(150))]);

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.MediaContributor.ContributorDisplayNameMustBeMaximum100CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthContributorLegalName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(contributors: [_mediaContributorDtoFixture.Create(displayName: new Faker().Random.String2(50), legalName: new Faker().Random.String2(150), roleName: "author", roleCategory: MediaContributorRoleCategory.Author)]);

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.MediaContributor.ContributorLegalNameMustBeMaximum100CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyContributorRole_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(contributors: [_mediaContributorDtoFixture.Create(displayName: new Faker().Random.String2(50), includeRole: false)]);

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.MediaContributor.ContributorRoleCannotBeNull.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyContributorRoleName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(contributors: [_mediaContributorDtoFixture.Create(displayName: new Faker().Random.String2(50), includeRoleName: false, roleCategory: MediaContributorRoleCategory.Author)]);

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.MediaContributor.RoleNameCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithInvalidLengthContributorRoleName_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(contributors: [_mediaContributorDtoFixture.Create(displayName: new Faker().Random.String2(50), roleName: new Faker().Random.String2(100), roleCategory: MediaContributorRoleCategory.Author)]);

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.MediaContributor.RoleNameMustBeMaximum50CharactersLong.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithEmptyContributorRoleCategory_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(contributors: [_mediaContributorDtoFixture.Create(displayName: new Faker().Random.String2(50), roleName: "author", includeRoleCategory: false)]);

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, Errors.MediaContributor.RoleCategoryCannotBeEmpty.Description);
    }

    [Fact]
    public async Task AddBook_WhenCalledWithNullRatings_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(includeOptionalProperties: false);

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
        AddBookRequest bookRequest = _requestBookFixture.Create(ratings: [_bookRatingDtoFixture.Create(source: BookRatingSource.Goodreads, includeOptionalProperties: false)]);
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
    public async Task AddBook_WhenTwoContributorsShareTheSameDisplayName_ShouldCreateOneContributorWithTwoBookLinks()
    {
        // Arrange
        AddBookRequest bookRequest = _requestBookFixture.Create(contributors:
            [
                _mediaContributorDtoFixture.Create(displayName: "Duplicated Author", legalName: "Duplicated Author Legal", roleName: "author", roleCategory: MediaContributorRoleCategory.Author),
                _mediaContributorDtoFixture.Create(displayName: "Duplicated Author", legalName: "Duplicated Author Legal", roleName: "illustrator", roleCategory: MediaContributorRoleCategory.Illustrator)
            ]);

        // Act
        HttpResponseMessage response = await PostBookAsync(bookRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Book? bookResponse = await response.Content.ReadFromJsonAsync<Book>(_jsonOptions);
        Assert.NotNull(bookResponse);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        MediaContributorEntity contributor = dbContext.MediaContributors.Single(mediaContributor => mediaContributor.DisplayName == "Duplicated Author");
        List<BookContributorEntity> links = [.. dbContext.Books
            .Include(book => book.BookContributors)
            .Single(book => book.Id == bookResponse!.Id.Value)
            .BookContributors];
        Assert.Equal(2, links.Count);
        Assert.All(links, link => Assert.Equal(contributor.Id, link.MediaContributorId));
        Assert.Contains(links, link => link.RoleName == "author");
        Assert.Contains(links, link => link.RoleName == "illustrator");
    }

    /// <summary>
    /// Posts a request to add a book to the media library owned by the authenticated user of the current test.
    /// </summary>
    /// <param name="request">The request to post.</param>
    /// <returns>The HTTP response of the POST request.</returns>
    private Task<HttpResponseMessage> PostBookAsync(AddBookRequest request)
    {
        return _client.PostAsJsonAsync("/api/v1/books", request with { LibraryId = _libraryId });
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
        Book? bookResponse = await response.Content.ReadFromJsonAsync<Book>(_jsonOptions);
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
