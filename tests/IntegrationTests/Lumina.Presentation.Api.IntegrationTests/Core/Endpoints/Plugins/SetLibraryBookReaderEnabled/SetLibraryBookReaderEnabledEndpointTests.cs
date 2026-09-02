#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Fixtures.Core.Requests.Plugins;
using Lumina.Contracts.Requests.Plugins;
using Lumina.DataAccess.Core.UoW;
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

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Plugins.SetLibraryBookReaderEnabled;

/// <summary>
/// Contains integration tests for the <c>/libraries/{libraryId}/book-readers/{pluginId}/enabled</c> route.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetLibraryBookReaderEnabledEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly LibraryEntityFixture _libraryEntityFixture = new();
    private readonly LibraryBookReaderConfigurationEntityFixture _configurationFixture = new();
    private readonly SetLibraryBookReaderEnabledRequestFixture _requestFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SetLibraryBookReaderEnabledEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public SetLibraryBookReaderEnabledEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task SetLibraryBookReaderEnabled_WhenNoConfigurationExists_ShouldCreateIt()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, userId);
        Guid pluginId = Guid.NewGuid();
        SetLibraryBookReaderEnabledRequest request = _requestFixture.Create(libraryId: libraryId, pluginId: pluginId, isEnabled: true);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/book-readers/{pluginId}/enabled", request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        LibraryBookReaderConfigurationEntity? configuration = await dbContext.LibraryBookReaderConfigurations
            .FirstOrDefaultAsync(config => config.LibraryId == libraryId && config.PluginId == pluginId);
        Assert.NotNull(configuration);
        Assert.True(configuration!.IsEnabled);
    }

    [Fact]
    public async Task SetLibraryBookReaderEnabled_WhenConfigurationExists_ShouldUpdateItsEnabledState()
    {
        // Arrange
        Guid userId = GetCurrentUserId();
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, userId);
        Guid pluginId = Guid.NewGuid();
        await SeedConfigurationAsync(libraryId, pluginId, isEnabled: false);
        SetLibraryBookReaderEnabledRequest request = _requestFixture.Create(libraryId: libraryId, pluginId: pluginId, isEnabled: true);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/book-readers/{pluginId}/enabled", request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        LibraryBookReaderConfigurationEntity configuration = await dbContext.LibraryBookReaderConfigurations
            .SingleAsync(config => config.LibraryId == libraryId && config.PluginId == pluginId);
        Assert.True(configuration.IsEnabled);
    }

    [Fact]
    public async Task SetLibraryBookReaderEnabled_WhenLibraryIsNotOwned_ShouldReturnForbidden()
    {
        // Arrange
        Guid otherUserId = await SeedOtherUserAsync();
        Guid libraryId = Guid.NewGuid();
        await SeedLibraryAsync(libraryId, otherUserId);
        Guid pluginId = Guid.NewGuid();
        SetLibraryBookReaderEnabledRequest request = _requestFixture.Create(libraryId: libraryId, pluginId: pluginId, isEnabled: true);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/book-readers/{pluginId}/enabled", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task SetLibraryBookReaderEnabled_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        SetLibraryBookReaderEnabledRequest request = _requestFixture.Create(libraryId: libraryId, pluginId: pluginId, isEnabled: true);

        // Act
        HttpResponseMessage response = await unauthenticatedClient.PutAsJsonAsync($"/api/v1/libraries/{libraryId}/book-readers/{pluginId}/enabled", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Seeds a library owned by <paramref name="userId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the library to seed.</param>
    /// <param name="userId">The Id of the user that owns the library.</param>
    private async Task SeedLibraryAsync(Guid libraryId, Guid userId)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        dbContext.Libraries.Add(_libraryEntityFixture.Create(id: libraryId, userId: userId, title: "Test Library", libraryType: Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary.LibraryType.EBook, contentLocations: []));
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds a book reader configuration for the library and plugin.
    /// </summary>
    /// <param name="libraryId">The Id of the library.</param>
    /// <param name="pluginId">The Id of the plugin.</param>
    /// <param name="isEnabled">Whether the book reader is enabled.</param>
    private async Task SeedConfigurationAsync(Guid libraryId, Guid pluginId, bool isEnabled)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        dbContext.LibraryBookReaderConfigurations.Add(_configurationFixture.Create(libraryId: libraryId, pluginId: pluginId, isEnabled: isEnabled));
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds a user that is not the authenticated one and returns its Id.
    /// </summary>
    /// <returns>The Id of the seeded user.</returns>
    private async Task<Guid> SeedOtherUserAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid userId = Guid.NewGuid();
        dbContext.Users.Add(_userEntityFixture.Create(id: userId, username: $"otheruser_{Guid.NewGuid()}", password: "TestPass123!"));
        await dbContext.SaveChangesAsync();
        return userId;
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
