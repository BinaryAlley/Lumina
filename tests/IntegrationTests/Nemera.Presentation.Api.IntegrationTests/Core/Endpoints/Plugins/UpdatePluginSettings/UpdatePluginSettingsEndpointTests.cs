#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Contracts.Fixtures.Core.Requests.Plugins;
using Lumina.Contracts.Requests.Plugins;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using Lumina.Presentation.Api.Core.Endpoints.Plugins.UpdatePluginSettings;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
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
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Plugins.UpdatePluginSettings;

/// <summary>
/// Contains integration tests for the <see cref="UpdatePluginSettingsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePluginSettingsEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly PluginEntityFixture _pluginEntityFixture = new();
    private readonly UpdatePluginSettingsRequestFixture _updatePluginSettingsRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePluginSettingsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public UpdatePluginSettingsEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        _apiFactory = apiFactory;
    }

    /// <summary>
    /// Initializes authenticated API client.
    /// </summary>
    public async Task InitializeAsync()
    {
        _client = await _apiFactory.CreateAuthenticatedClientAsync();
    }

    [Fact]
    public async Task UpdatePluginSettings_WhenPluginExists_ShouldUpdateTheSettings()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        await SeedPluginAsync(pluginId, "Test Plugin");
        UpdatePluginSettingsRequest request = _updatePluginSettingsRequestFixture.Create(
            pluginId: pluginId,
            settings: new Dictionary<string, string> { ["Key1"] = "Value1" }
        );

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/plugins/{pluginId}/settings", request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        PluginEntity? storedPlugin = await dbContext.Plugins.FirstOrDefaultAsync(plugin => plugin.Id == pluginId);
        Assert.NotNull(storedPlugin);
        Assert.Contains("Value1", storedPlugin!.SettingsJson);
    }

    [Fact]
    public async Task UpdatePluginSettings_WhenPluginDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        UpdatePluginSettingsRequest request = _updatePluginSettingsRequestFixture.Create(
            pluginId: pluginId,
            settings: new Dictionary<string, string> { ["Key1"] = "Value1" }
        );

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/plugins/{pluginId}/settings", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problemDetails["type"].GetString());
        Assert.Equal("General.NotFound", problemDetails["title"].GetString());
        Assert.Equal("PluginNotFound", problemDetails["detail"].GetString());
        Assert.Equal($"/api/v1/plugins/{pluginId}/settings", problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);
    }

    /// <summary>
    /// Seeds a <see cref="PluginEntity"/> in the database.
    /// </summary>
    /// <param name="pluginId">The Id of the plugin.</param>
    /// <param name="name">The display name of the plugin.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SeedPluginAsync(Guid pluginId, string name)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        dbContext.Plugins.Add(_pluginEntityFixture.Create(id: pluginId, name: name, loadStatus: PluginLoadStatus.Loaded, includeSettingsJson: false));
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _apiFactory.RemoveTestUserAsync();
    }
}
