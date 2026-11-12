#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.DeleteBook;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.DeleteBook;

/// <summary>
/// Contains security tests for the <see cref="DeleteBookEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteBookEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private readonly LuminaApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly List<string> _seededUsernames = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteBookEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public DeleteBookEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // A unique X-Forwarded-For isolates rate limiting state per test.
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
    }

    [Fact]
    public async Task DeleteBook_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        string url = $"/api/v1/libraries/{Guid.NewGuid()}/books/{Guid.NewGuid()}";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
        Assert.Equal(url, problemDetails["instance"].GetProperty("value").GetString());
    }

    [Theory]
    [InlineData("'; DROP TABLE Books; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; EXEC xp_cmdshell('dir'); --")] // command execution attempt
    public async Task DeleteBook_WithSQLInjectionInLibraryId_ShouldNotCorruptOrLeakData(string maliciousLibraryId)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);

        // Act
        HttpResponseMessage response = await client.DeleteAsync($"/api/v1/libraries/{Uri.EscapeDataString(maliciousLibraryId)}/books/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.Validation", problemDetails.RootElement.GetProperty("title").GetString());
    }

    [Theory]
    [InlineData("'; DROP TABLE Books; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    [InlineData("'; EXEC xp_cmdshell('dir'); --")] // command execution attempt
    public async Task DeleteBook_WithSQLInjectionInBookId_ShouldNotCorruptOrLeakData(string maliciousBookId)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);

        // Act
        HttpResponseMessage response = await client.DeleteAsync($"/api/v1/libraries/{Guid.NewGuid()}/books/{Uri.EscapeDataString(maliciousBookId)}");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.Validation", problemDetails.RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task DeleteBook_WhenBookBelongsToAnotherUserLibrary_ShouldReturnForbiddenResult()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);
        Guid otherUserId = await SeedOtherUserAsync();
        (Guid libraryId, Guid bookId) = await SeedLibraryAndBookAsync(otherUserId);

        // Act
        HttpResponseMessage response = await client.DeleteAsync($"/api/v1/libraries/{libraryId}/books/{bookId}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeleteBook_WhenBookBelongsToAnotherUsersLibrary_ShouldReturnNotFoundWithoutDeletingIt()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (Guid actingUserId, string actingUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(actingUsername);
        (Guid ownLibraryId, _) = await SeedLibraryAndBookAsync(actingUserId);

        // The book lives in a library owned by another user, and is referenced through the acting user's own library route.
        HttpClient victimClient = _apiFactory.CreateClient();
        (Guid victimUserId, string victimUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(victimClient);
        _seededUsernames.Add(victimUsername);
        (_, Guid victimBookId) = await SeedLibraryAndBookAsync(victimUserId);

        // Act
        HttpResponseMessage response = await client.DeleteAsync($"/api/v1/libraries/{ownLibraryId}/books/{victimBookId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.NotFound", problemDetails.RootElement.GetProperty("title").GetString());
        Assert.Equal("BookNotFound", problemDetails.RootElement.GetProperty("detail").GetString());

        // The other user's book is untouched.
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.NotNull(await dbContext.Books.FirstOrDefaultAsync(book => book.Id == victimBookId));
    }

    /// <summary>
    /// Seeds a user distinct from the authenticated test user and returns its Id.
    /// </summary>
    /// <returns>The Id of the seeded other user.</returns>
    private async Task<Guid> SeedOtherUserAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid userId = Guid.NewGuid();
        string username = $"otheruser_{Guid.NewGuid()}";
        dbContext.Users.Add(_userEntityFixture.Create(id: userId, username: username, password: "TestPass123!"));
        await dbContext.SaveChangesAsync();
        _seededUsernames.Add(username);
        return userId;
    }

    /// <summary>
    /// Seeds an ebook library owned by <paramref name="userId"/>, together with a book of that library.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <returns>The Ids of the seeded library and book.</returns>
    private async Task<(Guid libraryId, Guid bookId)> SeedLibraryAndBookAsync(Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Test Library", libraryType: LibraryType.EBook, contentLocations: []));
        dbContext.Books.Add(_bookEntityFixture.Create(id: bookId, libraryId: libraryId, path: $"/books/{bookId:N}.epub", title: "Test Book", includeMetadata: false));
        await dbContext.SaveChangesAsync();
        return (libraryId, bookId);
    }

    /// <summary>
    /// Disposes the API factory resources.
    /// </summary>
    public void Dispose()
    {
        _client.Dispose();
        foreach (string username in _seededUsernames)
            _apiFactory.RemoveTestUser(username);
    }
}
