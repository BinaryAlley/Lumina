#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.UoW;
using Lumina.Presentation.Api.Core.Endpoints.Plugins.InstallPlugin;
using Lumina.Presentation.Api.Fixtures.Core.Endpoints.Plugins;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Plugins.InstallPlugin;

/// <summary>
/// Contains integration tests for the <see cref="InstallPluginEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class InstallPluginEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly TestPluginArchiveFixture _pluginArchiveFixture = new();
    private readonly List<string> _installedPluginFileNames = [];
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="InstallPluginEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public InstallPluginEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task InstallPlugin_WhenCalledWithValidZipArchive_ShouldInstallPlugin()
    {
        // Arrange
        string dllName = $"test-plugin-{Guid.NewGuid():N}.dll";
        byte[] archiveBytes = _pluginArchiveFixture.CreateZip(dllName);
        using MultipartFormDataContent multipartContent = [];
        using ByteArrayContent fileContent = new(archiveBytes);
        multipartContent.Add(fileContent, "archive", "plugin.zip");
        _installedPluginFileNames.Add(dllName);

        // Act
        HttpResponseMessage response = await _client.PostAsync("/api/v1/plugins", multipartContent);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(File.Exists(Path.Combine(AppContext.BaseDirectory, "plugins", dllName)));
    }

    [Fact]
    public async Task InstallPlugin_WhenCalledWithSingleDll_ShouldInstallPlugin()
    {
        // Arrange
        string dllName = $"test-plugin-{Guid.NewGuid():N}.dll";
        using MultipartFormDataContent multipartContent = [];
        using ByteArrayContent fileContent = new(_pluginArchiveFixture.CreateDll());
        multipartContent.Add(fileContent, "archive", dllName);
        _installedPluginFileNames.Add(dllName);

        // Act
        HttpResponseMessage response = await _client.PostAsync("/api/v1/plugins", multipartContent);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(File.Exists(Path.Combine(AppContext.BaseDirectory, "plugins", dllName)));
    }

    [Fact]
    public async Task InstallPlugin_WhenCalledWithInvalidArchive_ShouldReturnProblemResult()
    {
        // Arrange
        byte[] invalidArchive = Encoding.UTF8.GetBytes("this is not a valid zip archive");
        using MultipartFormDataContent multipartContent = [];
        using ByteArrayContent fileContent = new(invalidArchive);
        multipartContent.Add(fileContent, "archive", "plugin.zip");

        // Act
        HttpResponseMessage response = await _client.PostAsync("/api/v1/plugins", multipartContent);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Failure", problemDetails["title"].GetString());
        Assert.Equal("PluginArchiveNotReadable", problemDetails["detail"].GetString());
        Assert.Equal("/api/v1/plugins", problemDetails["instance"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString()!);
    }

    [Fact]
    public async Task InstallPlugin_WhenCalledWithZipWithoutAssemblies_ShouldReturnProblemResult()
    {
        // Arrange
        using MultipartFormDataContent multipartContent = [];
        using ByteArrayContent fileContent = new(CreateZipWithEntry("readme.txt", "not an assembly"));
        multipartContent.Add(fileContent, "archive", "plugin.zip");

        // Act
        HttpResponseMessage response = await _client.PostAsync("/api/v1/plugins", multipartContent);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Failure", problemDetails["title"].GetString());
        Assert.Equal("PluginArchiveContainsNoAssemblies", problemDetails["detail"].GetString());
    }

    [Fact]
    public async Task InstallPlugin_WhenCalledWithUnsupportedFileType_ShouldReturnValidationProblem()
    {
        // Arrange
        using MultipartFormDataContent multipartContent = [];
        using ByteArrayContent fileContent = new(_pluginArchiveFixture.CreateDll());
        multipartContent.Add(fileContent, "archive", "plugin.txt");

        // Act
        HttpResponseMessage response = await _client.PostAsync("/api/v1/plugins", multipartContent);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableContent, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        Assert.NotNull(errors);
        Assert.Contains("General.Validation", errors.Keys);
        Assert.Contains("UnsupportedPluginFileType", errors["General.Validation"]);
    }

    [Fact]
    public async Task InstallPlugin_WhenCalledWithoutArchive_ShouldReturnValidationProblem()
    {
        // Arrange
        // a multipart form without any file part, only a regular form field, so the archive is missing
        using MultipartFormDataContent multipartContent = [];
        multipartContent.Add(new StringContent("value"), "field");

        // Act
        HttpResponseMessage response = await _client.PostAsync("/api/v1/plugins", multipartContent);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableContent, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        Assert.NotNull(errors);
        Assert.Contains("General.Validation", errors.Keys);
        Assert.Contains("PluginArchiveCannotBeNull", errors["General.Validation"]);
    }

    [Fact]
    public async Task InstallPlugin_WhenCalledWithNonAdminAccount_ShouldReturnForbiddenResult()
    {
        // Arrange
        HttpClient nonAdminClient = await _apiFactory.CreateAuthenticatedClientAsync();
        byte[] archiveBytes = _pluginArchiveFixture.CreateZip();
        using MultipartFormDataContent multipartContent = [];
        using ByteArrayContent fileContent = new(archiveBytes);
        multipartContent.Add(fileContent, "archive", "plugin.zip");

        // Act
        HttpResponseMessage response = await nonAdminClient.PostAsync("/api/v1/plugins", multipartContent);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());
    }

    [Fact]
    public async Task InstallPlugin_WhenCalledWithoutAuthentication_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        HttpClient anonymousClient = _apiFactory.CreateClient();
        byte[] archiveBytes = _pluginArchiveFixture.CreateZip();
        using MultipartFormDataContent multipartContent = [];
        using ByteArrayContent fileContent = new(archiveBytes);
        multipartContent.Add(fileContent, "archive", "plugin.zip");

        // Act
        HttpResponseMessage response = await anonymousClient.PostAsync("/api/v1/plugins", multipartContent);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Builds an in-memory ZIP archive containing the provided entry.
    /// </summary>
    /// <param name="entryName">The name of the archive entry.</param>
    /// <param name="content">The content of the archive entry.</param>
    /// <returns>The ZIP archive bytes.</returns>
    private static byte[] CreateZipWithEntry(string entryName, string content)
    {
        using MemoryStream memoryStream = new();
        using (System.IO.Compression.ZipArchive zipArchive = new(memoryStream, System.IO.Compression.ZipArchiveMode.Create, leaveOpen: true))
        {
            System.IO.Compression.ZipArchiveEntry entry = zipArchive.CreateEntry(entryName);
            using StreamWriter writer = new(entry.Open());
            writer.Write(content);
        }

        return memoryStream.ToArray();
    }

    /// <summary>
    /// Disposes API factory resources, removing the installed test plugins and the authorization seed data of the admin test user.
    /// </summary>
    public async Task DisposeAsync()
    {
        foreach (string fileName in _installedPluginFileNames)
        {
            string pluginPath = Path.Combine(AppContext.BaseDirectory, "plugins", fileName);
            if (File.Exists(pluginPath))
                File.Delete(pluginPath);
        }

        using IServiceScope scope = _apiFactory.Services.CreateScope();
        LuminaDbContext dbContext = scope.ServiceProvider.GetRequiredService<LuminaDbContext>();

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
