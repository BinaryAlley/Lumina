#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.UsersManagement.Settings;
using Lumina.DataAccess.Core.UoW;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.UsersManagement.Settings.GetUserSettings;

/// <summary>
/// Contains integration tests for the <see cref="GetUserSettingsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserSettingsEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserSettingsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetUserSettingsEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task GetUserSettings_WhenSettingsExist_ShouldReturnStoredSettings()
    {
        // Arrange
        UserSettingsEntity storedSettings = await CreateTestUserSettings();

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/users/me/settings");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();
        UserSettingsResponse? result = JsonSerializer.Deserialize<UserSettingsResponse>(content, _jsonOptions);

        Assert.NotNull(result);
        Assert.Equal(storedSettings.UserId, result!.UserId);
        Assert.Equal(storedSettings.IsPaginationEnabled, result.IsPaginationEnabled);
        Assert.Equal(storedSettings.ItemsPerPage, result.ItemsPerPage);
        Assert.Equal(storedSettings.IgnoreThePrefixForAlphaPicker, result.IgnoreThePrefixForAlphaPicker);
    }

    [Fact]
    public async Task GetUserSettings_WhenSettingsDoNotExist_ShouldReturnDefaultSettings()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/users/me/settings");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();
        UserSettingsResponse? result = JsonSerializer.Deserialize<UserSettingsResponse>(content, _jsonOptions);

        Assert.NotNull(result);
        Assert.True(result!.IsPaginationEnabled);
        Assert.Equal(48, result.ItemsPerPage);
        Assert.False(result.IgnoreThePrefixForAlphaPicker);
    }

    [Fact]
    public async Task GetUserSettings_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await unauthenticatedClient.GetAsync("/api/v1/users/me/settings");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<UserSettingsEntity> CreateTestUserSettings()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        Guid userId = dbContext.Users.First(user => user.Username == _apiFactory.TestUsername).Id;
        UserSettingsEntity settings = new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            IsPaginationEnabled = false,
            ItemsPerPage = 24,
            IgnoreThePrefixForAlphaPicker = true
        };

        dbContext.UserSettings.Add(settings);
        await dbContext.SaveChangesAsync();
        return settings;
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public async Task DisposeAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        await dbContext.Set<UserSettingsEntity>().ExecuteDeleteAsync();
        await dbContext.Set<UserEntity>().ExecuteDeleteAsync();
        await dbContext.SaveChangesAsync();
    }
}
