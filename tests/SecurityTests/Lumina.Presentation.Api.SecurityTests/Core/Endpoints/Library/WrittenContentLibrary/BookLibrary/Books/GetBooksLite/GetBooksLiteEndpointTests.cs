#region ========================================================================= USING =====================================================================================
using Lumina.DataAccess.Core.UoW;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBooksLite;

/// <summary>
/// Contains security tests for the <c>/books/lite</c> route.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBooksLiteEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private readonly HttpClient _client;
    private readonly LuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBooksLiteEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetBooksLiteEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task GetBooksLite_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/books/lite");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("/api/v1/books/lite", problemDetails["instance"].GetProperty("value").GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
    }

    [Theory]
    [InlineData("'; DROP TABLE Books; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task GetBooksLite_WhenSearchTermContainsSqlInjection_ShouldNotCorruptOrDeleteData(string maliciousSearchTerm)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        (Guid userId, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(client);
        Guid libraryId = Guid.NewGuid();
        await _apiFactory.SeedLibraryAsync(libraryId, userId);
        await _apiFactory.SeedBookAsync(libraryId, "Book A");

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/v1/books/lite?libraryId={libraryId}&searchTerm={Uri.EscapeDataString(maliciousSearchTerm)}");

        // Assert
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);

        // the injected statement must never be executed: the Books table and the seeded rows must still be there
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.Equal(1, await dbContext.Books.CountAsync(book => book.LibraryId == libraryId));

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
