#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.UoW;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingManifest;

/// <summary>
/// Contains integration tests for the <see cref="GetReadingManifestEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingManifestEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingManifestEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetReadingManifestEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task GetReadingManifest_WhenBookDoesNotExist_ShouldReturnBookNotFoundProblem()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{Guid.NewGuid()}/books/{Guid.NewGuid()}/reading/manifest");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.Contains("BookNotFound", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetReadingManifest_WhenBookBelongsToAnotherUser_ShouldReturnForbidden()
    {
        // Arrange
        Guid otherUserId = await SeedOtherUserAsync();
        (Guid libraryId, Guid bookId) = await SeedBookAsync(otherUserId);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{libraryId}/books/{bookId}/reading/manifest");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        using JsonDocument problemDetails = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("General.Unauthorized", problemDetails.RootElement.GetProperty("title").GetString());
        Assert.Equal("NotAuthorized", problemDetails.RootElement.GetProperty("detail").GetString());
        Assert.DoesNotContain("Exception", problemDetails.RootElement.GetRawText(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetReadingManifest_WhenBookBelongsToAnotherLibrary_ShouldReturnBookNotFoundProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (_, Guid bookId) = await SeedBookAsync(userId);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{Guid.NewGuid()}/books/{bookId}/reading/manifest");

        // Assert
        // The route library id is enforced, so a book can never be read through another library's route, and the mismatch is
        // reported as not found, without disclosing that the book exists in another library.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.Contains("BookNotFound", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetReadingManifest_WhenBookBelongsToAnOwnedLibrary_ShouldNotLeakAnException()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid bookId) = await SeedBookAsync(userId);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/{libraryId}/books/{bookId}/reading/manifest");

        // Assert
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Exception", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Stack Trace", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetReadingManifest_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await unauthenticatedClient.GetAsync($"/api/v1/libraries/{Guid.NewGuid()}/books/{Guid.NewGuid()}/reading/manifest");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetReadingManifest_WhenRouteIdsAreNotParseable_ShouldReturnUnprocessableEntity()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/libraries/not-a-guid/books/not-a-guid/reading/manifest");

        // Assert
        // The route values are kept as raw strings, so the unparseable Ids reach the query validator, which reports clean
        // validation errors instead of failing the request binding.
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.Contains("LibraryIdCannotBeEmpty", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("BookIdCannotBeEmpty", content, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Seeds a library owned by <paramref name="userId"/> and an EPUB book belonging to it.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <returns>The Ids of the seeded library and book.</returns>
    private async Task<(Guid libraryId, Guid bookId)> SeedBookAsync(Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        LibraryEntity library = _libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Test Library", libraryType: Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary.LibraryType.EBook, contentLocations: []);
        BookEntity book = _bookEntityFixture.Create(id: bookId, libraryId: libraryId, path: $"/books/{bookId:N}.epub", title: "Test Book", includeMetadata: false);
        dbContext.Libraries.Add(library);
        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync();
        return (libraryId, bookId);
    }

    /// <summary>
    /// Seeds a user that is not the authenticated one and returns its Id.
    /// </summary>
    /// <returns>The Id of the seeded user.</returns>
    private async Task<Guid> SeedOtherUserAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid userId = Guid.NewGuid();
        dbContext.Users.Add(_userEntityFixture.Create(id: userId, username: $"otheruser_{Guid.NewGuid()}", password: "TestPass123!"));
        await dbContext.SaveChangesAsync();
        return userId;
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
