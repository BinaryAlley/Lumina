#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.Plugins;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Nemera.DataAccess.Core.UoW;
using Nemera.Domain.SharedKernel.Common.Enums.Plugins;
using Nemera.Presentation.Api.Core.Endpoints.Plugins.GetPlugins;
using Nemera.Presentation.Api.IntegrationTests.Common.Setup;
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

namespace Nemera.Presentation.Api.IntegrationTests.Core.Endpoints.Plugins.GetPlugins;

/// <summary>
/// Contains integration tests for the <see cref="GetPluginsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPluginsEndpointTests : IClassFixture<AuthenticatedNemeraApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedNemeraApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private readonly List<Guid> _seededPluginIds = [];
    private readonly PluginEntityFixture _pluginEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPluginsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetPluginsEndpointTests(AuthenticatedNemeraApiFactory apiFactory)
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
    public async Task GetPlugins_WhenPluginsExist_ShouldReturnTheDetectedPlugins()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        await SeedPluginAsync(pluginId, "Test Plugin");

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/plugins");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<Nemera.Contracts.Responses.Plugins.PluginResponse>? plugins = await response.Content.ReadFromJsonAsync<List<Nemera.Contracts.Responses.Plugins.PluginResponse>>(_jsonOptions);
        Assert.NotNull(plugins);
        Assert.Contains(plugins!, plugin => plugin.Id == pluginId && plugin.Name == "Test Plugin");
    }

    [Fact]
    public async Task GetPlugins_WhenNoPluginsExist_ShouldReturnEmptyList()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/plugins");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<Nemera.Contracts.Responses.Plugins.PluginResponse>? plugins = await response.Content.ReadFromJsonAsync<List<Nemera.Contracts.Responses.Plugins.PluginResponse>>(_jsonOptions);
        Assert.NotNull(plugins);
        Assert.Empty(plugins!);
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
        NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();
        dbContext.Plugins.Add(_pluginEntityFixture.Create(id: pluginId, name: name, loadStatus: PluginLoadStatus.Loaded, includeSettingsJson: false));
        await dbContext.SaveChangesAsync();
        _seededPluginIds.Add(pluginId);
    }

    /// <summary>
    /// Removes the seeded plugins and disposes API factory resources.
    /// </summary>
    public async Task DisposeAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        NemeraDbContext dbContext = scope.ServiceProvider.GetRequiredService<NemeraDbContext>();
        foreach (Guid pluginId in _seededPluginIds)
        {
            PluginEntity? plugin = await dbContext.Plugins.FindAsync(pluginId);
            if (plugin is not null)
                dbContext.Plugins.Remove(plugin);
        }
        await dbContext.SaveChangesAsync();
        await _apiFactory.RemoveTestUserAsync();
    }
}
