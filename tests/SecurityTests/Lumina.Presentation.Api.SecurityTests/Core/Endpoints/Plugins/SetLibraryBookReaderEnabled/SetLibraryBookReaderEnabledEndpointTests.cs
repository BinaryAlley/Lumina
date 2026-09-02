#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.Plugins;
using Lumina.Contracts.Requests.Plugins;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Plugins.SetLibraryBookReaderEnabled;

/// <summary>
/// Contains security tests for the <c>/libraries/{libraryId}/book-readers/{pluginId}/enabled</c> route.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetLibraryBookReaderEnabledEndpointTests : IClassFixture<LuminaApiFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private readonly SetLibraryBookReaderEnabledRequestFixture _requestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SetLibraryBookReaderEnabledEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public SetLibraryBookReaderEnabledEndpointTests(LuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task SetLibraryBookReaderEnabled_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        SetLibraryBookReaderEnabledRequest request = _requestFixture.Create(libraryId: libraryId, pluginId: pluginId, isEnabled: true);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/book-readers/{pluginId}/enabled", request);

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
    [InlineData("'; DROP TABLE LibraryBookReaderConfigurations--")] // destructive injection
    public async Task SetLibraryBookReaderEnabled_WithSQLInjectionInPluginId_ShouldNotLeakDatabaseDetails(string maliciousPluginId)
    {
        // Arrange
        SetLibraryBookReaderEnabledRequest request = _requestFixture.Create(libraryId: Guid.NewGuid(), pluginId: Guid.NewGuid(), isEnabled: true);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/libraries/{Guid.NewGuid()}/book-readers/{Uri.EscapeDataString(maliciousPluginId)}/enabled", request);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
    }
}
