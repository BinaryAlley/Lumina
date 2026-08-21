#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.Infrastructure.Themes;
using Lumina.Contracts.Responses.Themes;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.Themes;
using Lumina.Presentation.Api.Core.Endpoints.Themes.Management.InstallTheme;
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
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Themes.Management.InstallTheme;

/// <summary>
/// Contains integration tests for the <see cref="InstallThemeEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class InstallThemeEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
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
    /// Initializes a new instance of the <see cref="InstallThemeEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public InstallThemeEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task InstallTheme_WhenCalledWithValidArchive_ShouldInstallTheme()
    {
        // Arrange
        string themeId = $"test-theme-{Guid.NewGuid():N}";
        byte[] archiveBytes = _themeArchiveFixture.Create(themeId);
        using MultipartFormDataContent multipartContent = [];
        using ByteArrayContent fileContent = new(archiveBytes);
        multipartContent.Add(fileContent, "file", "theme.zip");

        // Act
        HttpResponseMessage response = await _client.PostAsync("/api/v1/themes", multipartContent);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ThemeResponse? result = await response.Content.ReadFromJsonAsync<ThemeResponse>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Equal(themeId, result!.ThemeId);
        Assert.Equal("Test Theme", result.Name);
        Assert.Equal(ThemeInstallSource.Uploaded, result.InstallSource);
        Assert.Null(result.IsCurrent);

        // the installed theme is listed by GET /themes
        HttpResponseMessage listResponse = await _client.GetAsync("/api/v1/themes");
        List<ThemeResponse>? themes = await listResponse.Content.ReadFromJsonAsync<List<ThemeResponse>>(_jsonOptions);
        Assert.NotNull(themes);
        Assert.Contains(themes!, theme => theme.ThemeId == themeId);

        // the installed theme is persisted in the database
        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();
        Assert.NotNull(await dbContext.Themes.FirstOrDefaultAsync(theme => theme.ThemeId == themeId));

        _installedThemeIds.Add(themeId);
    }

    [Fact]
    public async Task InstallTheme_WhenCalledWithInvalidArchive_ShouldReturnProblemResult()
    {
        // Arrange
        byte[] invalidArchive = Encoding.UTF8.GetBytes("this is not a valid zip archive");
        using MultipartFormDataContent multipartContent = [];
        using ByteArrayContent fileContent = new(invalidArchive);
        multipartContent.Add(fileContent, "file", "theme.zip");

        // Act
        HttpResponseMessage response = await _client.PostAsync("/api/v1/themes", multipartContent);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Failure", problemDetails["title"].GetString());
        Assert.Equal("ThemeArchiveNotReadable", problemDetails["detail"].GetString());
        Assert.Equal("/api/v1/themes", problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);
    }

    [Fact]
    public async Task InstallTheme_WhenCalledWithoutArchive_ShouldReturnValidationProblem()
    {
        // Arrange
        // a multipart form without any file part, only a regular form field, so the archive is missing
        using MultipartFormDataContent multipartContent = [];
        multipartContent.Add(new StringContent("value"), "field");

        // Act
        HttpResponseMessage response = await _client.PostAsync("/api/v1/themes", multipartContent);

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
        Assert.Contains("ThemeArchiveCannotBeNull", errors["General.Validation"]);
    }

    [Fact]
    public async Task InstallTheme_WhenCalledWithNonAdminAccount_ShouldReturnForbiddenResult()
    {
        // Arrange
        HttpClient nonAdminClient = await _apiFactory.CreateAuthenticatedClientAsync();
        byte[] archiveBytes = _themeArchiveFixture.Create();
        using MultipartFormDataContent multipartContent = [];
        using ByteArrayContent fileContent = new(archiveBytes);
        multipartContent.Add(fileContent, "file", "theme.zip");

        // Act
        HttpResponseMessage response = await nonAdminClient.PostAsync("/api/v1/themes", multipartContent);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());
        Assert.Equal("/api/v1/themes", problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);
    }

    [Fact]
    public async Task InstallTheme_WhenCalledWithoutAuthentication_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient anonymousClient = _apiFactory.CreateClient();
        byte[] archiveBytes = _themeArchiveFixture.Create();
        using MultipartFormDataContent multipartContent = [];
        using ByteArrayContent fileContent = new(archiveBytes);
        multipartContent.Add(fileContent, "file", "theme.zip");

        // Act
        HttpResponseMessage response = await anonymousClient.PostAsync("/api/v1/themes", multipartContent);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Disposes API factory resources, removing the installed test themes and the authorization seed data of the admin test user.
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
