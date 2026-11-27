#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.UsersManagement.Settings;
using Lumina.DataAccess.Core.UoW;
using Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Settings.GetPlaybackState;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.UsersManagement.Settings.GetPlaybackState;

/// <summary>
/// Contains integration tests for the <see cref="GetPlaybackStateEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaybackStateEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly UserSettingsEntityFixture _userSettingsEntityFixture = new();
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaybackStateEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetPlaybackStateEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        _apiFactory = apiFactory;
    }

    /// <summary>
    /// Initializes the authenticated API client.
    /// </summary>
    public async Task InitializeAsync()
    {
        _client = await _apiFactory.CreateAuthenticatedClientAsync();
    }

    [Fact]
    public async Task GetPlaybackState_WhenNoSettingsAreStored_ShouldReturnTheDefaultPlaybackState()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/users/me/settings/playback-state");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();
        PlaybackStateResponse? result = JsonSerializer.Deserialize<PlaybackStateResponse>(content, _jsonOptions);

        Assert.NotNull(result);
        Assert.Equal(1.0, result!.Volume);
        Assert.False(result.IsMuted);
    }

    [Fact]
    public async Task GetPlaybackState_WhenSettingsAreStored_ShouldReturnTheStoredPlaybackState()
    {
        // Arrange
        await CreateTestUserSettings(volume: 0.42, isMuted: true);

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/users/me/settings/playback-state");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();
        PlaybackStateResponse? result = JsonSerializer.Deserialize<PlaybackStateResponse>(content, _jsonOptions);

        Assert.NotNull(result);
        Assert.Equal(0.42, result!.Volume);
        Assert.True(result.IsMuted);
    }

    [Fact]
    public async Task GetPlaybackState_WhenSettingsBelongToAnotherUser_ShouldReturnTheDefaultPlaybackState()
    {
        // Arrange
        await CreateSettingsForAnotherUserAsync(volume: 0.1, isMuted: true);

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/users/me/settings/playback-state");

        // Assert
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync();
        PlaybackStateResponse? result = JsonSerializer.Deserialize<PlaybackStateResponse>(content, _jsonOptions);

        Assert.NotNull(result);
        Assert.Equal(1.0, result!.Volume);
        Assert.False(result.IsMuted);
    }

    [Fact]
    public async Task GetPlaybackState_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await unauthenticatedClient.GetAsync("/api/v1/users/me/settings/playback-state");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Stores the playback state of the currently authenticated test user.
    /// </summary>
    /// <param name="volume">The volume to store.</param>
    /// <param name="isMuted">The mute state to store.</param>
    /// <returns>The stored settings.</returns>
    private async Task<UserSettingsEntity> CreateTestUserSettings(double volume, bool isMuted)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        Guid userId = dbContext.Users.First(user => user.Username == _apiFactory.TestUsername).Id;
        UserSettingsEntity settings = _userSettingsEntityFixture.Create(userId: userId, volume: volume, isMuted: isMuted);

        dbContext.UserSettings.Add(settings);
        await dbContext.SaveChangesAsync();
        return settings;
    }

    /// <summary>
    /// Seeds another user and stores that user's playback state, keeping it out of the current user's scope.
    /// </summary>
    /// <param name="volume">The volume to store.</param>
    /// <param name="isMuted">The mute state to store.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task CreateSettingsForAnotherUserAsync(double volume, bool isMuted)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        UserEntity otherUser = _userEntityFixture.Create(username: $"otheruser_{Guid.NewGuid()}", password: "TestPass123!");
        otherUser.TotpSecret = null;
        dbContext.Users.Add(otherUser);
        dbContext.UserSettings.Add(_userSettingsEntityFixture.Create(userId: otherUser.Id, volume: volume, isMuted: isMuted));
        await dbContext.SaveChangesAsync();
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
