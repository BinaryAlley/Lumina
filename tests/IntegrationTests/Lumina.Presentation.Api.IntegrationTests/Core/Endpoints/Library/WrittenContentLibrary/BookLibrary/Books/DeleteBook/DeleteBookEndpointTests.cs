#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Artwork;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.DeleteBook;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.DeleteBook;

/// <summary>
/// Contains integration tests for the <see cref="DeleteBookEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteBookEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly BookArtworkEntityFixture _bookArtworkEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteBookEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public DeleteBookEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task DeleteBook_WhenBookBelongsToAnOwnedLibrary_ShouldDeleteTheBook()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid bookId) = await SeedLibraryAndBookAsync(userId);

        // Act
        HttpResponseMessage response = await _client.DeleteAsync($"/api/v1/libraries/{libraryId}/books/{bookId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.Null(await dbContext.Books.FirstOrDefaultAsync(book => book.Id == bookId));
    }

    [Fact]
    public async Task DeleteBook_WhenBookHasStoredArtwork_ShouldRemoveTheArtworkAndTheStoredCoverFile()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (Guid libraryId, Guid bookId, string storedCoverPath) = await SeedLibraryBookAndCoverAsync(userId);
        string fullCoverPath = Path.Combine(AppContext.BaseDirectory, storedCoverPath.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        Assert.True(File.Exists(fullCoverPath));

        // Act
        HttpResponseMessage response = await _client.DeleteAsync($"/api/v1/libraries/{libraryId}/books/{bookId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.Null(await dbContext.Books.FirstOrDefaultAsync(book => book.Id == bookId));
        Assert.Null(await dbContext.Books.SelectMany(book => book.Artwork).FirstOrDefaultAsync(artwork => artwork.BookId == bookId));
        Assert.False(File.Exists(fullCoverPath));
    }

    [Fact]
    public async Task DeleteBook_WhenBookDoesNotExist_ShouldReturnBookNotFoundProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = await SeedLibraryAsync(userId);
        string url = $"/api/v1/libraries/{libraryId}/books/{Guid.NewGuid()}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        await AssertNotFoundAsync(response, "BookNotFound", url);
    }

    [Fact]
    public async Task DeleteBook_WhenBookRequestedThroughDifferentLibrary_ShouldReturnBookNotFoundProblem()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        (_, Guid bookId) = await SeedLibraryAndBookAsync(userId);
        string url = $"/api/v1/libraries/{Guid.NewGuid()}/books/{bookId}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        // The route library id is enforced, so a book can never be deleted through another library's route, and the mismatch is
        // reported as not found, without disclosing that the book exists in another library.
        await AssertNotFoundAsync(response, "BookNotFound", url);
    }

    [Fact]
    public async Task DeleteBook_WhenBookBelongsToAnotherUserLibrary_ShouldReturnForbiddenProblem()
    {
        // Arrange
        Guid otherUserId = await SeedOtherUserAsync();
        (Guid otherLibraryId, Guid otherBookId) = await SeedLibraryAndBookAsync(otherUserId);
        string url = $"/api/v1/libraries/{otherLibraryId}/books/{otherBookId}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        await AssertProblemDetails(response, HttpStatusCode.Forbidden, "General.Unauthorized", "NotAuthorized", url, "https://tools.ietf.org/html/rfc9110#section-15.5.4");
    }

    [Fact]
    public async Task DeleteBook_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();
        string url = $"/api/v1/libraries/{Guid.NewGuid()}/books/{Guid.NewGuid()}";

        // Act
        HttpResponseMessage response = await unauthenticatedClient.DeleteAsync(url);

        // Assert
        await AssertUnauthorizedAsync(response, url);
    }

    [Fact]
    public async Task DeleteBook_WhenLibraryIdIsNotParseable_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        string url = $"/api/v1/libraries/not-a-guid/books/{Guid.NewGuid()}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, url, "LibraryIdCannotBeEmpty");
    }

    [Fact]
    public async Task DeleteBook_WhenBookIdIsNotParseable_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        string url = $"/api/v1/libraries/{Guid.NewGuid()}/books/not-a-guid";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        await AssertUnprocessableEntityWithValidationErrors(response, url, "BookIdCannotBeEmpty");
    }

    /// <summary>
    /// Seeds an ebook library owned by <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <returns>The Id of the seeded library.</returns>
    private async Task<Guid> SeedLibraryAsync(Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Test Library", libraryType: LibraryType.EBook, contentLocations: []));
        await dbContext.SaveChangesAsync();
        return libraryId;
    }

    /// <summary>
    /// Seeds an ebook library owned by <paramref name="userId"/> and a book belonging to it.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <returns>The Ids of the seeded library and book.</returns>
    private async Task<(Guid libraryId, Guid bookId)> SeedLibraryAndBookAsync(Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        LibraryEntity library = _libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Test Library", libraryType: LibraryType.EBook, contentLocations: []);
        BookEntity book = _bookEntityFixture.Create(id: bookId, libraryId: libraryId, path: $"/books/{bookId:N}.epub", title: "Test Book", includeMetadata: false);
        dbContext.Libraries.Add(library);
        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync();
        return (libraryId, bookId);
    }

    /// <summary>
    /// Seeds an ebook library owned by <paramref name="userId"/>, a book belonging to it, and a stored cover file of that book together with its artwork row.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <returns>The Ids of the seeded library and book, and the relative path of the stored cover file.</returns>
    private async Task<(Guid libraryId, Guid bookId, string storedCoverPath)> SeedLibraryBookAndCoverAsync(Guid userId)
    {
        (Guid libraryId, Guid bookId) = await SeedLibraryAndBookAsync(userId);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        LibraryEntity library = await dbContext.Libraries.SingleAsync(library => library.Id == libraryId);
        BookEntity book = await dbContext.Books.Include(book => book.Artwork).SingleAsync(book => book.Id == bookId);
        IBookArtworkService bookArtworkService = scope.ServiceProvider.GetRequiredService<IBookArtworkService>();

        // Store a minimal PNG-like payload through the same artwork service that the API uses, so that the stored path matches the one the deletion resolves.
        byte[] payload = new byte[100];
        payload[0] = 137;
        payload[1] = 80;
        payload[2] = 78;
        payload[3] = 71;
        payload[4] = 13;
        payload[5] = 10;
        payload[6] = 26;
        payload[7] = 10;
        Result<string> saveArtworkResult;
        using (MemoryStream artworkStream = new(payload))
            saveArtworkResult = await bookArtworkService.SaveBookArtworkAsync(libraryId, bookId, library.Title, string.Empty, book.Title, artworkStream, "cover.png", CancellationToken.None);
        Assert.False(saveArtworkResult.IsFailure);

        book.Artwork.Add(_bookArtworkEntityFixture.Create(bookId: bookId, fileName: saveArtworkResult.Value));
        await dbContext.SaveChangesAsync();
        return (libraryId, bookId, saveArtworkResult.Value);
    }

    /// <summary>
    /// Seeds a user distinct from the authenticated test user and returns its Id.
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
    /// Asserts that the response is a not found problem details carrying the expected detail and request instance path.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedDetail">The expected problem detail.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    private async Task AssertNotFoundAsync(HttpResponseMessage response, string expectedDetail, string expectedInstance)
    {
        await AssertProblemDetails(response, HttpStatusCode.NotFound, "General.NotFound", expectedDetail, expectedInstance, "https://tools.ietf.org/html/rfc9110#section-15.5.5");
    }

    /// <summary>
    /// Asserts the shape of a problem details response with the provided status, title, detail, instance and type.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="statusCode">The expected HTTP status code.</param>
    /// <param name="expectedTitle">The expected problem title.</param>
    /// <param name="expectedDetail">The expected problem detail.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    /// <param name="expectedType">The expected problem type URI.</param>
    private async Task AssertProblemDetails(HttpResponseMessage response, HttpStatusCode statusCode, string expectedTitle, string expectedDetail, string expectedInstance, string expectedType)
    {
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal((int)statusCode, problemDetails!["status"].GetInt32());
        Assert.Equal(expectedType, problemDetails["type"].GetString());
        Assert.Equal(expectedTitle, problemDetails["title"].GetString());
        Assert.Equal(expectedDetail, problemDetails["detail"].GetString());
        Assert.Equal(expectedInstance, problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);
    }

    /// <summary>
    /// Asserts that the response is an unauthorized problem details carrying the expected request instance path.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    private async Task AssertUnauthorizedAsync(HttpResponseMessage response, string expectedInstance)
    {
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal((int)HttpStatusCode.Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
        Assert.Equal(expectedInstance, problemDetails["instance"].GetProperty("value").GetString());
    }

    /// <summary>
    /// Asserts that the response is an unprocessable entity problem details carrying the expected validation error codes.
    /// </summary>
    /// <param name="response">The HTTP response to assert on.</param>
    /// <param name="expectedInstance">The expected request instance path.</param>
    /// <param name="expectedErrorCodes">The validation error codes that must all be present.</param>
    private async Task AssertUnprocessableEntityWithValidationErrors(HttpResponseMessage response, string expectedInstance, params string[] expectedErrorCodes)
    {
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal((int)HttpStatusCode.UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", problemDetails["type"].GetString());
        Assert.Equal(expectedInstance, problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        Assert.NotNull(errors);
        Assert.Contains("General.Validation", errors.Keys);
        Assert.All(expectedErrorCodes, code => Assert.Contains(code, errors["General.Validation"]));
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _apiFactory.RemoveTestUserAsync();
    }
}
