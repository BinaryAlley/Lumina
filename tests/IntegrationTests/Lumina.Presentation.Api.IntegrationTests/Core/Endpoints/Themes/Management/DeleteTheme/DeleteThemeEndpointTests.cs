#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.Infrastructure.Themes;
using Lumina.Contracts.Responses.Themes;
using Lumina.DataAccess.Core.UoW;
using Lumina.Presentation.Api.Core.Endpoints.Themes.Management.DeleteTheme;
using Lumina.Presentation.Api.Fixtures.Core.Endpoints.Themes;
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
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Themes.Management.DeleteTheme;

/// <summary>
/// Contains integration tests for the <see cref="DeleteThemeEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteThemeEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
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
    /// Initializes a new instance of the <see cref="DeleteThemeEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public DeleteThemeEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task DeleteTheme_WhenCalledWithExistingTheme_ShouldDeleteTheme()
    {
        // Arrange
        string themeId = await InstallThemeAsync();

        // Act
        HttpResponseMessage response = await _client.DeleteAsync($"/api/v1/themes/{themeId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // the theme row is removed from the database
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.Null(await dbContext.Themes.FirstOrDefaultAsync(theme => theme.ThemeId == themeId));

        // the theme no longer appears in GET /themes
        HttpResponseMessage listResponse = await _client.GetAsync("/api/v1/themes");
        List<ThemeResponse>? themes = await listResponse.Content.ReadFromJsonAsync<List<ThemeResponse>>(_jsonOptions);
        Assert.NotNull(themes);
        Assert.DoesNotContain(themes!, theme => theme.ThemeId == themeId);
    }

    [Fact]
    public async Task DeleteTheme_WhenThemeDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        string themeId = "nonexistent-theme";

        // Act
        HttpResponseMessage response = await _client.DeleteAsync($"/api/v1/themes/{themeId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problemDetails["type"].GetString());
        Assert.Equal("General.NotFound", problemDetails["title"].GetString());
        Assert.Equal("ThemeNotFound", problemDetails["detail"].GetString());
        Assert.Equal($"/api/v1/themes/{themeId}", problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);
    }

    [Fact]
    public async Task DeleteTheme_WhenTryingToDeleteTheLastBundledTheme_ShouldReturnForbiddenResult()
    {
        // Arrange
        await ThemeTestHelpers.WaitForBundledThemeAsync(_apiFactory);

        // deleting a bundled theme that is not the last remaining one is allowed, so only the last one is protected
        HttpResponseMessage firstDeletionResponse = await _client.DeleteAsync("/api/v1/themes/editorial-paper");
        Assert.Equal(HttpStatusCode.NoContent, firstDeletionResponse.StatusCode);

        // Act: the last remaining bundled theme cannot be deleted
        HttpResponseMessage response = await _client.DeleteAsync("/api/v1/themes/lumina-default");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Forbidden", problemDetails["title"].GetString());
        Assert.Equal("LastBundledThemeCannotBeDeleted", problemDetails["detail"].GetString());
        Assert.Equal("/api/v1/themes/lumina-default", problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);

        // restore the soft-deleted bundled theme, so the following tests start from a clean state
        HttpResponseMessage restoreResponse = await _client.PostAsJsonAsync("/api/v1/themes/editorial-paper/restore", new { });
        Assert.Equal(HttpStatusCode.NoContent, restoreResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteTheme_WhenCalledWithNonAdminAccount_ShouldReturnForbiddenResult()
    {
        // Arrange
        HttpClient nonAdminClient = await _apiFactory.CreateAuthenticatedClientAsync();

        // Act
        HttpResponseMessage response = await nonAdminClient.DeleteAsync("/api/v1/themes/editorial-paper");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());
        Assert.Equal("/api/v1/themes/editorial-paper", problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);
    }

    [Fact]
    public async Task DeleteTheme_WhenCalledWithoutAuthentication_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient anonymousClient = _apiFactory.CreateClient();

        // Act
        HttpResponseMessage response = await anonymousClient.DeleteAsync("/api/v1/themes/editorial-paper");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Installs a test theme through the API and returns its manifest id.
    /// </summary>
    /// <returns>The manifest id of the installed theme.</returns>
    private async Task<string> InstallThemeAsync()
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
    /// Disposes API factory resources, removing any test theme left installed by a failed deletion and clearing the authorization seed data of the admin test user.
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
