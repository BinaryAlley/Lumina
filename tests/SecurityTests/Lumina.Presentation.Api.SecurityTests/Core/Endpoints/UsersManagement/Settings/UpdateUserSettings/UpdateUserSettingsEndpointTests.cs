#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Requests.UsersManagement.Settings;
using Lumina.Contracts.Responses.Authentication;
using Lumina.DataAccess.Core.UoW;
using Lumina.Infrastructure.Core.Security;
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
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
        UpdateUserSettingsRequest request = new(
            IsPaginationEnabled: true,
            ItemsPerPage: 48,
            IgnoreThePrefixForAlphaPicker: false
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
        UpdateUserSettingsRequest request = new(
            IsPaginationEnabled: true,
            ItemsPerPage: int.MaxValue,
            IgnoreThePrefixForAlphaPicker: false
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
        UpdateUserSettingsRequest request = new(
            IsPaginationEnabled: true,
            ItemsPerPage: int.MinValue,
            IgnoreThePrefixForAlphaPicker: false
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
        dbContext.Users.Add(new UserEntity
        {
            Id = userId,
            Username = username,
            Password = _hashService.HashString("TestPass123!"),
            Libraries = [],
            UserPermissions = [],
            UserRole = null,
            CreatedBy = userId,
            CreatedOnUtc = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        HttpResponseMessage loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(username, "TestPass123!"));
        string content = await loginResponse.Content.ReadAsStringAsync();
        LoginResponse? loginResult = JsonSerializer.Deserialize<LoginResponse>(content, _jsonOptions);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.Token);
    }
}
