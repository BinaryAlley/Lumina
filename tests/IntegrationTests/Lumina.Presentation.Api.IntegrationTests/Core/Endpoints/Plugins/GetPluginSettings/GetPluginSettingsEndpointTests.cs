#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using Lumina.Presentation.Api.Core.Endpoints.Plugins.GetPluginSettings;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Plugins.GetPluginSettings;

/// <summary>
/// Contains integration tests for the <see cref="GetPluginSettingsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPluginSettingsEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPluginSettingsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetPluginSettingsEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task GetPluginSettings_WhenPluginExists_ShouldReturnPluginSettings()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();
        await SeedPluginAsync(pluginId, "Test Plugin");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/plugins/{pluginId}/settings");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Lumina.Contracts.Responses.Plugins.PluginSettingsResponse? settings = await response.Content.ReadFromJsonAsync<Lumina.Contracts.Responses.Plugins.PluginSettingsResponse>(_jsonOptions);
        Assert.NotNull(settings);
        Assert.Equal(pluginId, settings!.PluginId);
    }

    [Fact]
    public async Task GetPluginSettings_WhenPluginDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        Guid pluginId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/plugins/{pluginId}/settings");

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
        Guid userId = GetCurrentUserId();
        dbContext.Plugins.Add(new PluginEntity
        {
            Id = pluginId,
            Name = name,
            Author = "Test Author",
            Version = "1.0.0",
            Description = "A test plugin.",
            LoadStatus = PluginLoadStatus.Loaded,
            LoadError = null,
            SettingsJson = null,
            CreatedBy = userId,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedBy = null
        });
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Gets the Id of the currently authenticated test user.
    /// </summary>
    /// <returns>The Id of the authenticated test user.</returns>
    private Guid GetCurrentUserId()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        UserEntity user = dbContext.Users.First(user => user.Username == _apiFactory.TestUsername);
        return user.Id;
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _apiFactory.RemoveTestUserAsync();
    }
}
