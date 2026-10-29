#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Responses.UsersManagement.Settings;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
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

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.UsersManagement.Settings.GetUserSettings;

/// <summary>
/// Contains integration tests for the <see cref="GetUserSettingsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserSettingsEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly UserSettingsEntityFixture _userSettingsEntityFixture = new();
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
        Assert.Equal(storedSettings.ShouldIgnoreThePrefixForAlphaPicker, result.ShouldIgnoreThePrefixForAlphaPicker);
        Assert.Equal(VuMeterOrientation.Horizontal, result.VuMeterOrientation);
        Assert.Equal(20, result.VuMeterSegmentCount);
        Assert.Equal(4, result.VuMeterSegmentGap);
        Assert.Equal(0.35, result.VuMeterMediumThreshold);
        Assert.Equal(0.9, result.VuMeterHighThreshold);
        Assert.False(result.VuMeterShowPeak);
        Assert.Equal(1200, result.VuMeterPeakHoldDurationInMilliseconds);
        Assert.Equal(7.5, result.VuMeterReleasePerSecond);
        Assert.Equal(0.25, result.VuMeterPeakReleasePerSecond);
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
        Assert.False(result.ShouldIgnoreThePrefixForAlphaPicker);
        Assert.Equal(VuMeterOrientation.Vertical, result.VuMeterOrientation);
        Assert.Equal(12, result.VuMeterSegmentCount);
        Assert.Equal(1, result.VuMeterSegmentGap);
        Assert.Equal(0.6, result.VuMeterMediumThreshold);
        Assert.Equal(0.8, result.VuMeterHighThreshold);
        Assert.True(result.VuMeterShowPeak);
        Assert.Equal(650, result.VuMeterPeakHoldDurationInMilliseconds);
        Assert.Equal(3.0, result.VuMeterReleasePerSecond);
        Assert.Equal(0.65, result.VuMeterPeakReleasePerSecond);
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
        UserSettingsEntity settings = _userSettingsEntityFixture.Create(
            userId: userId,
            isPaginationEnabled: false,
            itemsPerPage: 24,
            shouldIgnoreThePrefixForAlphaPicker: true,
            vuMeterOrientation: VuMeterOrientation.Horizontal,
            vuMeterSegmentCount: 20,
            vuMeterSegmentGap: 4,
            vuMeterMediumThreshold: 0.35,
            vuMeterHighThreshold: 0.9,
            vuMeterShowPeak: false,
            vuMeterPeakHoldDurationInMilliseconds: 1200,
            vuMeterReleasePerSecond: 7.5,
            vuMeterPeakReleasePerSecond: 0.25);

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
