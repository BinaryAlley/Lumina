#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Fixtures.Core.Requests.UsersManagement.Settings;
using Lumina.Contracts.Requests.UsersManagement.Settings;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.Playback;
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
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.UsersManagement.Settings.UpdateUserSettings;

/// <summary>
/// Contains integration tests for the <see cref="UpdateUserSettingsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserSettingsEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly UpdateUserSettingsRequestFixture _updateUserSettingsRequestFixture = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserSettingsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public UpdateUserSettingsEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task UpdateUserSettings_WhenCalledWithValidRequest_ShouldUpdateSettings()
    {
        // Arrange
        UpdateUserSettingsRequest request = _updateUserSettingsRequestFixture.Create(
            isPaginationEnabled: false,
            itemsPerPage: 24,
            shouldIgnoreThePrefixForAlphaPicker: true,
            isThemeCachingEnabled: false,
            shouldAggregateMetadataWhenMissing: false,
            vuMeterOrientation: VuMeterOrientation.Horizontal,
            vuMeterSegmentCount: 20,
            vuMeterSegmentGap: 4,
            vuMeterMediumThreshold: 0.35,
            vuMeterHighThreshold: 0.9,
            vuMeterShowPeak: false,
            vuMeterPeakHoldDurationInMilliseconds: 1200,
            vuMeterReleasePerSecond: 7.5,
            vuMeterPeakReleasePerSecond: 0.25
        );

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/users/me/settings", request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid userId = dbContext.Users.First(user => user.Username == _apiFactory.TestUsername).Id;
        UserSettingsEntity? storedSettings = await dbContext.UserSettings.FirstOrDefaultAsync(settings => settings.UserId == userId);
        Assert.NotNull(storedSettings);
        Assert.False(storedSettings.IsPaginationEnabled);
        Assert.Equal(24, storedSettings.ItemsPerPage);
        Assert.True(storedSettings.ShouldIgnoreThePrefixForAlphaPicker);
        Assert.False(storedSettings.IsThemeCachingEnabled);
        Assert.Equal(VuMeterOrientation.Horizontal, storedSettings.VuMeterOrientation);
        Assert.Equal(20, storedSettings.VuMeterSegmentCount);
        Assert.Equal(4, storedSettings.VuMeterSegmentGap);
        Assert.Equal(0.35, storedSettings.VuMeterMediumThreshold);
        Assert.Equal(0.9, storedSettings.VuMeterHighThreshold);
        Assert.False(storedSettings.VuMeterShowPeak);
        Assert.Equal(1200, storedSettings.VuMeterPeakHoldDurationInMilliseconds);
        Assert.Equal(7.5, storedSettings.VuMeterReleasePerSecond);
        Assert.Equal(0.25, storedSettings.VuMeterPeakReleasePerSecond);
    }

    [Fact]
    public async Task UpdateUserSettings_WhenItemsPerPageIsNotPositive_ShouldReturnValidationError()
    {
        // Arrange
        UpdateUserSettingsRequest request = _updateUserSettingsRequestFixture.Create(
            isPaginationEnabled: true,
            itemsPerPage: 0,
            shouldIgnoreThePrefixForAlphaPicker: false,
            isThemeCachingEnabled: true,
            shouldAggregateMetadataWhenMissing: false
        );

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/users/me/settings", request);

        // Assert
        await AssertValidationErrorAsync(response, "ItemsPerPageMustBeGreaterThanZero");
    }

    [Fact]
    public async Task UpdateUserSettings_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();
        UpdateUserSettingsRequest request = _updateUserSettingsRequestFixture.Create(
            isPaginationEnabled: true,
            itemsPerPage: 48,
            shouldIgnoreThePrefixForAlphaPicker: false,
            isThemeCachingEnabled: true,
            shouldAggregateMetadataWhenMissing: false
        );

        // Act
        HttpResponseMessage response = await unauthenticatedClient.PutAsJsonAsync("/api/v1/users/me/settings", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUserSettings_WhenVuMeterOrientationIsNull_ShouldReturnValidationError()
    {
        // Arrange
        UpdateUserSettingsRequest request = _updateUserSettingsRequestFixture.Create(includeVuMeterOrientation: false);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/users/me/settings", request);

        // Assert
        await AssertValidationErrorAsync(response, "VuMeterOrientationCannotBeNull");
    }

    [Fact]
    public async Task UpdateUserSettings_WhenVuMeterSegmentCountIsNull_ShouldReturnValidationError()
    {
        // Arrange
        UpdateUserSettingsRequest request = _updateUserSettingsRequestFixture.Create(includeVuMeterSegmentCount: false);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/users/me/settings", request);

        // Assert
        await AssertValidationErrorAsync(response, "VuMeterSegmentCountCannotBeNull");
    }

    [Fact]
    public async Task UpdateUserSettings_WhenVuMeterSegmentGapIsNull_ShouldReturnValidationError()
    {
        // Arrange
        UpdateUserSettingsRequest request = _updateUserSettingsRequestFixture.Create(includeVuMeterSegmentGap: false);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/users/me/settings", request);

        // Assert
        await AssertValidationErrorAsync(response, "VuMeterSegmentGapCannotBeNull");
    }

    [Fact]
    public async Task UpdateUserSettings_WhenVuMeterMediumThresholdIsNull_ShouldReturnValidationError()
    {
        // Arrange
        UpdateUserSettingsRequest request = _updateUserSettingsRequestFixture.Create(includeVuMeterMediumThreshold: false);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/users/me/settings", request);

        // Assert
        await AssertValidationErrorAsync(response, "VuMeterMediumThresholdCannotBeNull");
    }

    [Fact]
    public async Task UpdateUserSettings_WhenVuMeterHighThresholdIsNull_ShouldReturnValidationError()
    {
        // Arrange
        UpdateUserSettingsRequest request = _updateUserSettingsRequestFixture.Create(includeVuMeterHighThreshold: false);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/users/me/settings", request);

        // Assert
        await AssertValidationErrorAsync(response, "VuMeterHighThresholdCannotBeNull");
    }

    [Fact]
    public async Task UpdateUserSettings_WhenVuMeterShowPeakIsNull_ShouldReturnValidationError()
    {
        // Arrange
        UpdateUserSettingsRequest request = _updateUserSettingsRequestFixture.Create(includeVuMeterShowPeak: false);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/users/me/settings", request);

        // Assert
        await AssertValidationErrorAsync(response, "VuMeterShowPeakCannotBeNull");
    }

    [Fact]
    public async Task UpdateUserSettings_WhenVuMeterPeakHoldDurationIsNull_ShouldReturnValidationError()
    {
        // Arrange
        UpdateUserSettingsRequest request = _updateUserSettingsRequestFixture.Create(includeVuMeterPeakHoldDurationInMilliseconds: false);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/users/me/settings", request);

        // Assert
        await AssertValidationErrorAsync(response, "VuMeterPeakHoldDurationCannotBeNull");
    }

    [Fact]
    public async Task UpdateUserSettings_WhenVuMeterReleasePerSecondIsNull_ShouldReturnValidationError()
    {
        // Arrange
        UpdateUserSettingsRequest request = _updateUserSettingsRequestFixture.Create(includeVuMeterReleasePerSecond: false);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/users/me/settings", request);

        // Assert
        await AssertValidationErrorAsync(response, "VuMeterReleasePerSecondCannotBeNull");
    }

    [Fact]
    public async Task UpdateUserSettings_WhenVuMeterPeakReleasePerSecondIsNull_ShouldReturnValidationError()
    {
        // Arrange
        UpdateUserSettingsRequest request = _updateUserSettingsRequestFixture.Create(includeVuMeterPeakReleasePerSecond: false);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/users/me/settings", request);

        // Assert
        await AssertValidationErrorAsync(response, "VuMeterPeakReleasePerSecondCannotBeNull");
    }

    [Fact]
    public async Task UpdateUserSettings_WhenVuMeterOrientationIsNotDefined_ShouldReturnValidationError()
    {
        // Arrange
        UpdateUserSettingsRequest request = _updateUserSettingsRequestFixture.Create(vuMeterOrientation: (VuMeterOrientation)999);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/users/me/settings", request);

        // Assert
        await AssertValidationErrorAsync(response, "InvalidVuMeterOrientation");
    }

    [Theory]
    [InlineData(0)] // below the minimum
    [InlineData(129)] // above the maximum
    public async Task UpdateUserSettings_WhenVuMeterSegmentCountIsOutOfRange_ShouldReturnValidationError(int vuMeterSegmentCount)
    {
        // Arrange
        UpdateUserSettingsRequest request = _updateUserSettingsRequestFixture.Create(vuMeterSegmentCount: vuMeterSegmentCount);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/users/me/settings", request);

        // Assert
        await AssertValidationErrorAsync(response, "VuMeterSegmentCountMustBeBetweenOneAndOneHundredTwentyEight");
    }

    [Theory]
    [InlineData(-1)] // below the minimum
    [InlineData(33)] // above the maximum
    public async Task UpdateUserSettings_WhenVuMeterSegmentGapIsOutOfRange_ShouldReturnValidationError(int vuMeterSegmentGap)
    {
        // Arrange
        UpdateUserSettingsRequest request = _updateUserSettingsRequestFixture.Create(vuMeterSegmentGap: vuMeterSegmentGap);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/users/me/settings", request);

        // Assert
        await AssertValidationErrorAsync(response, "VuMeterSegmentGapMustBeBetweenZeroAndThirtyTwo");
    }

    [Theory]
    [InlineData(-0.01)] // just below the minimum
    [InlineData(1.01)] // just above the maximum
    public async Task UpdateUserSettings_WhenVuMeterMediumThresholdIsOutOfRange_ShouldReturnValidationError(double vuMeterMediumThreshold)
    {
        // Arrange
        UpdateUserSettingsRequest request = _updateUserSettingsRequestFixture.Create(
            vuMeterMediumThreshold: vuMeterMediumThreshold,
            vuMeterHighThreshold: 1.0);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/users/me/settings", request);

        // Assert
        await AssertValidationErrorAsync(response, "VuMeterMediumThresholdMustBeBetweenZeroAndOne");
    }

    [Theory]
    [InlineData(-0.01)] // just below the minimum
    [InlineData(1.01)] // just above the maximum
    public async Task UpdateUserSettings_WhenVuMeterHighThresholdIsOutOfRange_ShouldReturnValidationError(double vuMeterHighThreshold)
    {
        // Arrange
        UpdateUserSettingsRequest request = _updateUserSettingsRequestFixture.Create(
            vuMeterMediumThreshold: 0.0,
            vuMeterHighThreshold: vuMeterHighThreshold);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/users/me/settings", request);

        // Assert
        await AssertValidationErrorAsync(response, "VuMeterHighThresholdMustBeBetweenZeroAndOne");
    }

    [Fact]
    public async Task UpdateUserSettings_WhenVuMeterHighThresholdIsLowerThanMediumThreshold_ShouldReturnValidationError()
    {
        // Arrange
        UpdateUserSettingsRequest request = _updateUserSettingsRequestFixture.Create(
            vuMeterMediumThreshold: 0.9,
            vuMeterHighThreshold: 0.1);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/users/me/settings", request);

        // Assert
        await AssertValidationErrorAsync(response, "VuMeterHighThresholdMustBeGreaterThanOrEqualToMediumThreshold");
    }

    [Theory]
    [InlineData(0.5, 0.5)] // the thresholds are equal
    [InlineData(0.4, 0.9)] // the high threshold is greater than the medium one
    public async Task UpdateUserSettings_WhenVuMeterHighThresholdIsGreaterThanOrEqualToMediumThreshold_ShouldUpdateSettings(
        double vuMeterMediumThreshold,
        double vuMeterHighThreshold)
    {
        // Arrange
        UpdateUserSettingsRequest request = _updateUserSettingsRequestFixture.Create(
            vuMeterMediumThreshold: vuMeterMediumThreshold,
            vuMeterHighThreshold: vuMeterHighThreshold);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/users/me/settings", request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData(-1)] // below the minimum
    [InlineData(10001)] // above the maximum
    public async Task UpdateUserSettings_WhenVuMeterPeakHoldDurationIsOutOfRange_ShouldReturnValidationError(int vuMeterPeakHoldDurationInMilliseconds)
    {
        // Arrange
        UpdateUserSettingsRequest request = _updateUserSettingsRequestFixture.Create(
            vuMeterPeakHoldDurationInMilliseconds: vuMeterPeakHoldDurationInMilliseconds);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/users/me/settings", request);

        // Assert
        await AssertValidationErrorAsync(response, "VuMeterPeakHoldDurationMustBeBetweenZeroAndTenThousandMilliseconds");
    }

    [Theory]
    [InlineData(-0.01)] // just below the minimum
    [InlineData(100.01)] // just above the maximum
    public async Task UpdateUserSettings_WhenVuMeterReleasePerSecondIsOutOfRange_ShouldReturnValidationError(double vuMeterReleasePerSecond)
    {
        // Arrange
        UpdateUserSettingsRequest request = _updateUserSettingsRequestFixture.Create(vuMeterReleasePerSecond: vuMeterReleasePerSecond);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/users/me/settings", request);

        // Assert
        await AssertValidationErrorAsync(response, "VuMeterReleasePerSecondMustBeBetweenZeroAndOneHundred");
    }

    [Theory]
    [InlineData(-0.01)] // just below the minimum
    [InlineData(100.01)] // just above the maximum
    public async Task UpdateUserSettings_WhenVuMeterPeakReleasePerSecondIsOutOfRange_ShouldReturnValidationError(double vuMeterPeakReleasePerSecond)
    {
        // Arrange
        UpdateUserSettingsRequest request = _updateUserSettingsRequestFixture.Create(vuMeterPeakReleasePerSecond: vuMeterPeakReleasePerSecond);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/users/me/settings", request);

        // Assert
        await AssertValidationErrorAsync(response, "VuMeterPeakReleasePerSecondMustBeBetweenZeroAndOneHundred");
    }

    [Fact]
    public async Task UpdateUserSettings_WhenAllVuMeterValuesAreAtTheirMinimums_ShouldUpdateSettings()
    {
        // Arrange
        UpdateUserSettingsRequest request = _updateUserSettingsRequestFixture.Create(
            vuMeterOrientation: VuMeterOrientation.Vertical,
            vuMeterSegmentCount: 1,
            vuMeterSegmentGap: 0,
            vuMeterMediumThreshold: 0.0,
            vuMeterHighThreshold: 0.0,
            vuMeterShowPeak: false,
            vuMeterPeakHoldDurationInMilliseconds: 0,
            vuMeterReleasePerSecond: 0.0,
            vuMeterPeakReleasePerSecond: 0.0);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/users/me/settings", request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUserSettings_WhenAllVuMeterValuesAreAtTheirMaximums_ShouldUpdateSettings()
    {
        // Arrange
        UpdateUserSettingsRequest request = _updateUserSettingsRequestFixture.Create(
            vuMeterOrientation: VuMeterOrientation.Horizontal,
            vuMeterSegmentCount: 128,
            vuMeterSegmentGap: 32,
            vuMeterMediumThreshold: 1.0,
            vuMeterHighThreshold: 1.0,
            vuMeterShowPeak: true,
            vuMeterPeakHoldDurationInMilliseconds: 10000,
            vuMeterReleasePerSecond: 100.0,
            vuMeterPeakReleasePerSecond: 100.0);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/users/me/settings", request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
        Assert.Equal("/api/v1/users/me/settings", problemDetails["instance"].GetString());

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        Assert.NotNull(errors);
        Assert.Contains("General.Validation", errors.Keys);
        Assert.Contains(expectedError, errors["General.Validation"]);
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
