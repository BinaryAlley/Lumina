#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Fixtures.Core.Requests.Authentication;
using Lumina.Contracts.Fixtures.Core.Requests.UsersManagement.Settings;
using Lumina.Contracts.Requests.UsersManagement.Settings;
using Lumina.Contracts.Responses.Authentication;
using Lumina.DataAccess.Core.UoW;
using Lumina.Infrastructure.Core.Security;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.UsersManagement.Settings.UpdateUserSettings;

/// <summary>
/// Contains security tests for the <c>/users/me/settings</c> route.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserSettingsEndpointTests : IClassFixture<LuminaApiFactory>
{
    private readonly HttpClient _client;
    private readonly LuminaApiFactory _apiFactory;
    private readonly PasswordHashService _hashService = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private readonly UserEntityFixture _userEntityFixture = new();
    private readonly LoginRequestFixture _loginRequestFixture = new();
    private readonly UpdateUserSettingsRequestFixture _updateUserSettingsRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserSettingsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public UpdateUserSettingsEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task UpdateUserSettings_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        UpdateUserSettingsRequest request = _updateUserSettingsRequestFixture.Create(
            isPaginationEnabled: true,
            itemsPerPage: 48,
            shouldIgnoreThePrefixForAlphaPicker: false,
            isThemeCachingEnabled: true,
            shouldAggregateMetadataWhenMissing: false
        );

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/users/me/settings", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("/api/v1/users/me/settings", problemDetails["instance"].GetProperty("value").GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
    }

    [Fact]
    public async Task UpdateUserSettings_WhenItemsPerPageIsAtIntMaxValue_ShouldNotCrash()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        await CreateAndAuthenticateUserAsync(client);
        UpdateUserSettingsRequest request = _updateUserSettingsRequestFixture.Create(
            isPaginationEnabled: true,
            itemsPerPage: int.MaxValue,
            shouldIgnoreThePrefixForAlphaPicker: false,
            isThemeCachingEnabled: true,
            shouldAggregateMetadataWhenMissing: false
        );

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync("/api/v1/users/me/settings", request);

        // Assert
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateUserSettings_WhenItemsPerPageIsAtIntMinValue_ShouldReturnValidationErrorWithoutCrashing()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        await CreateAndAuthenticateUserAsync(client);
        UpdateUserSettingsRequest request = _updateUserSettingsRequestFixture.Create(
            isPaginationEnabled: true,
            itemsPerPage: int.MinValue,
            shouldIgnoreThePrefixForAlphaPicker: false,
            isThemeCachingEnabled: true,
            shouldAggregateMetadataWhenMissing: false
        );

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync("/api/v1/users/me/settings", request);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Creates a test user and authenticates it on <paramref name="client"/>.
    /// </summary>
    /// <param name="client">The HTTP client to authenticate.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task CreateAndAuthenticateUserAsync(HttpClient client)
    {
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("X-Forwarded-For", $"192.168.1.{Random.Shared.Next(1, 255)}");

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Guid userId = Guid.NewGuid();
        string username = $"testuser_{Guid.NewGuid()}";
        UserEntity user = _userEntityFixture.Create(id: userId, username: username, password: _hashService.HashString("TestPass123!"));
        user.TotpSecret = null;
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        HttpResponseMessage loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", _loginRequestFixture.Create(username: username, password: "TestPass123!"));
        string content = await loginResponse.Content.ReadAsStringAsync();
        LoginResponse? loginResult = JsonSerializer.Deserialize<LoginResponse>(content, _jsonOptions);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.Token);
    }
}
