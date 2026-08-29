#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.Infrastructure.Themes;
using Lumina.Contracts.Requests.Themes;
using Lumina.Contracts.Responses.Themes;
using Lumina.DataAccess.Core.UoW;
using Lumina.Presentation.Api.Core.Endpoints.Themes.Management.SetCurrentTheme;
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

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Themes.Management.SetCurrentTheme;

/// <summary>
/// Contains integration tests for the <see cref="SetCurrentThemeEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetCurrentThemeEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
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
    /// Initializes a new instance of the <see cref="SetCurrentThemeEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public SetCurrentThemeEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task SetCurrentTheme_WhenCalledWithExistingTheme_ShouldSetCurrentTheme()
    {
        // Arrange
        await ThemeTestHelpers.WaitForBundledThemeAsync(_apiFactory);
        string themeId = await InstallThemeAsync();
        SetCurrentThemeRequest request = new(ThemeId: themeId);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/themes/current", request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ThemeResponse? result = await response.Content.ReadFromJsonAsync<ThemeResponse>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Equal(themeId, result!.ThemeId);
        Assert.True(result.IsCurrent);

        // the newly active theme is returned by GET /themes/current
        HttpResponseMessage currentResponse = await _client.GetAsync("/api/v1/themes/current");
        ThemeResponse? current = await currentResponse.Content.ReadFromJsonAsync<ThemeResponse>(_jsonOptions);
        Assert.NotNull(current);
        Assert.Equal(themeId, current!.ThemeId);
        Assert.True(current.IsCurrent);
    }

    [Fact]
    public async Task SetCurrentTheme_WhenThemeDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        string themeId = "nonexistent-theme";
        SetCurrentThemeRequest request = new(ThemeId: themeId);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/themes/current", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problemDetails["type"].GetString());
        Assert.Equal("General.NotFound", problemDetails["title"].GetString());
        Assert.Equal("ThemeNotFound", problemDetails["detail"].GetString());
        Assert.Equal("/api/v1/themes/current", problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);
    }

    [Fact]
    public async Task SetCurrentTheme_WhenThemeIdIsEmpty_ShouldReturnValidationProblem()
    {
        // Arrange
        SetCurrentThemeRequest request = new(ThemeId: string.Empty);

        // Act
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/v1/themes/current", request);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableContent, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", problemDetails["type"].GetString());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        Assert.NotNull(errors);
        Assert.Contains("General.Validation", errors.Keys);
        Assert.Contains("ThemeIdCannotBeEmpty", errors["General.Validation"]);
    }

    [Fact]
    public async Task SetCurrentTheme_WhenCalledWithNonAdminAccount_ShouldReturnForbiddenResult()
    {
        // Arrange
        HttpClient nonAdminClient = await _apiFactory.CreateAuthenticatedClientAsync();
        SetCurrentThemeRequest request = new(ThemeId: "editorial-paper");

        // Act
        HttpResponseMessage response = await nonAdminClient.PutAsJsonAsync("/api/v1/themes/current", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());
        Assert.Equal("/api/v1/themes/current", problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);
    }

    [Fact]
    public async Task SetCurrentTheme_WhenCalledWithoutAuthentication_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient anonymousClient = _apiFactory.CreateClient();
        SetCurrentThemeRequest request = new(ThemeId: "editorial-paper");

        // Act
        HttpResponseMessage response = await anonymousClient.PutAsJsonAsync("/api/v1/themes/current", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Installs a test theme through the API and returns its manifest id.
    /// </summary>
    /// <param name="themeId">Optional manifest id of the theme to install.</param>
    /// <returns>The manifest id of the installed theme.</returns>
    private async Task<string> InstallThemeAsync(string? themeId = null)
    {
        string resolvedThemeId = themeId ?? $"test-theme-{Guid.NewGuid():N}";
        byte[] archiveBytes = _themeArchiveFixture.Create(resolvedThemeId);

        using MultipartFormDataContent multipartContent = [];
        using ByteArrayContent fileContent = new(archiveBytes);
        multipartContent.Add(fileContent, "file", "theme.zip");

        HttpResponseMessage response = await _client.PostAsync("/api/v1/themes", multipartContent);
        response.EnsureSuccessStatusCode();

        _installedThemeIds.Add(resolvedThemeId);
        return resolvedThemeId;
    }

    /// <summary>
    /// Disposes API factory resources, removing the installed test themes, restoring the bundled theme as the active one, and clearing the authorization seed data of the admin test user.
    /// </summary>
    public async Task DisposeAsync()
    {
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

        foreach (string themeId in _installedThemeIds)
        {
            ThemeEntity? theme = await dbContext.Themes.FirstOrDefaultAsync(t => t.ThemeId == themeId);
            if (theme is not null)
            {
                theme.IsCurrent = null;
                dbContext.Themes.Remove(theme);
            }
        }

        // restore the bundled default theme as the active one, so the following tests start from a clean state
        ThemeEntity? bundledTheme = await dbContext.Themes.FirstOrDefaultAsync(t => t.ThemeId == "lumina-default");
        if (bundledTheme is not null)
            bundledTheme.IsCurrent = true;

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
