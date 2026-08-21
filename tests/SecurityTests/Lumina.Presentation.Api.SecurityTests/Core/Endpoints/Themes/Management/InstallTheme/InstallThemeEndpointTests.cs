#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Api.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
#endregion

namespace Lumina.Presentation.Api.SecurityTests.Core.Endpoints.Themes.Management.InstallTheme;

/// <summary>
/// Contains security tests for the <c>POST /api/v1/themes</c> route.
/// </summary>
[ExcludeFromCodeCoverage]
public class InstallThemeEndpointTests : IClassFixture<LuminaApiFactory>, IDisposable
{
    private const long MAX_ARCHIVE_BYTES = 8 * 1024 * 1024;
    private const string VALID_MANIFEST_JSON = """
        {
          "schemaVersion": 1,
          "id": "valid-theme",
          "name": "Valid Theme",
          "description": "description",
          "author": "author",
          "version": "1.0.0",
          "templates": {
            "default": "templates/default.html"
          }
        }
        """;

    private readonly LuminaApiFactory _apiFactory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="InstallThemeEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public InstallThemeEndpointTests(LuminaApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _client = apiFactory.CreateClient();
        // a unique X-Forwarded-For isolates rate limiting state per test
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", LuminaApiFactory.GetUniqueTestIp());
    }

    [Fact]
    public async Task InstallTheme_WhenUnauthorized_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/themes", new { });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc7235#section-3.1", problemDetails["type"].GetString());
        Assert.Equal("Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("/api/v1/themes", problemDetails["instance"].GetProperty("value").GetString());
        Assert.Equal("Authentication failed", problemDetails["detail"].GetString());
    }

    [Fact]
    public async Task InstallTheme_WhenAuthenticatedNonAdmin_ShouldReturnForbiddenResult()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        await _apiFactory.CreateAndAuthenticateUserAsync(client);

        // Act
        using MultipartFormDataContent form = CreateMultipartForm(Encoding.UTF8.GetBytes("not-a-zip"), "pack.zip", "application/zip");
        HttpResponseMessage response = await client.PostAsync("/api/v1/themes", form);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.Equal("General.Unauthorized", problemDetails["title"].GetString());
        Assert.Equal("NotAuthorized", problemDetails["detail"].GetString());
        Assert.Equal("/api/v1/themes", problemDetails["instance"].GetString());
        Assert.True(problemDetails.ContainsKey("traceId"));
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InstallTheme_WithNonZipBody_ShouldReturnCleanProblemDetails()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        await _apiFactory.CreateAndAuthenticateAdminUserAsync(client);

        // Act
        using MultipartFormDataContent form = CreateMultipartForm(Encoding.UTF8.GetBytes("this is not a zip archive"), "pack.zip", "application/zip");
        HttpResponseMessage response = await client.PostAsync("/api/v1/themes", form);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Failure", problemDetails["title"].GetString());
        Assert.Equal("ThemeArchiveNotReadable", problemDetails["detail"].GetString());
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InstallTheme_WithEmptyArchiveFile_ShouldReturnCleanPackageInvalidProblemDetails()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        await _apiFactory.CreateAndAuthenticateAdminUserAsync(client);

        // Act
        // a multipart upload carrying a 0 byte archive reaches the theme service, which must reject it without crashing
        using MultipartFormDataContent form = CreateMultipartForm([], "pack.zip", "application/zip");
        HttpResponseMessage response = await client.PostAsync("/api/v1/themes", form);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", problemDetails["type"].GetString());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
        Assert.True(problemDetails["errors"].GetProperty("Theme.Package.Invalid").GetArrayLength() > 0);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InstallTheme_WithOversizedArchive_ShouldReturnCleanProblemDetails()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        await _apiFactory.CreateAndAuthenticateAdminUserAsync(client);

        // Act
        using MultipartFormDataContent form = CreateMultipartForm(new byte[MAX_ARCHIVE_BYTES + 1], "pack.zip", "application/zip");
        HttpResponseMessage response = await client.PostAsync("/api/v1/themes", form);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
        Assert.True(problemDetails["errors"].GetProperty("Theme.Archive.TooLarge").GetArrayLength() > 0);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InstallTheme_WithZipContainingTraversalEntry_ShouldReturnCleanProblemDetails()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        await _apiFactory.CreateAndAuthenticateAdminUserAsync(client);
        byte[] zipBytes = CreateZipWithEntries(new Dictionary<string, string>
        {
            ["theme.json"] = VALID_MANIFEST_JSON,
            ["templates/default.html"] = "<html><body>default</body></html>",
            [@"..\evil.txt"] = "evil" // path traversal entry inside the archive
        });

        // Act
        using MultipartFormDataContent form = CreateMultipartForm(zipBytes, "pack.zip", "application/zip");
        HttpResponseMessage response = await client.PostAsync("/api/v1/themes", form);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
        Assert.True(problemDetails["errors"].GetProperty("Theme.Package.Invalid").GetArrayLength() > 0);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InstallTheme_WithManifestDeclaringTraversalTemplate_ShouldReturnCleanProblemDetails()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        await _apiFactory.CreateAndAuthenticateAdminUserAsync(client);
        const string MALICIOUS_MANIFEST = """
            {
              "schemaVersion": 1,
              "id": "evil-theme",
              "name": "Evil Theme",
              "description": "description",
              "author": "author",
              "version": "1.0.0",
              "templates": {
                "default": "templates/default.html",
                "admin": "../secret.html"
              }
            }
            """;
        byte[] zipBytes = CreateZipWithEntries(new Dictionary<string, string>
        {
            ["theme.json"] = MALICIOUS_MANIFEST,
            ["templates/default.html"] = "<html><body>default</body></html>"
        });

        // Act
        using MultipartFormDataContent form = CreateMultipartForm(zipBytes, "pack.zip", "application/zip");
        HttpResponseMessage response = await client.PostAsync("/api/v1/themes", form);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
        Assert.True(problemDetails["errors"].GetProperty("Theme.Package.Invalid").GetArrayLength() > 0);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InstallTheme_WithEmptyMultipartBody_ShouldReturnCleanValidationProblemDetails()
    {
        // Arrange
        HttpClient client = _apiFactory.CreateClient();
        await _apiFactory.CreateAndAuthenticateAdminUserAsync(client);
        using MultipartFormDataContent emptyForm = [];

        // Act
        HttpResponseMessage response = await client.PostAsync("/api/v1/themes", emptyForm);
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
    /// <param name="contentType">The content type of the uploaded file.</param>
    /// <returns>The multipart form content.</returns>
    private static MultipartFormDataContent CreateMultipartForm(byte[] fileBytes, string fileName, string contentType)
    {
        MultipartFormDataContent form = [];
        ByteArrayContent fileContent = new(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(fileContent, "archive", fileName);
        return form;
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
    /// Disposes the API factory resources.
    /// </summary>
    public void Dispose()
    {
        _client.Dispose();
    }
}
