#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Themes.Queries.GetCurrentTheme;

/// <summary>
/// Contains security tests for the <c>/api/v1/themes/current</c> route.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetCurrentThemeEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCurrentThemeEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetCurrentThemeEndpointTests(LuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        // a unique X-Forwarded-For isolates rate limiting state per test
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
    }

    [Fact]
    public async Task GetCurrentTheme_WhenAnonymous_ShouldNotRequireAuthenticationOrLeakInternalDetails()
    {
        // Arrange
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/themes/current");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        // the route is AllowAnonymous, so anonymous callers must never be challenged for credentials
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);

        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);

        // when no active theme exists yet, the failure must be a clean, generic 404 that discloses nothing internal
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
            Assert.NotNull(problemDetails);
            Assert.Equal(StatusCodes.Status404NotFound, problemDetails!["status"].GetInt32());
            Assert.Equal("General.NotFound", problemDetails["title"].GetString());
            Assert.Equal("ThemeNotFound", problemDetails["detail"].GetString());
            Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problemDetails["type"].GetString());
        }
    }

    /// <summary>
    /// Disposes the API factory resources.
    /// </summary>
    public void Dispose()
    {
        _client.Dispose();
    }
}
