#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Api.Core.Endpoints.Plugins.GetLibraryTypeMetadataProviders;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Plugins.GetLibraryTypeMetadataProviders;

/// <summary>
/// Contains security tests for the <see cref="GetLibraryTypeMetadataProvidersEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryTypeMetadataProvidersEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private readonly LuminaApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryTypeMetadataProvidersEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetLibraryTypeMetadataProvidersEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // A unique X-Forwarded-For isolates rate limiting state per test.
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
    }

    [Fact]
    public async Task GetLibraryTypeMetadataProviders_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        string libraryType = "Music";

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/libraries/metadata-providers/{libraryType}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal($"/api/v1/libraries/metadata-providers/{libraryType}", problemDetails["instance"].GetProperty("value").GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
    }

    [Theory]
    [InlineData("' OR '1'='1")] // Basic SQL injection.
    [InlineData("'; DROP TABLE Libraries--")] // Destructive SQL injection.
    [InlineData("Music; rm -rf /")] // Command injection.
    [InlineData("$(whoami)")] // Command substitution injection.
    [InlineData("`whoami`")] // Backtick command injection.
    [InlineData("{0}{1}{0}")] // Template/placeholder injection.
    [InlineData("..\\..\\..\\Windows\\System32\\config\\SAM")] // Windows path traversal.
    [InlineData("%2e%2e%2fetc%2fpasswd")] // Encoded path traversal.
    public async Task GetLibraryTypeMetadataProviders_WithInjectionInLibraryType_ShouldNotLeakOrError(string maliciousLibraryType)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        await _apiFactory.CreateAndAuthenticateAdminUserAsync(client);

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/v1/libraries/metadata-providers/{Uri.EscapeDataString(maliciousLibraryType)}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        // An unrecognized library type fails the validation of the query, so it is reported as a validation problem, and the response
        // must stay generic: the failed request leaks no database, filesystem or framework internals.
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", problemDetails["type"].GetString());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        Assert.NotNull(errors);
        Assert.Contains("UnknownLibraryType", errors!["General.Validation"]);
        Assert.StartsWith("/api/v1/libraries/metadata-providers/", problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);

        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Disposes the API factory resources.
    /// </summary>
    public void Dispose()
    {
        _client.Dispose();
    }
}
