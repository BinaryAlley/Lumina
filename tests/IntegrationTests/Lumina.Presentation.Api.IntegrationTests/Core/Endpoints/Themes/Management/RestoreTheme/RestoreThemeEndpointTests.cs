#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.Infrastructure.Themes;
using Lumina.DataAccess.Core.UoW;
using Lumina.Presentation.Api.Core.Endpoints.Themes.Management.RestoreTheme;
using Lumina.Presentation.Api.Fixtures.Core.Endpoints.Themes;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Themes.Management.RestoreTheme;

/// <summary>
/// Contains integration tests for the <see cref="RestoreThemeEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreThemeEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly TestThemeArchiveFixture _themeArchiveFixture = new();
    private readonly List<string> _installedThemeIds = [];
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreThemeEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public RestoreThemeEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
        _apiFactory = apiFactory;
    }

    /// <summary>
    /// Initializes an authenticated admin API client.
    /// </summary>
    public async Task InitializeAsync()
    {
        _client = await _apiFactory.CreateAuthenticatedAdminClientAsync();
    }

    [Fact]
    public async Task RestoreTheme_WhenCalledWithDeletedBundledTheme_ShouldRestoreTheme()
    {
        // Arrange
        await ThemeTestHelpers.WaitForBundledThemeAsync(_apiFactory);
        using (IServiceScope scope = _apiFactory.Services.CreateScope())
        {
            LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
            ThemeEntity? theme = await dbContext.Themes.FirstOrDefaultAsync(candidate => candidate.ThemeId == "editorial-paper");
            Assert.NotNull(theme);
            theme!.IsDeleted = true;
            theme.IsCurrent = null;
            await dbContext.SaveChangesAsync();
        }

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/themes/editorial-paper/restore", new { });

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // the theme is available again and appears in the list
        using IServiceScope verificationScope = _apiFactory.Services.CreateScope();
        LuminaDbContext verificationContext = verificationScope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        ThemeEntity? restoredTheme = await verificationContext.Themes.FirstOrDefaultAsync(candidate => candidate.ThemeId == "editorial-paper");
        Assert.NotNull(restoredTheme);
        Assert.False(restoredTheme!.IsDeleted);

        HttpResponseMessage listResponse = await _client.GetAsync("/api/v1/themes");
        List<Lumina.Contracts.Responses.Themes.ThemeResponse>? themes = await listResponse.Content.ReadFromJsonAsync<List<Lumina.Contracts.Responses.Themes.ThemeResponse>>(_jsonOptions);
        Assert.NotNull(themes);
        Assert.Contains(themes!, theme => theme.ThemeId == "editorial-paper" && !theme.IsDeleted);
    }

    [Fact]
    public async Task RestoreTheme_WhenThemeDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        string themeId = "nonexistent-theme";

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"/api/v1/themes/{themeId}/restore", new { });

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problemDetails["type"].GetString());
        Assert.Equal("General.NotFound", problemDetails["title"].GetString());
        Assert.Equal("ThemeNotFound", problemDetails["detail"].GetString());
        Assert.Equal($"/api/v1/themes/{themeId}/restore", problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);
    }

    [Fact]
    public async Task RestoreTheme_WhenThemeIsNotDeleted_ShouldReturnForbiddenResult()
    {
        // Arrange
        string themeId = await InstallUserThemeAsync();

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"/api/v1/themes/{themeId}/restore", new { });

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Forbidden", problemDetails["title"].GetString());
        Assert.Equal("ThemeCannotBeRestored", problemDetails["detail"].GetString());
        Assert.Equal($"/api/v1/themes/{themeId}/restore", problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);
    }

    [Fact]
    public async Task RestoreTheme_WhenCalledWithNonAdminAccount_ShouldReturnForbiddenResult()
    {
        // Arrange
        HttpClient nonAdminClient = await _apiFactory.CreateAuthenticatedClientAsync();

        // Act
        HttpResponseMessage response = await nonAdminClient.PostAsJsonAsync("/api/v1/themes/editorial-paper/restore", new { });

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());
        Assert.Equal("/api/v1/themes/editorial-paper/restore", problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);
    }

    [Fact]
    public async Task RestoreTheme_WhenCalledWithoutAuthentication_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient anonymousClient = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await anonymousClient.PostAsJsonAsync("/api/v1/themes/editorial-paper/restore", new { });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Installs a test theme through the API and returns its manifest id.
    /// </summary>
    /// <returns>The manifest id of the installed theme.</returns>
    private async Task<string> InstallUserThemeAsync()
    {
        string themeId = $"test-theme-{Guid.NewGuid():N}";
        byte[] archiveBytes = _themeArchiveFixture.Create(themeId);

        using MultipartFormDataContent multipartContent = [];
        using ByteArrayContent fileContent = new(archiveBytes);
        multipartContent.Add(fileContent, "file", "theme.zip");

        HttpResponseMessage response = await _client.PostAsync("/api/v1/themes", multipartContent);
        response.EnsureSuccessStatusCode();

        _installedThemeIds.Add(themeId);
        return themeId;
    }

    /// <summary>
    /// Removes any test theme installed by a failed test and clears the authorization seed data of the admin test user.
    /// </summary>
    public async Task DisposeAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        foreach (string themeId in _installedThemeIds)
        {
            ThemeEntity? theme = await dbContext.Themes.FirstOrDefaultAsync(t => t.ThemeId == themeId);
            if (theme is not null)
                dbContext.Themes.Remove(theme);
        }

        await dbContext.SaveChangesAsync();

        IThemeService themeService = scope.ServiceProvider.GetRequiredService<IThemeService>();
        foreach (string themeId in _installedThemeIds)
            await themeService.DeleteAsync(themeId, CancellationToken.None);

        // the admin seeding performed by the factory is not idempotent, so the seed data must be cleared between tests
        await dbContext.Set<RolePermissionEntity>().ExecuteDeleteAsync();
        await dbContext.Set<UserPermissionEntity>().ExecuteDeleteAsync();
        await dbContext.Set<UserRoleEntity>().ExecuteDeleteAsync();
        await dbContext.Set<UserEntity>().ExecuteDeleteAsync();
        await dbContext.Set<RoleEntity>().ExecuteDeleteAsync();
        await dbContext.Set<PermissionEntity>().ExecuteDeleteAsync();

        await _apiFactory.RemoveTestUserAsync();
    }
}
