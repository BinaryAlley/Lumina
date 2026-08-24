#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Contracts.Fixtures.Core.Requests.Plugins;
using Lumina.Contracts.Requests.Plugins;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Plugins.ReorderLibraryMetadataProviders;
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
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Plugins.ReorderLibraryMetadataProviders;

/// <summary>
/// Contains integration tests for the <see cref="ReorderLibraryMetadataProvidersEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReorderLibraryMetadataProvidersEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly LibraryMetadataProviderConfigurationEntityFixture _libraryMetadataProviderConfigurationEntityFixture = new();
    private readonly ReorderLibraryMetadataProvidersRequestFixture _reorderLibraryMetadataProvidersRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ReorderLibraryMetadataProvidersEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public ReorderLibraryMetadataProvidersEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task ReorderLibraryMetadataProviders_WhenCalledWithValidData_ShouldReorderTheProviders()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, userId);
        Guid pluginIdA = Guid.NewGuid();
        Guid pluginIdB = Guid.NewGuid();
        await SeedConfigurationAsync(libraryId, pluginIdA, rank: 1);
        await SeedConfigurationAsync(libraryId, pluginIdB, rank: 2);
        ReorderLibraryMetadataProvidersRequest request = _reorderLibraryMetadataProvidersRequestFixture.Create(
            libraryId: libraryId,
            pluginIds: [pluginIdB, pluginIdA]
        );

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/metadata-providers/reorder", request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        List<LibraryMetadataProviderConfigurationEntity> configurations = await dbContext.LibraryMetadataProviderConfigurations
            .Where(config => config.LibraryId == libraryId)
            .OrderBy(config => config.Rank)
            .ToListAsync();
        Assert.Equal(2, configurations.Count);
        Assert.Equal(pluginIdB, configurations[0].PluginId);
        Assert.Equal(1, configurations[0].Rank);
        Assert.Equal(pluginIdA, configurations[1].PluginId);
        Assert.Equal(2, configurations[1].Rank);
    }

    /// <summary>
    /// Seeds a <see cref="Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management.LibraryEntity"/> owned by <paramref name="userId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the library to seed.</param>
    /// <param name="userId">The Id of the user that owns the library.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SeedLibraryAsync(Guid libraryId, Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Test Library", libraryType: LibraryType.EBook, contentLocations: []));
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds a <see cref="LibraryMetadataProviderConfigurationEntity"/> in the database.
    /// </summary>
    /// <param name="libraryId">The Id of the media library.</param>
    /// <param name="pluginId">The Id of the plugin.</param>
    /// <param name="rank">The rank of the provider.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SeedConfigurationAsync(Guid libraryId, Guid pluginId, int rank)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        dbContext.LibraryMetadataProviderConfigurations.Add(_libraryMetadataProviderConfigurationEntityFixture.Create(libraryId: libraryId, pluginId: pluginId, rank: rank));
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
