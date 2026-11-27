#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Nemera.DataAccess.Core.UoW;
using Nemera.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Nemera.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.RestoreBook;
using Nemera.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
#endregion

namespace Nemera.Presentation.Api.SecurityTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.RestoreBook;

/// <summary>
/// Contains security tests for the <see cref="RestoreBookEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreBookEndpointTests : IClassFixture<NemeraApiFactory>, IDisposable
{
    private readonly NemeraApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly List<string> _seededUsernames = [];
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreBookEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public RestoreBookEndpointTests(NemeraApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", NemeraApiFactory.GetUniqueTestIp());
    }

    [Fact]
    public async Task RestoreBook_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        string url = $"/api/v1/libraries/{Guid.NewGuid()}/books/{Guid.NewGuid()}/restore";

        // Act
        HttpResponseMessage response = await _client.PostAsync(url, null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
    }

    [Theory]
    [InlineData("'; DROP TABLE Books; --")] // Destructive injection.
    [InlineData("' OR '1'='1")] // Boolean-based injection.
    [InlineData("'; EXEC xp_cmdshell('dir'); --")] // Command execution attempt.
    public async Task RestoreBook_WithSQLInjectionInRouteIds_ShouldNotCorruptOrLeakData(string maliciousId)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);

        // Act
        HttpResponseMessage response = await client.PostAsync($"/api/v1/libraries/{Uri.EscapeDataString(maliciousId)}/books/{Uri.EscapeDataString(maliciousId)}/restore", null);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.Validation", problemDetails.RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task RestoreBook_WhenBookBelongsToAnotherUser_ShouldReturnForbiddenAndKeepItDeleted()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);
        HttpClient victimClient = _apiFactory.CreateClient();
        (Guid victimUserId, string victimUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(victimClient);
        _seededUsernames.Add(victimUsername);
        (Guid libraryId, Guid bookId) = await SeedLibraryAndDeletedBookAsync(victimUserId);

        // Act
        HttpResponseMessage response = await client.PostAsync($"/api/v1/libraries/{libraryId}/books/{bookId}/restore", null);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();
        BookEntity? book = await dbContext.Books.FirstOrDefaultAsync(book => book.Id == bookId);
        Assert.NotNull(book);
        Assert.True(book!.IsDeleted);
    }

    [Fact]
    public async Task RestoreBook_WhenBookIsRestoredThroughAnotherLibrary_ShouldReturnNotFoundAndKeepItDeleted()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (Guid actingUserId, string actingUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(actingUsername);
        (Guid ownLibraryId, _) = await SeedLibraryAndDeletedBookAsync(actingUserId, isDeleted: false);
        HttpClient victimClient = _apiFactory.CreateClient();
        (Guid victimUserId, string victimUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(victimClient);
        _seededUsernames.Add(victimUsername);
        (_, Guid victimBookId) = await SeedLibraryAndDeletedBookAsync(victimUserId);

        // Act
        HttpResponseMessage response = await client.PostAsync($"/api/v1/libraries/{ownLibraryId}/books/{victimBookId}/restore", null);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();
        BookEntity? victimBook = await dbContext.Books.FirstOrDefaultAsync(book => book.Id == victimBookId);
        Assert.NotNull(victimBook);
        Assert.True(victimBook!.IsDeleted);
    }

    /// <summary>
    /// Seeds an ebook library owned by <paramref name="userId"/>, together with a book in the requested deletion state.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <param name="isDeleted">Whether the seeded book is soft deleted.</param>
    /// <returns>The Ids of the seeded library and book.</returns>
    private async Task<(Guid libraryId, Guid bookId)> SeedLibraryAndDeletedBookAsync(Guid userId, bool isDeleted = true)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();
        Guid libraryId = Guid.NewGuid();
        BookEntity book = _bookEntityFixture.Create(libraryId: libraryId, path: $"/books/{Guid.NewGuid():N}.epub", isDeleted: isDeleted, includeMetadata: false);
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Victim Library", libraryType: LibraryType.EBook, contentLocations: []));
        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync();
        return (libraryId, book.Id);
    }

    /// <summary>
    /// Removes the users created by the tests.
    /// </summary>
    public void Dispose()
    {
        _client.Dispose();
        foreach (string username in _seededUsernames)
            _apiFactory.RemoveTestUser(username);
    }
}
