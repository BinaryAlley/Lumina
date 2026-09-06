#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.DataAccess.Core.UoW;
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
    private readonly WrittenContentMetadataDtoFixture _writtenContentMetadataDtoFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
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
        UpdateBookRequest request = _requestBookFixture.Create(id: bookId, metadata: _writtenContentMetadataDtoFixture.Create(title: "Updated Title"), isbns: [], contributors: [], ratings: [], includeOptionalProperties: false);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/books/{bookId}", request);

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
    public async Task UpdateBook_WhenTwoContributorsShareTheSameDisplayName_ShouldCreateOneContributorWithTwoBookLinks()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (_, Guid bookId) = await SeedLibraryAndBookAsync(userId, "Original Title");
        List<MediaContributorDto> duplicatedContributors =
        [
            new MediaContributorDto(
                new MediaContributorNameDto("Duplicated Author", "Duplicated Author Legal"),
                new MediaContributorRoleDto("author", MediaContributorRoleCategory.Author)),
            new MediaContributorDto(
                new MediaContributorNameDto("Duplicated Author", "Duplicated Author Legal"),
                new MediaContributorRoleDto("illustrator", MediaContributorRoleCategory.Illustrator))
        ];
        UpdateBookRequest request = _requestBookFixture.Create(id: bookId, contributors: duplicatedContributors, isbns: [], ratings: [], includeOptionalProperties: false);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/books/{bookId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        MediaContributorEntity contributor = dbContext.MediaContributors.Single(mediaContributor => mediaContributor.DisplayName == "Duplicated Author");
        List<BookContributorEntity> links = dbContext.Books
            .Include(book => book.BookContributors)
            .Single(book => book.Id == bookId)
            .BookContributors
            .ToList();
        Assert.Equal(2, links.Count);
        Assert.All(links, link => Assert.Equal(contributor.Id, link.MediaContributorId));
        Assert.Contains(links, link => link.RoleName == "author");
        Assert.Contains(links, link => link.RoleName == "illustrator");
    }

    [Fact]
    public async Task UpdateBook_WhenRouteBookDoesNotExistButBodyIdExists_ShouldNotUpdateTheBodyBook()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid bodyBookId) = await SeedLibraryAndBookAsync(userId, "Body Book Title");
        Guid routeId = Guid.NewGuid();
        UpdateBookRequest request = _requestBookFixture.Create(id: bodyBookId, metadata: _writtenContentMetadataDtoFixture.Create(title: "Should Not Apply"));

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/books/{routeId}", request);

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
        UpdateBookRequest request = _requestBookFixture.Create();

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/books/not-a-guid", request);

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
        UpdateBookRequest request = _requestBookFixture.Create();

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/books/{Guid.NewGuid()}", request);

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
        (_, Guid bookId) = await SeedLibraryAndBookAsync(otherUserId, "Other User Book");
        UpdateBookRequest request = _requestBookFixture.Create(id: bookId);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/books/{bookId}", request);

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
        (_, Guid bookId) = await SeedLibraryAndBookAsync(userId, "Original Title");
        UpdateBookRequest request = _requestBookFixture.Create(id: bookId, metadata: _writtenContentMetadataDtoFixture.Create(includeTitle: false));

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/books/{bookId}", request);

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
        UpdateBookRequest request = _requestBookFixture.Create();

        // Act
        HttpResponseMessage response = await unauthenticatedClient.PutAsJsonAsync($"/api/v1/books/{Guid.NewGuid()}", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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
