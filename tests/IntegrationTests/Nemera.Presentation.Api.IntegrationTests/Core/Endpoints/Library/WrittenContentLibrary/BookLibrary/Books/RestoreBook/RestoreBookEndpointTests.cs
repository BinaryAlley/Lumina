#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Nemera.Application.Common.DataAccess.Entities.UsersManagement;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Nemera.DataAccess.Core.UoW;
using Nemera.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Nemera.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.RestoreBook;
using Nemera.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.IntegrationTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.RestoreBook;

/// <summary>
/// Contains integration tests for the <see cref="RestoreBookEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreBookEndpointTests : IClassFixture<AuthenticatedNemeraApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedNemeraApiFactory _apiFactory;
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreBookEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public RestoreBookEndpointTests(AuthenticatedNemeraApiFactory apiFactory)
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
    public async Task RestoreBook_WhenDeletedBookBelongsToAnOwnedLibrary_ShouldRestoreTheBook()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid bookId) = await SeedLibraryAndBookAsync(userId, isDeleted: true);

        // Act
        HttpResponseMessage response = await _client.PostAsync($"/api/v1/libraries/{libraryId}/books/{bookId}/restore", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();
        BookEntity? book = await dbContext.Books.FirstOrDefaultAsync(book => book.Id == bookId);
        Assert.NotNull(book);
        Assert.False(book!.IsDeleted);
    }

    [Fact]
    public async Task RestoreBook_WhenBookIsNotDeleted_ShouldReturnBookNotFoundProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid bookId) = await SeedLibraryAndBookAsync(userId, isDeleted: false);

        // Act
        HttpResponseMessage response = await _client.PostAsync($"/api/v1/libraries/{libraryId}/books/{bookId}/restore", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RestoreBook_WhenBookIsRestoredThroughDifferentLibrary_ShouldReturnBookNotFoundProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (_, Guid bookId) = await SeedLibraryAndBookAsync(userId, isDeleted: true);

        // Act
        HttpResponseMessage response = await _client.PostAsync($"/api/v1/libraries/{Guid.NewGuid()}/books/{bookId}/restore", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RestoreBook_WhenBookBelongsToAnotherUserLibrary_ShouldReturnForbiddenProblem()
    {
        // Arrange
        Guid otherUserId = await SeedOtherUserAsync();
        (Guid otherLibraryId, Guid otherBookId) = await SeedLibraryAndBookAsync(otherUserId, isDeleted: true);

        // Act
        HttpResponseMessage response = await _client.PostAsync($"/api/v1/libraries/{otherLibraryId}/books/{otherBookId}/restore", null);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RestoreBook_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await unauthenticatedClient.PostAsync($"/api/v1/libraries/{Guid.NewGuid()}/books/{Guid.NewGuid()}/restore", null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RestoreBook_WhenIdsAreNotParseable_ShouldReturnUnprocessableEntity()
    {
        // Act
        HttpResponseMessage response = await _client.PostAsync($"/api/v1/libraries/not-a-guid/books/not-a-guid/restore", null);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    /// <summary>
    /// Seeds an ebook library owned by <paramref name="userId"/> and a book belonging to it.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <param name="isDeleted">Whether the seeded book is soft deleted.</param>
    /// <returns>The Ids of the seeded library and book.</returns>
    private async Task<(Guid libraryId, Guid bookId)> SeedLibraryAndBookAsync(Guid userId, bool isDeleted)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();
        Guid libraryId = Guid.NewGuid();
        BookEntity book = _bookEntityFixture.Create(libraryId: libraryId, path: $"/books/{Guid.NewGuid():N}.epub", isDeleted: isDeleted, includeMetadata: false);
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Test Library", libraryType: LibraryType.EBook, contentLocations: []));
        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync();
        return (libraryId, book.Id);
    }

    /// <summary>
    /// Seeds a user distinct from the authenticated test user and returns its Id.
    /// </summary>
    /// <returns>The Id of the seeded user.</returns>
    private async Task<Guid> SeedOtherUserAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();
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
        NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();
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
