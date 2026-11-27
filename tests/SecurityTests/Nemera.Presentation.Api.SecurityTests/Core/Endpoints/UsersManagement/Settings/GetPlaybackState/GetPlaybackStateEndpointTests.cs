#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.UsersManagement.Settings;
using Lumina.DataAccess.Core.UoW;
using Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Settings.GetPlaybackState;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text.Json;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.UsersManagement.Settings.GetPlaybackState;

/// <summary>
/// Contains security tests for the <see cref="GetPlaybackStateEndpoint"/> class.
/// </summary>
/// <remarks>
/// The route has no user controlled string input and no configured rate limiting policy, so injection resistance and rate limiting are not
/// applicable to it. It also never issues a token, so token and claim correctness is not applicable either.
/// </remarks>
[ExcludeFromCodeCoverage]
public class GetPlaybackStateEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private readonly HttpClient _client;
    private readonly LuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly UserSettingsEntityFixture _userSettingsEntityFixture = new();
    private readonly List<string> _seededUsernames = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaybackStateEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetPlaybackStateEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // A unique X-Forwarded-For isolates rate limiting state per test.
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
    }

    [Fact]
    public async Task GetPlaybackState_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/users/me/settings/playback-state");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("/api/v1/users/me/settings/playback-state", problemDetails["instance"].GetProperty("value").GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
    }

    [Fact]
    public async Task GetPlaybackState_WhenAnotherUserHasStoredState_ShouldReturnOnlyTheAuthenticatedUsersState()
    {
        // Arrange
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(_client);
        _seededUsernames.Add(username);
        await SeedOtherUserWithPlaybackStateAsync(volume: 0.1, isMuted: true);

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
    public async Task GetPlaybackState_WhenAuthenticated_ShouldNotLeakSensitiveData()
    {
        // Arrange
        (_, string username) = await _apiFactory.CreateAndAuthenticateUserAsync(_client);
        _seededUsernames.Add(username);

        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/users/me/settings/playback-state");

        // Assert
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina.", content, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Seeds a user distinct from the authenticated one, with a stored playback state.
    /// </summary>
    /// <param name="volume">The volume to store.</param>
    /// <param name="isMuted">The mute state to store.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SeedOtherUserWithPlaybackStateAsync(double volume, bool isMuted)
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        string username = $"otheruser_{Guid.NewGuid()}";
        UserEntity otherUser = _userEntityFixture.Create(username: username, password: "TestPass123!");
        otherUser.TotpSecret = null;
        dbContext.Users.Add(otherUser);
        dbContext.UserSettings.Add(_userSettingsEntityFixture.Create(userId: otherUser.Id, volume: volume, isMuted: isMuted));
        await dbContext.SaveChangesAsync();
        _seededUsernames.Add(username);
    }

    /// <summary>
    /// Disposes the API factory resources and the seeded users.
    /// </summary>
    public void Dispose()
    {
        _client.Dispose();
        foreach (string username in _seededUsernames)
            _apiFactory.RemoveTestUser(username);
    }
}
