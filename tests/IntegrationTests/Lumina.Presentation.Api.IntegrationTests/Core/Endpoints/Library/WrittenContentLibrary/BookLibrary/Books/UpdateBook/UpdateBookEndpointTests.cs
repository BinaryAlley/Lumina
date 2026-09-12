#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.UpdateBook;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
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

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.UpdateBook;

/// <summary>
/// Contains integration tests for the <see cref="UpdateBookEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateBookEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly UpdateBookRequestFixture _requestBookFixture = new();
    private readonly MediaContributorReferenceDtoFixture _mediaContributorReferenceDtoFixture = new();
    private readonly WrittenContentMetadataDtoFixture _writtenContentMetadataDtoFixture = new();
    private readonly ReleaseInfoDtoFixture _releaseInfoDtoFixture = new();
    private readonly GenreDtoFixture _genreDtoFixture = new();
    private readonly TagDtoFixture _tagDtoFixture = new();
    private readonly LanguageInfoDtoFixture _languageInfoDtoFixture = new();
    private readonly IsbnDtoFixture _isbnDtoFixture = new();
    private readonly BookRatingDtoFixture _bookRatingDtoFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly MediaContributorEntityFixture _mediaContributorEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBookEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public UpdateBookEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        _apiFactory = apiFactory;
    }

    /// <summary>
    /// Initializes authenticated API client.
    /// </summary>
    public async Task InitializeAsync()
    {
        _client = await _apiFactory.CreateAuthenticatedClientAsync();
    }

    [Fact]
    public async Task UpdateBook_WhenCalledByOwnerWithValidData_ShouldUpdateBookAndPersistChanges()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid bookId) = await SeedLibraryAndBookAsync(userId, "Original Title");
        UpdateBookRequest request = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(title: "Updated Title"), isbns: [], contributors: [], ratings: [], includeOptionalProperties: false);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/books/{bookId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        BookResponse? bookResponse = JsonSerializer.Deserialize<BookResponse>(content, _jsonOptions);
        Assert.NotNull(bookResponse);
        Assert.Equal(bookId, bookResponse!.Id);
        Assert.Equal(libraryId, bookResponse.LibraryId);
        Assert.Equal("Updated Title", bookResponse.Metadata!.Title);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        BookEntity storedBook = dbContext.Books.Single(book => book.Id == bookId);
        Assert.Equal("Updated Title", storedBook.Title);
    }

    [Fact]
    public async Task UpdateBook_WhenTwoContributorsHaveTheSameIdWithDifferentRoles_ShouldCreateTwoBookLinks()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid bookId) = await SeedLibraryAndBookAsync(userId, "Original Title");
        Guid contributorId = Guid.NewGuid();
        List<MediaContributorReferenceDto> duplicatedContributors =
        [
            _mediaContributorReferenceDtoFixture.Create(contributorId: contributorId, role: MediaContributorRole.Author),
            _mediaContributorReferenceDtoFixture.Create(contributorId: contributorId, role: MediaContributorRole.Illustrator)
        ];
        await SeedContributorsAsync(duplicatedContributors);
        UpdateBookRequest request = _requestBookFixture.Create(contributors: duplicatedContributors, isbns: [], ratings: [], includeOptionalProperties: false);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/books/{bookId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        List<BookContributorEntity> links = dbContext.Books
            .Include(book => book.BookContributors)
            .Single(book => book.Id == bookId)
            .BookContributors
            .ToList();
        Assert.Equal(2, links.Count);
        Assert.All(links, link => Assert.Equal(contributorId, link.MediaContributorId));
        Assert.Contains(links, link => link.Role == MediaContributorRole.Author);
        Assert.Contains(links, link => link.Role == MediaContributorRole.Illustrator);
    }

    [Fact]
    public async Task UpdateBook_WhenRouteBookDoesNotExist_ShouldNotUpdateAnyBook()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid bodyBookId) = await SeedLibraryAndBookAsync(userId, "Body Book Title");
        Guid routeId = Guid.NewGuid();
        UpdateBookRequest request = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(title: "Should Not Apply"));

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/books/{routeId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        BookEntity storedBook = dbContext.Books.Single(book => book.Id == bodyBookId);
        Assert.Equal("Body Book Title", storedBook.Title);
    }

    [Fact]
    public async Task UpdateBook_WhenRouteIdIsNotParseable_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        UpdateBookRequest request = _requestBookFixture.Create();

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/books/not-a-guid", request);

        // Assert
        // The route value is kept as a raw string, so the unparseable Id becomes Guid.Empty in the command mapping and the
        // command validator reports a clean validation error instead of failing the request binding.
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.Validation", problemDetails.RootElement.GetProperty("title").GetString());
        Assert.Contains("BookIdCannotBeEmpty", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateBook_WhenBookDoesNotExist_ShouldReturnBookNotFoundProblem()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        UpdateBookRequest request = _requestBookFixture.Create();

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/books/{Guid.NewGuid()}", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.NotFound", problemDetails.RootElement.GetProperty("title").GetString());
        Assert.Equal("BookNotFound", problemDetails.RootElement.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task UpdateBook_WhenBookBelongsToAnotherUserLibrary_ShouldReturnForbiddenProblem()
    {
        // Arrange
        (Guid otherUserId, _) = await SeedOtherUserAsync();
        (Guid otherLibraryId, Guid bookId) = await SeedLibraryAndBookAsync(otherUserId, "Other User Book");
        UpdateBookRequest request = _requestBookFixture.Create();

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/libraries/{otherLibraryId}/books/{bookId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.Unauthorized", problemDetails.RootElement.GetProperty("title").GetString());
        Assert.Equal("NotAuthorized", problemDetails.RootElement.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithInvalidBody_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid bookId) = await SeedLibraryAndBookAsync(userId, "Original Title");
        UpdateBookRequest request = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includeTitle: false));

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/books/{bookId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.Validation", problemDetails.RootElement.GetProperty("title").GetString());
        Assert.Contains("TitleCannotBeEmpty", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateBook_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();
        Guid libraryId = Guid.NewGuid();
        UpdateBookRequest request = _requestBookFixture.Create();

        // Act
        HttpResponseMessage response = await unauthenticatedClient.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/books/{Guid.NewGuid()}", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }


    [Fact]
    public async Task UpdateBook_WhenTitleIsNull_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includeTitle: false));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "TitleCannotBeEmpty");
    }

    [Fact]
    public async Task UpdateBook_WhenTitleExceeds255Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(title: new Faker().Random.String2(300)));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "TitleMustBeMaximum255CharactersLong");
    }

    [Fact]
    public async Task UpdateBook_WhenTitleIsValid_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(title: new Faker().Random.String2(200)));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenOriginalTitleExceeds255Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(originalTitle: new Faker().Random.String2(300)));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "OriginalTitleMustBeMaximum255CharactersLong");
    }

    [Fact]
    public async Task UpdateBook_WhenOriginalTitleIsValid_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(originalTitle: new Faker().Random.String2(200)));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenOriginalTitleIsNull_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includeOriginalTitle: false));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenDescriptionExceeds2000Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(description: new Faker().Random.String2(2001)));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "DescriptionMustBeMaximum2000CharactersLong");
    }

    [Fact]
    public async Task UpdateBook_WhenDescriptionIsValid_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(description: new Faker().Random.String2(1500)));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenDescriptionIsNull_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includeDescription: false));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenDescriptionIsEmpty_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(description: string.Empty));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenReleaseInfoIsNull_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includeReleaseInfo: false));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "ReleaseInfoCannotBeNull");
    }

    [Fact]
    public async Task UpdateBook_WhenOriginalReleaseYearIsValid_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(includeOriginalReleaseDate: false, originalReleaseYear: new Faker().Random.Int(2000, 2005), reReleaseYear: new Faker().Random.Int(2005, 2010), includeReReleaseDate: false)));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenOriginalReleaseYearIsLessThan1_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: new ReleaseInfoDto(null, 0, null, null, null, null)));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "OriginalReleaseYearMustBeBetween1And9999");
    }

    [Fact]
    public async Task UpdateBook_WhenOriginalReleaseYearIsGreaterThan9999_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: new ReleaseInfoDto(null, 10000, null, null, null, null)));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "OriginalReleaseYearMustBeBetween1And9999");
    }

    [Fact]
    public async Task UpdateBook_WhenReReleaseYearIsValid_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: new Faker().Random.Int(2000, 2005), reReleaseYear: new Faker().Random.Int(2005, 2010), includeReReleaseDate: false)));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenReReleaseYearIsLessThan1_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: new ReleaseInfoDto(null, null, null, 0, null, null)));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "ReReleaseYearMustBeBetween1And9999");
    }

    [Fact]
    public async Task UpdateBook_WhenReReleaseYearIsGreaterThan9999_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: new ReleaseInfoDto(null, null, null, 10000, null, null)));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "ReReleaseYearMustBeBetween1And9999");
    }

    [Fact]
    public async Task UpdateBook_WhenReleaseVersionIsValid_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(releaseVersion: new Faker().Random.String2(50))));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenReleaseVersionExceeds50Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(releaseVersion: new Faker().Random.String2(51))));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "ReleaseVersionMustBeMaximum50CharactersLong");
    }

    [Fact]
    public async Task UpdateBook_WhenReReleaseYearIsAfterOriginalReleaseYear_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 2000, reReleaseYear: 2001, includeReReleaseDate: false)));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenReReleaseYearIsBeforeOriginalReleaseYear_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseYear: 2001, reReleaseYear: 2000, includeReReleaseDate: false)));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "ReReleaseYearCannotBeEarlierThanOriginalReleaseYear");
    }

    [Fact]
    public async Task UpdateBook_WhenReReleaseDateIsAfterOriginalReleaseDate_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2000, 1, 1), reReleaseDate: new DateOnly(2001, 1, 1))));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenReReleaseDateIsBeforeOriginalReleaseDate_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(releaseInfo: _releaseInfoDtoFixture.Create(originalReleaseDate: new DateOnly(2001, 1, 1), reReleaseDate: new DateOnly(2000, 1, 1))));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "ReReleaseDateCannotBeEarlierThanOriginalReleaseDate");
    }

    [Fact]
    public async Task UpdateBook_WhenGenresIsNull_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includeGenres: false));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "GenresListCannotBeNull");
    }

    [Fact]
    public async Task UpdateBook_WhenGenreNameIsEmpty_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: string.Empty)]));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "GenreNameCannotBeEmpty");
    }

    [Fact]
    public async Task UpdateBook_WhenGenreNameExceeds50Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: new Faker().Random.String2(51))]));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "GenreNameMustBeMaximum50CharactersLong");
    }

    [Fact]
    public async Task UpdateBook_WhenGenresAreValid_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(genres: [_genreDtoFixture.Create(name: new Faker().Random.String2(50))]));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenTagsIsNull_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includeTags: false));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "TagsListCannotBeNull");
    }

    [Fact]
    public async Task UpdateBook_WhenTagNameIsEmpty_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: string.Empty)]));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "TagNameCannotBeEmpty");
    }

    [Fact]
    public async Task UpdateBook_WhenTagNameExceeds50Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: new Faker().Random.String2(51))]));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "TagNameMustBeMaximum50CharactersLong");
    }

    [Fact]
    public async Task UpdateBook_WhenTagsAreValid_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(tags: [_tagDtoFixture.Create(name: new Faker().Random.String2(50))]));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenLanguageIsNull_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includeLanguage: false));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenLanguageCodeIsEmpty_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: string.Empty)));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "LanguageCodeCannotBeEmpty");
    }

    [Fact]
    public async Task UpdateBook_WhenLanguageCodeExceeds2Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(3))));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "LanguageCodeMustBe2CharactersLong");
    }

    [Fact]
    public async Task UpdateBook_WhenLanguageCodeIsShorterThan2Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(1))));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "LanguageCodeMustBe2CharactersLong");
    }

    [Fact]
    public async Task UpdateBook_WhenLanguageNameIsEmpty_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageName: string.Empty)));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "LanguageNameCannotBeEmpty");
    }

    [Fact]
    public async Task UpdateBook_WhenLanguageNameExceeds50Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(51))));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "LanguageNameMustBeMaximum50CharactersLong");
    }

    [Fact]
    public async Task UpdateBook_WhenLanguageNativeNameIsNull_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(includeNativeName: false)));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenLanguageNativeNameExceeds50Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(language: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(51))));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "LanguageNativeNameMustBeMaximum50CharactersLong");
    }

    [Fact]
    public async Task UpdateBook_WhenOriginalLanguageIsNull_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includeOriginalLanguage: false));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenOriginalLanguageCodeIsEmpty_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageCode: string.Empty)));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "LanguageCodeCannotBeEmpty");
    }

    [Fact]
    public async Task UpdateBook_WhenOriginalLanguageCodeIsShorterThan2Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(1))));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "LanguageCodeMustBe2CharactersLong");
    }

    [Fact]
    public async Task UpdateBook_WhenOriginalLanguageCodeExceeds2Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageCode: new Faker().Random.String2(3))));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "LanguageCodeMustBe2CharactersLong");
    }

    [Fact]
    public async Task UpdateBook_WhenOriginalLanguageNameIsEmpty_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageName: string.Empty)));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "LanguageNameCannotBeEmpty");
    }

    [Fact]
    public async Task UpdateBook_WhenOriginalLanguageNameExceeds50Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(languageName: new Faker().Random.String2(51))));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "LanguageNameMustBeMaximum50CharactersLong");
    }

    [Fact]
    public async Task UpdateBook_WhenOriginalLanguageNativeNameIsNull_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(includeNativeName: false)));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenOriginalLanguageNativeNameExceeds50Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(originalLanguage: _languageInfoDtoFixture.Create(nativeName: new Faker().Random.String2(51))));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "LanguageNativeNameMustBeMaximum50CharactersLong");
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithEmptyPublisher_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includePublisher: false));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithValidPublisher_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(publisher: new Faker().Random.String2(100)));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithInvalidLengthPublisher_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(publisher: new Faker().Random.String2(101)));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "PublisherMustBeMaximum100CharactersLong");
    }

    [Fact]
    public async Task UpdateBook_WhenPageCountIsNull_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(includePageCount: false));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenPageCountIsZero_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(pageCount: 0));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "PageCountMustBeGreaterThanZero");
    }

    [Fact]
    public async Task UpdateBook_WhenPageCountIsNegative_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(pageCount: -1));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "PageCountMustBeGreaterThanZero");
    }

    [Fact]
    public async Task UpdateBook_WhenPageCountIsPositive_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(metadata: _writtenContentMetadataDtoFixture.Create(pageCount: 100));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenFormatIsNull_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create() with { Format = null };

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenFormatIsValid_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(format: BookFormat.Hardcover);

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenFormatIsInvalid_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(format: (BookFormat)99);

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "UnknownBookFormat");
    }

    [Fact]
    public async Task UpdateBook_WhenEditionIsNull_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create() with { Edition = null };

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenEditionIsValid_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(edition: new Faker().Random.String2(50));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenEditionExceeds50Characters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(edition: new Faker().Random.String2(51));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "EditionMustBeMaximum50CharactersLong");
    }

    [Fact]
    public async Task UpdateBook_WhenVolumeNumberIsNull_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create() with { VolumeNumber = null };

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenVolumeNumberIsZero_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(volumeNumber: 0);

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "VolumeNumberMustBeGreaterThanZero");
    }

    [Fact]
    public async Task UpdateBook_WhenVolumeNumberIsNegative_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(volumeNumber: -1);

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "VolumeNumberMustBeGreaterThanZero");
    }

    [Fact]
    public async Task UpdateBook_WhenVolumeNumberIsPositive_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(volumeNumber: 1);

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenSeriesIsNull_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create();

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenAsinIsValid_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(asin: new Faker().Random.String2(10));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenAsinIsNull_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create() with { ASIN = null };

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenAsinIsNotTenCharacters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(asin: new Faker().Random.String2(9));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "AsinMustBe10CharactersLong");
    }

    [Fact]
    public async Task UpdateBook_WhenGoodreadsIdIsNull_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create() with { GoodreadsId = null };

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenGoodreadsIdIsValid_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(goodreadsId: "123456789");

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenGoodreadsIdIsNonNumeric_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(goodreadsId: "abc123");

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "GoodreadsIdMustBeNumeric");
    }

    [Fact]
    public async Task UpdateBook_WhenGoodreadsIdContainsSpaces_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(goodreadsId: "123 456");

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "GoodreadsIdMustBeNumeric");
    }

    [Fact]
    public async Task UpdateBook_WhenGoodreadsIdContainsSpecialCharacters_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(goodreadsId: "123-456");

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "GoodreadsIdMustBeNumeric");
    }

    [Fact]
    public async Task UpdateBook_WhenLccnIsNull_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create() with { LCCN = null };

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenLccnIsValid_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(lccn: "n78890351");

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenLccnHasInvalidFormat_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(lccn: "invalid123");

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "InvalidLccnFormat");
    }

    [Fact]
    public async Task UpdateBook_WhenLccnIsTooLong_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(lccn: new Faker().Random.String2(15));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "InvalidLccnFormat");
    }

    [Fact]
    public async Task UpdateBook_WhenLccnIsTooShort_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(lccn: "n12");

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "InvalidLccnFormat");
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithEmptyOclcNumber_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create() with { OCLCNumber = null };

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithValidOclcNumberFormat1_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(oclcNumber: "ocm12345678");

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithValidOclcNumberFormat2_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(oclcNumber: "ocn123456789");

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithValidOclcNumberFormat3_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(oclcNumber: "on1234567890");

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithValidOclcNumberFormat4_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(oclcNumber: "(OCoLC)1234567890");

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithValidOclcNumberFormat5_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(oclcNumber: "12345678");

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithInvalidOclcNumber_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(oclcNumber: "invalid_oclc_number");

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "InvalidOclcFormat");
    }

    [Fact]
    public async Task UpdateBook_WhenOpenLibraryIdIsNull_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create() with { OpenLibraryId = null };

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithEmptyLibraryThingId_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create() with { LibraryThingId = null };

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithValidLibraryThingId_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(libraryThingId: new Faker().Random.String2(50));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithInvalidLengthLibraryThingId_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(libraryThingId: new Faker().Random.String2(51));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "LibraryThingIdMustBeMaximum50CharactersLong");
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithEmptyGoogleBooksId_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create() with { GoogleBooksId = null };

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithInvalidLengthGoogleBooksId_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(googleBooksId: new Faker().Random.String2(11));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "GoogleBooksIdMustBe12CharactersLong");
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithInvalidFormatGoogleBooksId_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(googleBooksId: new Faker().Random.String2(11) + " ");

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "InvalidGoogleBooksIdFormat");
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithValidGoogleBooksId_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(googleBooksId: new Faker().Random.String2(12, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-"));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithEmptyBarnesAndNobleId_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create() with { BarnesAndNobleId = null };

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithInvalidLengthBarnesAndNobleId_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(barnesAndNobleId: new Faker().Random.String2(11));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "BarnesAndNoblesIdMustBe10CharactersLong");
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithNonNumericBarnesAndNobleId_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(barnesAndNobleId: new Faker().Random.AlphaNumeric(10));

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "InvalidBarnesAndNoblesIdFormat");
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithValidBarnesAndNobleId_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(barnesAndNobleId: new Faker().Random.Number(1000000000, 999999999).ToString());

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithEmptyAppleBooksId_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create() with { AppleBooksId = null };

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithValidAppleBooksId_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(appleBooksId: "id123456");

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithInvalidAppleBooksId_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(appleBooksId: "invalid_id");

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "InvalidAppleBooksIdFormat");
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithNullIsbns_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(includeOptionalProperties: false);

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "IsbnListCannotBeNull");
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithEmptyIsbnValue_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(isbns: [_isbnDtoFixture.Create(value: string.Empty)]);

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "IsbnValueCannotBeEmpty");
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithInvalidIsbn10Value_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(isbns: [_isbnDtoFixture.Create(value: new Faker().Random.String2(5), format: IsbnFormat.Isbn10)]);

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "InvalidIsbn10Format");
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithInvalidIsbn13Value_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(isbns: [_isbnDtoFixture.Create(value: new Faker().Random.String2(5), format: IsbnFormat.Isbn13)]);

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "InvalidIsbn13Format");
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithInvalidIsbnFormat_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(isbns: [_isbnDtoFixture.Create(format: (IsbnFormat)99)]);

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "UnknownIsbnFormat");
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithValidIsbn10_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(isbns: [_isbnDtoFixture.Create(value: "0-306-40615-2", format: IsbnFormat.Isbn10)]);

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithValidIsbn13_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(isbns: [_isbnDtoFixture.Create(value: "978-3-16-148410-0", format: IsbnFormat.Isbn13)]);

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenContributorsIsNull_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create() with { Contributors = null };

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "ContributorsListCannotBeNull");
    }

    [Fact]
    public async Task UpdateBook_WhenContributorsAreValid_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(role: MediaContributorRole.Author)]);

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenContributorIdIsEmpty_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(contributorId: Guid.Empty, role: MediaContributorRole.Author)]);

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "MediaContributorIdCannotBeEmpty");
    }

    [Fact]
    public async Task UpdateBook_WhenContributorRoleIsInvalid_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(contributors: [_mediaContributorReferenceDtoFixture.Create(contributorId: Guid.NewGuid(), role: (MediaContributorRole)999)]);

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "UnknownMediaContributorRole");
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithNullRatings_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(includeOptionalProperties: false);

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "RatingsListCannotBeNull");
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithNegativeRatingValue_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(ratings: [_bookRatingDtoFixture.Create(value: -1)]);

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "RatingValueMustBePositive");
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithRatingValueGreaterThanMaxValue_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(ratings: [_bookRatingDtoFixture.Create(value: 6, maxValue: 5)]);

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "RatingValueCannotBeGreaterThanMaxValue");
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithNegativeMaxRatingValue_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(ratings: [_bookRatingDtoFixture.Create(maxValue: -1)]);

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "RatingMaxValueMustBePositive");
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithNegativeVoteCount_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(ratings: [_bookRatingDtoFixture.Create(voteCount: -1)]);

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, "RatingVoteCountMustBePositive");
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithValidRatings_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(ratings: [_bookRatingDtoFixture.Create(value: 4, maxValue: 5, voteCount: 100)]);

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WhenCalledWithNullVoteCount_ShouldUpdateBook()
    {
        // Arrange
        UpdateBookRequest bookRequest = _requestBookFixture.Create(ratings: [_bookRatingDtoFixture.Create(source: BookRatingSource.Goodreads, includeOptionalProperties: false)]);

        // Act
        HttpResponseMessage response = await PutBookAsync(bookRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Updates the book described by the provided request, seeding the media library, the book and the referenced media contributors.
    /// </summary>
    /// <param name="request">The request to send.</param>
    /// <returns>The HTTP response of the PUT request.</returns>
    private async Task<HttpResponseMessage> PutBookAsync(UpdateBookRequest request)
    {
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid bookId) = await SeedLibraryAndBookAsync(userId, "Original Title");
        await SeedContributorsAsync(request.Contributors);
        return await _client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/books/{bookId}", request);
    }

    /// <summary>
    /// Asserts that the response is an unprocessable entity problem details carrying the expected validation error codes.
    /// </summary>
    /// <param name="response">The HTTP response to assert.</param>
    /// <param name="expectedErrorCodes">The validation error codes expected in the response.</param>
    private async Task AssertUnprocessableEntityWithValidationErrors(HttpResponseMessage response, params string[] expectedErrorCodes)
    {
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(422, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        Assert.NotNull(errors);
        Assert.Contains("General.Validation", errors.Keys);
        Assert.All(expectedErrorCodes, code => Assert.Contains(code, errors["General.Validation"]));
    }

    /// <summary>
    /// Seeds a library owned by <paramref name="userId"/> and a book belonging to it.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <param name="title">The title of the seeded book.</param>
    /// <returns>The Ids of the seeded library and book.</returns>
    private async Task<(Guid libraryId, Guid bookId)> SeedLibraryAndBookAsync(Guid userId, string title)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        LibraryEntity library = _libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Test Library", libraryType: LibraryType.EBook, contentLocations: []);
        BookEntity book = _bookEntityFixture.Create(id: bookId, libraryId: libraryId, path: $"/books/{bookId:N}.epub", title: title, includeMetadata: false);
        dbContext.Libraries.Add(library);
        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync();
        return (libraryId, bookId);
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

    /// <summary>
    /// Seeds a user distinct from the authenticated test user and returns its Id.
    /// </summary>
    /// <returns>The Id of the seeded user and its username.</returns>
    private async Task<(Guid userId, string username)> SeedOtherUserAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid userId = Guid.NewGuid();
        string username = $"otheruser_{Guid.NewGuid()}";
        dbContext.Users.Add(_userEntityFixture.Create(id: userId, username: username, password: "TestPass123!"));
        await dbContext.SaveChangesAsync();
        return (userId, username);
    }

    /// <summary>
    /// Gets the Id of the currently authenticated test user.
    /// </summary>
    /// <returns>The Id of the authenticated test user.</returns>
    private Guid GetCurrentUserId()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        UserEntity user = dbContext.Users.First(user => user.Username == _apiFactory.TestUsername);
        return user.Id;
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _apiFactory.RemoveTestUserAsync();
    }
}