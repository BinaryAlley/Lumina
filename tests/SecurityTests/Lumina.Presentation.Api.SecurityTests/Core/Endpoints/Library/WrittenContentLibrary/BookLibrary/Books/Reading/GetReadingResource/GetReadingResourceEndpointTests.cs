#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingResource;

/// <summary>
/// Contains security tests for the <c>/books/{bookId}/reading/resources/{resourceKey}</c> route.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingResourceEndpointTests : IClassFixture<LuminaApiFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingResourceEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetReadingResourceEndpointTests(LuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task GetReadingResource_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/books/{Guid.NewGuid()}/reading/resources/cover");

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
    public async Task GetReadingResource_WithSQLInjectionInResourceKey_ShouldNotLeakDatabaseDetails(string maliciousResourceKey)
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/books/{Guid.NewGuid()}/reading/resources/{Uri.EscapeDataString(maliciousResourceKey)}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("../../etc/passwd")] // directory traversal
    [InlineData("..%2F..%2Fetc%2Fpasswd")] // encoded directory traversal
    public async Task GetReadingResource_WithPathTraversalInResourceKey_ShouldNotExposeFilesystemPaths(string traversalPayload)
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/books/{Guid.NewGuid()}/reading/resources/{traversalPayload}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.DoesNotContain("etc/passwd", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Stack Trace", content, StringComparison.Ordinal);
    }
}
