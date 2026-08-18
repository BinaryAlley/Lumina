#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Requests.Plugins;
using Lumina.Contracts.Responses.Plugins;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using Lumina.Presentation.Api.Core.Endpoints.Plugins.SetLibraryMetadataProviderEnabled;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.EntityFrameworkCore;
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

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Plugins.SetLibraryMetadataProviderEnabled;

/// <summary>
/// Contains integration tests for the <see cref="SetLibraryMetadataProviderEnabledEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetLibraryMetadataProviderEnabledEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="SetLibraryMetadataProviderEnabledEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public SetLibraryMetadataProviderEnabledEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task SetLibraryMetadataProviderEnabled_WhenNoConfigurationExists_ShouldCreateIt()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        SetLibraryMetadataProviderEnabledRequest request = new(
            LibraryId: libraryId,
            PluginId: pluginId,
            IsEnabled: true
        );

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/metadata-providers/{pluginId}/enabled", request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        LibraryMetadataProviderConfigurationEntity? configuration = await dbContext.LibraryMetadataProviderConfigurations
            .FirstOrDefaultAsync(config => config.LibraryId == libraryId && config.PluginId == pluginId);
        Assert.NotNull(configuration);
        Assert.True(configuration!.IsEnabled);
        Assert.Equal(1, configuration.Rank);
    }

    [Fact]
    public async Task SetLibraryMetadataProviderEnabled_WhenConfigurationExists_ShouldUpdateItsEnabledState()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        Guid userId = GetCurrentUserId();
        await SeedConfigurationAsync(libraryId, pluginId, userId, isEnabled: false, rank: 1);
        SetLibraryMetadataProviderEnabledRequest request = new(
            LibraryId: libraryId,
            PluginId: pluginId,
            IsEnabled: true
        );

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/metadata-providers/{pluginId}/enabled", request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        LibraryMetadataProviderConfigurationEntity? configuration = await dbContext.LibraryMetadataProviderConfigurations
            .FirstOrDefaultAsync(config => config.LibraryId == libraryId && config.PluginId == pluginId);
        Assert.NotNull(configuration);
        Assert.True(configuration!.IsEnabled);
    }

    /// <summary>
    /// Seeds a <see cref="LibraryMetadataProviderConfigurationEntity"/> in the database.
    /// </summary>
    /// <param name="libraryId">The Id of the media library.</param>
    /// <param name="pluginId">The Id of the plugin.</param>
    /// <param name="userId">The Id of the user that owns the data.</param>
    /// <param name="isEnabled">Whether the provider is enabled.</param>
    /// <param name="rank">The rank of the provider.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SeedConfigurationAsync(Guid libraryId, Guid pluginId, Guid userId, bool isEnabled, int rank)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        dbContext.LibraryMetadataProviderConfigurations.Add(new LibraryMetadataProviderConfigurationEntity
        {
            Id = Guid.NewGuid(),
            LibraryId = libraryId,
            PluginId = pluginId,
            IsEnabled = isEnabled,
            Rank = rank,
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
