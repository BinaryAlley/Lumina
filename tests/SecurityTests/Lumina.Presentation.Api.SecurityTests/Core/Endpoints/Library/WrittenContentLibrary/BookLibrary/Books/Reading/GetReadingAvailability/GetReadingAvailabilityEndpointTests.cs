#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingAvailability;

/// <summary>
/// Contains security tests for the <c>/books/{bookId}/reading/availability</c> route.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingAvailabilityEndpointTests : IClassFixture<LuminaApiFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingAvailabilityEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetReadingAvailabilityEndpointTests(LuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task GetReadingAvailability_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/books/{Guid.NewGuid()}/reading/availability");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
        Assert.DoesNotContain("Exception", content, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("'; DROP TABLE Books--")] // destructive injection
    public async Task GetReadingAvailability_WithSQLInjectionInBookId_ShouldNotLeakDatabaseDetails(string maliciousBookId)
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/books/{Uri.EscapeDataString(maliciousBookId)}/reading/availability");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        // The {bookId} route parameter is Guid-typed, so the malicious value fails model binding before it reaches the handler.
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
    }
}
