#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Requests.UsersManagement.Settings;
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
        UpdateUserSettingsRequest request = new(
            IsPaginationEnabled: false,
            ItemsPerPage: 24,
            IgnoreThePrefixForAlphaPicker: true
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
        Assert.True(storedSettings.IgnoreThePrefixForAlphaPicker);
    }

    [Fact]
    public async Task UpdateUserSettings_WhenItemsPerPageIsNotPositive_ShouldReturnValidationError()
    {
        // Arrange
        UpdateUserSettingsRequest request = new(
            IsPaginationEnabled: true,
            ItemsPerPage: 0,
            IgnoreThePrefixForAlphaPicker: false
        );

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/users/me/settings", request);

        // Assert
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
        Assert.Contains("ItemsPerPageMustBeGreaterThanZero", errors["General.Validation"]);
    }

    [Fact]
    public async Task UpdateUserSettings_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient unauthenticatedClient = _apiFactory.CreateClient();
        UpdateUserSettingsRequest request = new(
            IsPaginationEnabled: true,
            ItemsPerPage: 48,
            IgnoreThePrefixForAlphaPicker: false
        );

        // Act
        HttpResponseMessage response = await unauthenticatedClient.PutAsJsonAsync("/api/v1/users/me/settings", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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
