#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Nemera.DataAccess.Core.UoW;
using Nemera.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Nemera.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetDeletedBooks;
using Nemera.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
#endregion

namespace Nemera.Presentation.Api.SecurityTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetDeletedBooks;

/// <summary>
/// Contains security tests for the <see cref="GetDeletedBooksEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDeletedBooksEndpointTests : IClassFixture<NemeraApiFactory>, IDisposable
{
    private readonly NemeraApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly List<string> _seededUsernames = [];
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly BookEntityFixture _bookEntityFixture = new();
    private readonly BookArtworkEntityFixture _bookArtworkEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDeletedBooksEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetDeletedBooksEndpointTests(NemeraApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", NemeraApiFactory.GetUniqueTestIp());
    }

    [Fact]
    public async Task GetDeletedBooks_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        string url = $"/api/v1/libraries/{Guid.NewGuid()}/books/deleted-items";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);

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
    public async Task GetDeletedBooks_WithSQLInjectionInLibraryId_ShouldNotCorruptOrLeakData(string maliciousLibraryId)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/v1/libraries/{Uri.EscapeDataString(maliciousLibraryId)}/books/deleted-items");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.Validation", problemDetails.RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task GetDeletedBooks_WhenLibraryBelongsToAnotherUser_ShouldReturnForbiddenAndNotLeakTheDeletedBook()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        _seededUsernames.Add(username);
        HttpClient victimClient = _apiFactory.CreateClient();
        (Guid victimUserId, string victimUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(victimClient);
        _seededUsernames.Add(victimUsername);
        const string TITLE = "Secret Deleted Book";
        const string PATH = "/private/deleted-book.epub";
        const string COVER_PATH = "/private/deleted-book-cover.jpg";
        (Guid libraryId, _) = await SeedLibraryAndDeletedBookAsync(victimUserId, TITLE, PATH, COVER_PATH);

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/v1/libraries/{libraryId}/books/deleted-items");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.DoesNotContain(TITLE, content, StringComparison.Ordinal);
        Assert.DoesNotContain(PATH, content, StringComparison.Ordinal);
        Assert.DoesNotContain(COVER_PATH, content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Seeds an ebook library owned by <paramref name="userId"/>, together with a deleted book and its cover.
    /// </summary>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <param name="title">The title of the deleted book.</param>
    /// <param name="path">The file system path of the deleted book.</param>
    /// <param name="coverPath">The stored cover path of the deleted book.</param>
    /// <returns>The Id of the seeded library and the seeded book.</returns>
    private async Task<(Guid libraryId, BookEntity book)> SeedLibraryAndDeletedBookAsync(Guid userId, string title, string path, string coverPath)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();
        Guid libraryId = Guid.NewGuid();
        BookEntity book = _bookEntityFixture.Create(libraryId: libraryId, path: path, title: title, isDeleted: true, includeMetadata: false);
        book.Artwork = [_bookArtworkEntityFixture.Create(bookId: book.Id, artworkType: ArtworkType.Cover, ordinal: 0, fileName: coverPath)];
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Victim Library", libraryType: LibraryType.EBook, contentLocations: []));
        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync();
        return (libraryId, book);
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
