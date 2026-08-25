#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Contracts.Fixtures.Core.Requests.Plugins;
using Lumina.Contracts.Requests.Plugins;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Plugins.UpdatePluginSettings;

/// <summary>
/// Contains security tests for the <c>/plugins/{pluginId}/settings</c> route.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePluginSettingsEndpointTests : IClassFixture<LuminaApiFactory>
{
    private readonly HttpClient _client;
    private readonly LuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private readonly PluginEntityFixture _pluginEntityFixture = new();
    private readonly UpdatePluginSettingsRequestFixture _updatePluginSettingsRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePluginSettingsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public UpdatePluginSettingsEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task UpdatePluginSettings_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        string pluginId = Guid.NewGuid().ToString();
        UpdatePluginSettingsRequest request = _updatePluginSettingsRequestFixture.Create(
            pluginId: Guid.NewGuid(),
            settings: new Dictionary<string, string> { ["Key"] = "Value" }
        );

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/plugins/{pluginId}/settings", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal($"/api/v1/plugins/{pluginId}/settings", problemDetails["instance"].GetProperty("value").GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("'; DROP TABLE Plugins--")] // destructive injection
    public async Task UpdatePluginSettings_WithSQLInjectionInSettings_ShouldNotLeakOrError(string maliciousValue)
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        await _apiFactory.CreateAndAuthenticateAdminUserAsync(client);
        Guid pluginId = await SeedPluginAsync();
        UpdatePluginSettingsRequest request = _updatePluginSettingsRequestFixture.Create(
            pluginId: pluginId,
            settings: new Dictionary<string, string> { ["Key"] = maliciousValue }
        );

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/v1/plugins/{pluginId}/settings", request);

        // Assert
        // the malicious value passes the authenticated handler, reaches the parameterized data access, and is persisted verbatim
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        PluginEntity? storedPlugin = await dbContext.Plugins.FirstOrDefaultAsync(plugin => plugin.Id == pluginId);
        Assert.NotNull(storedPlugin);
        Assert.NotNull(storedPlugin!.SettingsJson);
        Dictionary<string, string>? storedSettings = JsonSerializer.Deserialize<Dictionary<string, string>>(storedPlugin.SettingsJson!);
        Assert.NotNull(storedSettings);
        Assert.Equal(maliciousValue, storedSettings!["Key"]);
    }

    /// <summary>
    /// Seeds a <see cref="PluginEntity"/> created by the authenticated admin user.
    /// </summary>
    /// <returns>The Id of the seeded plugin.</returns>
    private async Task<Guid> SeedPluginAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid pluginId = Guid.NewGuid();
        dbContext.Plugins.Add(_pluginEntityFixture.Create(id: pluginId, name: "Test Plugin", loadStatus: PluginLoadStatus.Loaded, includeSettingsJson: false));
        await dbContext.SaveChangesAsync();
        return pluginId;
    }
}
