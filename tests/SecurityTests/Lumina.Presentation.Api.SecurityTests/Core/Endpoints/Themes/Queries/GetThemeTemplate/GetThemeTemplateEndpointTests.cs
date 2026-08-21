#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Themes;
using Lumina.DataAccess.Core.UoW;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Themes.Queries.GetThemeTemplate;

/// <summary>
/// Contains security tests for the <c>/api/v1/themes/{themeId}/templates/{*pageKey}</c> route.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemeTemplateEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private const string DECOY_FILE_NAME = "decoy-secret.txt";
    private const string DECOY_CONTENT = "DECOY-SECRET-CONTENT";

    private readonly LuminaApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly string _themeId;
    private readonly ThemeEntityFixture _themeEntityFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemeTemplateEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetThemeTemplateEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // a unique X-Forwarded-For isolates rate limiting state per test
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
        _themeId = $"testtheme{Guid.NewGuid():N}";
    }

    [Fact]
    public async Task GetThemeTemplate_WhenThemeDoesNotExist_ShouldReturnCleanNotFoundProblemDetails()
    {
        // Arrange
        string url = $"/api/v1/themes/does-not-exist/templates/home";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problemDetails["type"].GetString());
        Assert.Equal("General.NotFound", problemDetails["title"].GetString());
        Assert.Equal("ThemeNotFound", problemDetails["detail"].GetString());
        Assert.Equal(url, problemDetails["instance"].GetString());
        Assert.True(problemDetails.ContainsKey("traceId"));
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("..%2F..%2Fevil.html")] // encoded slash path traversal
    [InlineData("%2e%2e%2f%2e%2e%2fevil.html")] // encoded dot-slash path traversal
    [InlineData("..%5C..%5Cevil.html")] // encoded backslash path traversal
    [InlineData("..%2F..%2F..%2F..%2Fdecoy-secret.txt")] // deep traversal targeting a decoy file outside the pack
    public async Task GetThemeTemplate_WithEncodedPathTraversalInPageKey_ShouldOnlyServeInPackTemplates(string maliciousPageKey)
    {
        // Arrange
        await SeedThemeAsync();
        EnsureThemePackOnDisk();
        EnsureDecoyFileOnDisk();

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/themes/{_themeId}/templates/{maliciousPageKey}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(DECOY_CONTENT, content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);

        // a successful render must only ever come from a template inside the theme pack
        if (response.StatusCode == HttpStatusCode.OK)
            Assert.Contains("default-template", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetThemeTemplate_WithRawPathTraversalInPageKey_ShouldNotLeakOrError()
    {
        // Arrange
        await SeedThemeAsync();
        EnsureThemePackOnDisk();
        EnsureDecoyFileOnDisk();

        // Act
        // note: Kestrel normalizes literal dot-segments out of the raw path, so the request is re-routed or rejected before the handler
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/themes/{_themeId}/templates/../evil.html");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(DECOY_CONTENT, content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("..%2F..%2Fevil")] // encoded slash traversal
    [InlineData("%2e%2e%2f%2e%2e%2fevil")] // encoded dot-slash traversal
    [InlineData("..%5Cevil")] // encoded backslash traversal
    [InlineData("..%2F")] // encoded trailing slash traversal
    public async Task GetThemeTemplate_WithPathTraversalInThemeId_ShouldReturnCleanNotFoundProblemDetails(string maliciousThemeId)
    {
        // Arrange
        string url = $"/api/v1/themes/{maliciousThemeId}/templates/home";

        // Act
        HttpResponseMessage response = await _client.GetAsync(url);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
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
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SeedThemeAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        dbContext.Themes.Add(_themeEntityFixture.Create(themeId: _themeId, isDeleted: false));
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Writes a valid theme pack for the unique test theme into the server storage directory.
    /// </summary>
    private void EnsureThemePackOnDisk()
    {
        string themeRoot = Path.Combine(AppContext.BaseDirectory, "themes", _themeId);
        Directory.CreateDirectory(Path.Combine(themeRoot, "templates"));
        Directory.CreateDirectory(Path.Combine(themeRoot, "assets"));
        string manifest = $$"""
            {
              "schemaVersion": 1,
              "id": "{{_themeId}}",
              "name": "Test Theme",
              "description": "Test theme description",
              "author": "Tester",
              "version": "1.0.0",
              "templates": {
                "default": "templates/default.html",
                "home": "templates/home.html"
              }
            }
            """;
        File.WriteAllText(Path.Combine(themeRoot, "theme.json"), manifest);
        File.WriteAllText(Path.Combine(themeRoot, "templates", "default.html"), "<html><body>default-template</body></html>");
        File.WriteAllText(Path.Combine(themeRoot, "templates", "home.html"), "<html><body>home-template</body></html>");
        File.WriteAllText(Path.Combine(themeRoot, "assets", "logo.txt"), "logo-content");
    }

    /// <summary>
    /// Writes a decoy file outside any theme pack, used to prove traversal payloads never escape the pack root.
    /// </summary>
    private static void EnsureDecoyFileOnDisk()
    {
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory, DECOY_FILE_NAME), DECOY_CONTENT);
    }

    /// <summary>
    /// Removes the seeded theme, its pack directory, and the decoy file.
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

        string themeRoot = Path.Combine(AppContext.BaseDirectory, "themes", _themeId);
        if (Directory.Exists(themeRoot))
            Directory.Delete(themeRoot, recursive: true);

        string decoyPath = Path.Combine(AppContext.BaseDirectory, DECOY_FILE_NAME);
        if (File.Exists(decoyPath))
            File.Delete(decoyPath);

        _client.Dispose();
    }
}
