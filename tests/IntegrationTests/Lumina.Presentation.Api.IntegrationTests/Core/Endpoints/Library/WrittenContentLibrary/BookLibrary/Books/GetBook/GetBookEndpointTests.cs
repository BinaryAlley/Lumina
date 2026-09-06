#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBook;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBook;

/// <summary>
/// Contains integration tests for the <see cref="GetBookEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBookEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBookEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetBookEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task GetBook_WhenBookBelongsToAnOwnedLibrary_ShouldReturnBookDetails()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid bookId) = await SeedLibraryAndBookAsync(userId, "Test Book");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/books/{bookId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        BookResponse? book = JsonSerializer.Deserialize<BookResponse>(content, _jsonOptions);
        Assert.NotNull(book);
        Assert.Equal(bookId, book!.Id);
        Assert.Equal(libraryId, book.LibraryId);
        Assert.Equal("Test Book", book.Metadata!.Title);
        Assert.Equal($"/books/{bookId:N}.epub", book.Path);
    }

    [Fact]
    public async Task GetBook_WhenBookDoesNotExist_ShouldReturnBookNotFoundProblem()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/books/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Microsoft.AspNetCore.Mvc.ProblemDetails? problemDetails = JsonSerializer.Deserialize<Microsoft.AspNetCore.Mvc.ProblemDetails>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal("General.NotFound", problemDetails!.Title);
        Assert.Equal("BookNotFound", problemDetails.Detail);
        Assert.DoesNotContain("Exception", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetBook_WhenBookBelongsToAnotherUserLibrary_ShouldReturnForbiddenProblem()
    {
        // Arrange
        (Guid otherUserId, _) = await SeedOtherUserAsync();
        (_, Guid bookId) = await SeedLibraryAndBookAsync(otherUserId, "Other User Book");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/books/{bookId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        using JsonDocument problemDetails = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("General.Unauthorized", problemDetails.RootElement.GetProperty("title").GetString());
        Assert.Equal("NotAuthorized", problemDetails.RootElement.GetProperty("detail").GetString());
        Assert.DoesNotContain("Exception", problemDetails.RootElement.GetRawText(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetBook_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await unauthenticatedClient.GetAsync($"/api/v1/books/{Guid.NewGuid()}");

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
