#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Contracts.Fixtures.Core.Requests.Plugins;
using Lumina.Contracts.Requests.Plugins;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Plugins.SetLibraryArtworkProviderEnabled;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Plugins.SetLibraryArtworkProviderEnabled;

/// <summary>
/// Contains integration tests for the <see cref="SetLibraryArtworkProviderEnabledEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetLibraryArtworkProviderEnabledEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly LibraryArtworkProviderConfigurationEntityFixture _libraryArtworkProviderConfigurationEntityFixture = new();
    private readonly SetLibraryArtworkProviderEnabledRequestFixture _setLibraryArtworkProviderEnabledRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SetLibraryArtworkProviderEnabledEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public SetLibraryArtworkProviderEnabledEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task SetLibraryArtworkProviderEnabled_WhenNoConfigurationExists_ShouldCreateIt()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, userId);
        Guid pluginId = Guid.NewGuid();
        SetLibraryArtworkProviderEnabledRequest request = _setLibraryArtworkProviderEnabledRequestFixture.Create(libraryId: libraryId, pluginId: pluginId, isEnabled: true);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/artwork-providers/{pluginId}/enabled", request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        LibraryArtworkProviderConfigurationEntity? configuration = await dbContext.LibraryArtworkProviderConfigurations
            .FirstOrDefaultAsync(config => config.LibraryId == libraryId && config.PluginId == pluginId);
        Assert.NotNull(configuration);
        Assert.True(configuration!.IsEnabled);
        Assert.Equal(1, configuration.Rank);
    }

    [Fact]
    public async Task SetLibraryArtworkProviderEnabled_WhenConfigurationExists_ShouldUpdateItsEnabledState()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, userId);
        Guid pluginId = Guid.NewGuid();
        await SeedConfigurationAsync(libraryId, pluginId, isEnabled: false, rank: 1);
        SetLibraryArtworkProviderEnabledRequest request = _setLibraryArtworkProviderEnabledRequestFixture.Create(libraryId: libraryId, pluginId: pluginId, isEnabled: true);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/artwork-providers/{pluginId}/enabled", request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        LibraryArtworkProviderConfigurationEntity? configuration = await dbContext.LibraryArtworkProviderConfigurations
            .FirstOrDefaultAsync(config => config.LibraryId == libraryId && config.PluginId == pluginId);
        Assert.NotNull(configuration);
        Assert.True(configuration!.IsEnabled);
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
    /// Seeds a <see cref="LibraryArtworkProviderConfigurationEntity"/> in the database.
    /// </summary>
    /// <param name="libraryId">The Id of the media library.</param>
    /// <param name="pluginId">The Id of the plugin.</param>
    /// <param name="isEnabled">Whether the provider is enabled.</param>
    /// <param name="rank">The rank of the provider.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SeedConfigurationAsync(Guid libraryId, Guid pluginId, bool isEnabled, int rank)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        dbContext.LibraryArtworkProviderConfigurations.Add(_libraryArtworkProviderConfigurationEntityFixture.Create(libraryId: libraryId, pluginId: pluginId, rank: rank, isEnabled: isEnabled));
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
