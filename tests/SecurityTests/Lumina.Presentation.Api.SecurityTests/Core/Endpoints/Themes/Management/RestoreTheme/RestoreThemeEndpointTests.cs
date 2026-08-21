#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Themes;
using Lumina.DataAccess.Core.UoW;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Themes.Management.RestoreTheme;

/// <summary>
/// Contains security tests for the <c>POST /api/v1/themes/{themeId}/restore</c> route.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreThemeEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private readonly LuminaApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly string _themeId;
    private readonly ThemeEntityFixture _themeEntityFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreThemeEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public RestoreThemeEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // a unique X-Forwarded-For isolates rate limiting state per test
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
        _themeId = $"testtheme{Guid.NewGuid():N}";
    }

    [Fact]
    public async Task RestoreTheme_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        string url = $"/api/v1/themes/{Guid.NewGuid():N}/restore";

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync(url, new { });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal(url, problemDetails["instance"].GetProperty("value").GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
    }

    [Fact]
    public async Task RestoreTheme_WhenAuthenticatedNonAdmin_ShouldReturnForbiddenResult()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        await _apiFactory.CreateAndAuthenticateUserAsync(client);
        string url = $"/api/v1/themes/{Guid.NewGuid():N}/restore";

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync(url, new { });
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());
        Assert.Equal(url, problemDetails["instance"].GetString());
        Assert.True(problemDetails.ContainsKey("traceId"));
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RestoreTheme_WhenThemeIsNotDeleted_ShouldReturnForbiddenResultWithoutReactivatingIt()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        await _apiFactory.CreateAndAuthenticateAdminUserAsync(client);
        await SeedThemeAsync(isDeleted: false);
        string url = $"/api/v1/themes/{_themeId}/restore";

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync(url, new { });
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Forbidden", problemDetails["title"].GetString());
        Assert.Equal("ThemeCannotBeRestored", problemDetails["detail"].GetString());
        Assert.Equal(url, problemDetails["instance"].GetString());
        Assert.True(problemDetails.ContainsKey("traceId"));
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("..%2F..%2Fevil")] // encoded slash traversal
    [InlineData("%2e%2e%2f%2e%2e%2fevil")] // encoded dot-slash traversal
    [InlineData("..%5Cevil")] // encoded backslash traversal
    [InlineData("..%2F")] // encoded trailing slash traversal
    public async Task RestoreTheme_WithPathTraversalInThemeId_ShouldReturnCleanNotFoundProblemDetails(string maliciousThemeId)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        await _apiFactory.CreateAndAuthenticateAdminUserAsync(client);
        string url = $"/api/v1/themes/{maliciousThemeId}/restore";

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync(url, new { });
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);

        // note: some payloads are normalized away by the server before routing, producing an empty-body 404;
        // when the value reaches the handler, the failure must still be the generic theme-not-found problem
        if (!string.IsNullOrWhiteSpace(content))
        {
            Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
            Assert.NotNull(problemDetails);
            Assert.Equal(StatusCodes.Status404NotFound, problemDetails!["status"].GetInt32());
            Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problemDetails["type"].GetString());
            Assert.Equal("General.NotFound", problemDetails["title"].GetString());
            Assert.Equal("ThemeNotFound", problemDetails["detail"].GetString());
        }
    }

    /// <summary>
    /// Seeds a <see cref="ThemeEntity"/> with the unique test theme id.
    /// </summary>
    /// <param name="isDeleted">Whether the seeded theme is marked as deleted.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SeedThemeAsync(bool isDeleted)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        dbContext.Themes.Add(_themeEntityFixture.Create(themeId: _themeId, isDeleted: isDeleted));
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Removes the seeded theme.
    /// </summary>
    public void Dispose()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        ThemeEntity? theme = dbContext.Themes.FirstOrDefault(candidate => candidate.ThemeId == _themeId);
        if (theme is not null)
        {
            dbContext.Themes.Remove(theme);
            dbContext.SaveChanges();
        }

        _client.Dispose();
    }
}
