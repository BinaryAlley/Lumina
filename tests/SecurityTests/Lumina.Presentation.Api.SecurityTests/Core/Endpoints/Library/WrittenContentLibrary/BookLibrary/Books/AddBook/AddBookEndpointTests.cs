#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.AddBook;

/// <summary>
/// Contains security tests for the <see cref="AddBookEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddBookEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private readonly LuminaApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly AddBookRequestFixture _addBookRequestFixture = new();
    private readonly WrittenContentMetadataDtoFixture _writtenContentMetadataDtoFixture = new();
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="AddBookEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public AddBookEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task AddBook_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        AddBookRequest request = _addBookRequestFixture.Create(contributors: []);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"/api/v1/libraries/{libraryId}/books", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
        Assert.Equal($"/api/v1/libraries/{libraryId}/books", problemDetails["instance"].GetProperty("value").GetString());
    }

    [Fact]
    public async Task AddBook_WhenUserDoesNotOwnTheLibrary_ShouldReturnForbidden()
    {
        // Arrange
        HttpClient ownerClient = _apiFactory.CreateClient();
        (Guid ownerId, string ownerUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(ownerClient);
        Guid libraryId = Guid.NewGuid();
        await _apiFactory.SeedLibraryAsync(libraryId, ownerId);

        (Guid _, string requesterUsername) = await _apiFactory.CreateAndAuthenticateUserAsync(_client);
        AddBookRequest request = _addBookRequestFixture.Create(contributors: []);

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"/api/v1/libraries/{libraryId}/books", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());

        await _apiFactory.RemoveTestUserAsync(ownerUsername);
        await _apiFactory.RemoveTestUserAsync(requesterUsername);
    }

    [Theory]
    [InlineData("'; DROP TABLE Books; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task AddBook_WithSQLInjectionInPath_ShouldNotCorruptOrDeleteData(string maliciousPath)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (Guid userId, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        Guid libraryId = Guid.NewGuid();
        string contentLocation = Path.GetTempPath();
        string injectedPath = Path.Combine(contentLocation, $"{maliciousPath}.epub");
        using (IServiceScope seedScope = _apiFactory.Services.CreateScope())
        {
            LuminaDbContext seedDbContext = seedScope.ServiceProvider.GetRequiredService<LuminaDbContext>();
            seedDbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Test Library", libraryType: LibraryType.EBook, contentLocations: [contentLocation]));
            await seedDbContext.SaveChangesAsync();
        }
        AddBookRequest request = _addBookRequestFixture.Create(path: injectedPath, contributors: []);

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync($"/api/v1/libraries/{libraryId}/books", request);

        // Assert
        // the malicious path passes the authenticated handler, reaches the parameterized insert, and is persisted
        // verbatim: if it were concatenated into raw SQL, the insert would fail and the book would not be created
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.DoesNotContain("SqliteException", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.Equal(1, await dbContext.Libraries.CountAsync(library => library.Id == libraryId));
        // the single book in the library is the one inserted by this request, with the malicious path stored as data
        Assert.Equal(1, await dbContext.Books.CountAsync(book => book.LibraryId == libraryId));

        await _apiFactory.RemoveTestUserAsync(username);
    }

    [Theory]
    [InlineData("'; DROP TABLE Books; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task AddBook_WithSQLInjectionInBody_ShouldNotCorruptOrDeleteData(string maliciousTitle)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (Guid userId, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        Guid libraryId = Guid.NewGuid();
        string contentLocation = Path.GetTempPath();
        using (IServiceScope seedScope = _apiFactory.Services.CreateScope())
        {
            LuminaDbContext seedDbContext = seedScope.ServiceProvider.GetRequiredService<LuminaDbContext>();
            seedDbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Test Library", libraryType: LibraryType.EBook, contentLocations: [contentLocation]));
            await seedDbContext.SaveChangesAsync();
        }
        string bookPath = Path.Combine(contentLocation, $"{Guid.NewGuid()}.epub");
        AddBookRequest request = _addBookRequestFixture.Create(path: bookPath, metadata: _writtenContentMetadataDtoFixture.Create(title: maliciousTitle), contributors: []);

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync($"/api/v1/libraries/{libraryId}/books", request);

        // Assert
        // the malicious title passes the authenticated handler, reaches the parameterized insert, and is persisted
        // verbatim: if it were concatenated into raw SQL, the insert would fail and the book would not be created
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.DoesNotContain("SqliteException", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        BookEntity storedBook = await dbContext.Books.SingleAsync(book => book.LibraryId == libraryId);
        Assert.Equal(maliciousTitle, storedBook.Title);

        await _apiFactory.RemoveTestUserAsync(username);
    }

    [Theory]
    [InlineData("'; DROP TABLE Books; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task AddBook_WithSQLInjectionInRouteId_ShouldNotCorruptOrLeakData(string maliciousLibraryId)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (Guid _, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        AddBookRequest request = _addBookRequestFixture.Create(contributors: []);

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync($"/api/v1/libraries/{Uri.EscapeDataString(maliciousLibraryId)}/books", request);

        // Assert
        // The route is authoritative and its raw value is kept as a string, so an unparseable route Id reaches the command
        // validator, which reports it as a clean validation error. The response must not leak stack traces, database details,
        // or the injected payload.
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(maliciousLibraryId, content, StringComparison.Ordinal);
        using JsonDocument problemDetails = JsonDocument.Parse(content);
        Assert.Equal("General.Validation", problemDetails.RootElement.GetProperty("title").GetString());
        Assert.Contains("LibraryIdCannotBeEmpty", content, StringComparison.OrdinalIgnoreCase);

        await _apiFactory.RemoveTestUserAsync(username);
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public void Dispose()
    {
        // nothing to clean up: each test removes its own seeded rows
    }
}
