#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Plugins.InstallPlugin;

/// <summary>
/// Contains security tests for the <c>POST /api/v1/plugins</c> route.
/// </summary>
[ExcludeFromCodeCoverage]
public class InstallPluginEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private readonly LuminaApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly List<string> _installedPluginFileNames = [];
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="InstallPluginEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public InstallPluginEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // a unique X-Forwarded-For isolates rate limiting state per test
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
    }

    [Fact]
    public async Task InstallPlugin_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        // Act
        HttpResponseMessage response = await _client.PostAsync("/api/v1/plugins", new ByteArrayContent([]));

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
    }

    [Fact]
    public async Task InstallPlugin_WhenAuthenticatedNonAdmin_ShouldReturnForbiddenResult()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        await _apiFactory.CreateAndAuthenticateUserAsync(client);

        // Act
        using MultipartFormDataContent form = CreateMultipartForm(CreateZipWithAssembly(), "plugin.zip");
        HttpResponseMessage response = await client.PostAsync("/api/v1/plugins", form);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InstallPlugin_WithNonZipBody_ShouldReturnCleanProblemDetails()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        await _apiFactory.CreateAndAuthenticateAdminUserAsync(client);

        // Act
        using MultipartFormDataContent form = CreateMultipartForm(Encoding.UTF8.GetBytes("this is not a zip archive"), "plugin.zip");
        HttpResponseMessage response = await client.PostAsync("/api/v1/plugins", form);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Failure", problemDetails["title"].GetString());
        Assert.Equal("PluginArchiveNotReadable", problemDetails["detail"].GetString());
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InstallPlugin_WithZipContainingTraversalEntry_ShouldNotEscapePluginsDirectory()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        await _apiFactory.CreateAndAuthenticateAdminUserAsync(client);
        byte[] zipBytes = CreateZipWithEntries(new Dictionary<string, string>
        {
            [@"..\evil.dll"] = "evil" // path traversal entry inside the archive
        });
        _installedPluginFileNames.Add("evil.dll");

        // Act
        using MultipartFormDataContent form = CreateMultipartForm(zipBytes, "plugin.zip");
        HttpResponseMessage response = await client.PostAsync("/api/v1/plugins", form);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        // the entry is flattened, so it cannot escape the plugins directory
        Assert.False(File.Exists(Path.Combine(AppContext.BaseDirectory, "evil.dll")));
        Assert.True(File.Exists(Path.Combine(AppContext.BaseDirectory, "plugins", "evil.dll")));
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InstallPlugin_WithUnsupportedFileType_ShouldReturnCleanProblemDetails()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        await _apiFactory.CreateAndAuthenticateAdminUserAsync(client);

        // Act
        using MultipartFormDataContent form = CreateMultipartForm([0x4D, 0x5A, 0x90, 0x00], "plugin.exe");
        HttpResponseMessage response = await client.PostAsync("/api/v1/plugins", form);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InstallPlugin_WithEmptyMultipartBody_ShouldReturnCleanValidationProblemDetails()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        await _apiFactory.CreateAndAuthenticateAdminUserAsync(client);
        using MultipartFormDataContent emptyForm = [];

        // Act
        HttpResponseMessage response = await client.PostAsync("/api/v1/plugins", emptyForm);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Builds a multipart form containing a single file part.
    /// </summary>
    /// <param name="fileBytes">The file content bytes.</param>
    /// <param name="fileName">The name of the uploaded file.</param>
    /// <returns>The multipart form content.</returns>
    private static MultipartFormDataContent CreateMultipartForm(byte[] fileBytes, string fileName)
    {
        MultipartFormDataContent form = [];
        ByteArrayContent fileContent = new(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        form.Add(fileContent, "archive", fileName);
        return form;
    }

    /// <summary>
    /// Builds an in-memory ZIP archive containing a single plugin assembly entry.
    /// </summary>
    /// <returns>The ZIP archive bytes.</returns>
    private static byte[] CreateZipWithAssembly()
    {
        return CreateZipWithEntries(new Dictionary<string, string>
        {
            ["test-plugin.dll"] = "not a real assembly, but the installer only checks the file extension"
        });
    }

    /// <summary>
    /// Builds an in-memory ZIP archive from the given entry paths and contents.
    /// </summary>
    /// <param name="entries">The entry paths mapped to their contents.</param>
    /// <returns>The ZIP archive bytes.</returns>
    private static byte[] CreateZipWithEntries(IReadOnlyDictionary<string, string> entries)
    {
        using MemoryStream memoryStream = new();
        using (ZipArchive zipArchive = new(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach ((string path, string content) in entries)
            {
                ZipArchiveEntry entry = zipArchive.CreateEntry(path);
                using StreamWriter writer = new(entry.Open());
                writer.Write(content);
            }
        }

        return memoryStream.ToArray();
    }

    /// <summary>
    /// Disposes the API factory resources and removes the plugin files written by the tests.
    /// </summary>
    public void Dispose()
    {
        foreach (string fileName in _installedPluginFileNames)
        {
            string pluginPath = Path.Combine(AppContext.BaseDirectory, "plugins", fileName);
            if (File.Exists(pluginPath))
                File.Delete(pluginPath);
        }

        _client.Dispose();
    }
}
