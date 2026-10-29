#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Fixtures.Core.Requests.UsersManagement.Settings;
using Lumina.Contracts.Requests.UsersManagement.Settings;
using Lumina.Contracts.Responses.UsersManagement.Settings;
using Lumina.DataAccess.Core.UoW;
using Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Settings.UpdatePlaybackState;
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
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.UsersManagement.Settings.UpdatePlaybackState;

/// <summary>
/// Contains integration tests for the <see cref="UpdatePlaybackStateEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePlaybackStateEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly UpdatePlaybackStateRequestFixture _updatePlaybackStateRequestFixture = new();
    private readonly UserSettingsEntityFixture _userSettingsEntityFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePlaybackStateEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public UpdatePlaybackStateEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task UpdatePlaybackState_WhenCalledWithValidRequest_ShouldStoreTheReportedStateWithTheDefaultHandEditedSettings()
    {
        // Arrange
        UpdatePlaybackStateRequest request = _updatePlaybackStateRequestFixture.Create(volume: 0.35, isMuted: true);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/users/me/settings/playback-state", request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        UserSettingsEntity? storedSettings = await GetCurrentUserSettingsAsync();
        Assert.NotNull(storedSettings);
        Assert.Equal(0.35, storedSettings!.Volume);
        Assert.True(storedSettings.IsMuted);
        Assert.True(storedSettings.IsPaginationEnabled);
        Assert.Equal(48, storedSettings.ItemsPerPage);
    }

    [Fact]
    public async Task UpdatePlaybackState_WhenSettingsAlreadyExist_ShouldPreserveTheHandEditedSettings()
    {
        // Arrange
        await CreateTestUserSettingsAsync(volume: 1.0, isMuted: false, itemsPerPage: 24, isPaginationEnabled: false);
        UpdatePlaybackStateRequest request = _updatePlaybackStateRequestFixture.Create(volume: 0.25, isMuted: true);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/users/me/settings/playback-state", request);

        // Assert
        response.EnsureSuccessStatusCode();

        UserSettingsEntity? storedSettings = await GetCurrentUserSettingsAsync();
        Assert.NotNull(storedSettings);
        Assert.Equal(0.25, storedSettings!.Volume);
        Assert.True(storedSettings.IsMuted);
        Assert.False(storedSettings.IsPaginationEnabled);
        Assert.Equal(24, storedSettings.ItemsPerPage);
    }

    [Fact]
    public async Task UpdatePlaybackState_WhenUpdatedThenRead_ShouldReturnTheReportedState()
    {
        // Arrange
        UpdatePlaybackStateRequest request = _updatePlaybackStateRequestFixture.Create(volume: 0.6, isMuted: true);

        // Act
        HttpResponseMessage updateResponse = await _client.PutAsJsonAsync("/api/v1/users/me/settings/playback-state", request);
        HttpResponseMessage getResponse = await _client.GetAsync("/api/v1/users/me/settings/playback-state");

        // Assert
        updateResponse.EnsureSuccessStatusCode();
        getResponse.EnsureSuccessStatusCode();
        string content = await getResponse.Content.ReadAsStringAsync();
        PlaybackStateResponse? result = JsonSerializer.Deserialize<PlaybackStateResponse>(content, _jsonOptions);
        Assert.NotNull(result);
        Assert.Equal(0.6, result!.Volume);
        Assert.True(result.IsMuted);
    }

    [Fact]
    public async Task UpdatePlaybackState_WhenCalledByOneUser_ShouldNotChangeThePlaybackStateOfAnotherUser()
    {
        // Arrange
        UpdatePlaybackStateRequest request = _updatePlaybackStateRequestFixture.Create(volume: 0.25, isMuted: true);
        HttpResponseMessage updateResponse = await _client.PutAsJsonAsync("/api/v1/users/me/settings/playback-state", request);
        HttpClient otherClient = await _apiFactory.CreateAuthenticatedClientAsync();

        // Act
        HttpResponseMessage getResponse = await otherClient.GetAsync("/api/v1/users/me/settings/playback-state");

        // Assert
        updateResponse.EnsureSuccessStatusCode();
        getResponse.EnsureSuccessStatusCode();
        string content = await getResponse.Content.ReadAsStringAsync();
        PlaybackStateResponse? result = JsonSerializer.Deserialize<PlaybackStateResponse>(content, _jsonOptions);
        Assert.NotNull(result);
        Assert.Equal(1.0, result!.Volume);
        Assert.False(result.IsMuted);
    }

    [Fact]
    public async Task UpdatePlaybackState_WhenVolumeIsNull_ShouldReturnValidationError()
    {
        // Arrange
        UpdatePlaybackStateRequest request = _updatePlaybackStateRequestFixture.Create(includeVolume: false);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/users/me/settings/playback-state", request);

        // Assert
        await AssertValidationErrorAsync(response, "VolumeCannotBeNull");
    }

    [Theory]
    [InlineData(-0.01)] // just below the minimum
    [InlineData(1.01)] // just above the maximum
    public async Task UpdatePlaybackState_WhenVolumeIsOutOfRange_ShouldReturnValidationError(double volume)
    {
        // Arrange
        UpdatePlaybackStateRequest request = _updatePlaybackStateRequestFixture.Create(volume: volume);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/users/me/settings/playback-state", request);

        // Assert
        await AssertValidationErrorAsync(response, "VolumeMustBeBetweenZeroAndOne");
    }

    [Fact]
    public async Task UpdatePlaybackState_WhenIsMutedIsNull_ShouldReturnValidationError()
    {
        // Arrange
        UpdatePlaybackStateRequest request = _updatePlaybackStateRequestFixture.Create(includeIsMuted: false);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/users/me/settings/playback-state", request);

        // Assert
        await AssertValidationErrorAsync(response, "IsMutedCannotBeNull");
    }

    [Fact]
    public async Task UpdatePlaybackState_WhenPayloadHasUnknownFields_ShouldIgnoreThemAndStoreTheReportedState()
    {
        // Arrange
        UpdatePlaybackStateRequest request = _updatePlaybackStateRequestFixture.Create(volume: 0.15, isMuted: false);
        JsonObject payload = JsonSerializer.SerializeToNode(request, new JsonSerializerOptions(JsonSerializerDefaults.Web))!.AsObject();
        payload["unknownField"] = "ignored";
        StringContent requestContent = new(payload.ToJsonString(), Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await _client.PutAsync("/api/v1/users/me/settings/playback-state", requestContent);

        // Assert
        response.EnsureSuccessStatusCode();
        UserSettingsEntity? storedSettings = await GetCurrentUserSettingsAsync();
        Assert.NotNull(storedSettings);
        Assert.Equal(0.15, storedSettings!.Volume);
        Assert.False(storedSettings.IsMuted);
    }

    [Fact]
    public async Task UpdatePlaybackState_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();
        UpdatePlaybackStateRequest request = _updatePlaybackStateRequestFixture.Create(volume: 0.35, isMuted: true);

        // Act
        HttpResponseMessage response = await unauthenticatedClient.PutAsJsonAsync("/api/v1/users/me/settings/playback-state", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Asserts that <paramref name="response"/> is a validation problem response carrying the specified error.
    /// </summary>
    /// <param name="response">The response to assert.</param>
    /// <param name="expectedError">The exact error description expected in the validation errors.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task AssertValidationErrorAsync(HttpResponseMessage response, string expectedError)
    {
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", problemDetails["type"].GetString());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("/api/v1/users/me/settings/playback-state", problemDetails["instance"].GetString());

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        Assert.NotNull(errors);
        Assert.Contains("General.Validation", errors.Keys);
        Assert.Contains(expectedError, errors["General.Validation"]);
    }

    /// <summary>
    /// Stores a playback state for the currently authenticated test user.
    /// </summary>
    /// <param name="volume">The volume to store.</param>
    /// <param name="isMuted">The mute state to store.</param>
    /// <param name="itemsPerPage">The number of items displayed per page to store.</param>
    /// <param name="isPaginationEnabled">Whether pagination is enabled.</param>
    /// <returns>The stored settings.</returns>
    private async Task<UserSettingsEntity> CreateTestUserSettingsAsync(double volume, bool isMuted, int itemsPerPage, bool isPaginationEnabled)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        Guid userId = dbContext.Users.First(user => user.Username == _apiFactory.TestUsername).Id;
        UserSettingsEntity settings = _userSettingsEntityFixture.Create(
            userId: userId,
            volume: volume,
            isMuted: isMuted,
            itemsPerPage: itemsPerPage,
            isPaginationEnabled: isPaginationEnabled);

        dbContext.UserSettings.Add(settings);
        await dbContext.SaveChangesAsync();
        return settings;
    }

    /// <summary>
    /// Reads the stored playback state of the currently authenticated test user.
    /// </summary>
    /// <returns>The stored settings, or <see langword="null"/> when the user has none.</returns>
    private async Task<UserSettingsEntity?> GetCurrentUserSettingsAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        Guid userId = dbContext.Users.First(user => user.Username == _apiFactory.TestUsername).Id;
        return await dbContext.UserSettings.FirstOrDefaultAsync(settings => settings.UserId == userId);
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
