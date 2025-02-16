#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.FileSystemManagement.Directories;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.FileSystemManagement.Fixtures;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.FileSystemManagement.Directories;

/// <summary>
/// Contains integration tests for the <see cref="GetDirectoriesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDirectoriesEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private static readonly bool s_isUnix = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDirectoriesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetDirectoriesEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task GetDirectories_WhenCalledWithValidPathAndHiddenChildrenAndNotIncludeHiddenElements_ShouldReturnNoDirectories()
    {
        // Arrange
        FileSystemStructureFixture fileSystemFixture = new();
        string testPath = fileSystemFixture.CreateFileSystemStructure();
        testPath = System.IO.Path.GetDirectoryName(testPath) ?? testPath;
        testPath = System.IO.Path.GetDirectoryName(testPath) ?? testPath; // two levels to get to the element that has a hidden element as child
        string encodedPath = Uri.EscapeDataString(testPath);
        bool includeHiddenElements = false;
        try
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/v1/directories/get-directories?path={encodedPath}&includeHiddenElements={includeHiddenElements}");

            // Assert
            string content = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            List<DirectoryResponse>? directories = JsonSerializer.Deserialize<List<DirectoryResponse>>(content, _jsonOptions);

            Assert.NotNull(directories);
            Assert.Empty(directories);
        }
        finally
        {
            fileSystemFixture.CleanupFileSystemStructure();
        }
    }

    [Fact]
    public async Task GetDirectories_WhenCalledWithValidPathAndWithIncludeHiddenElements_ShouldReturnDirectoriesWithHiddenElements()
    {
        // Arrange
        FileSystemStructureFixture fileSystemFixture = new();
        string testPath = fileSystemFixture.CreateFileSystemStructure();
        testPath = System.IO.Path.GetDirectoryName(testPath) ?? testPath;
        testPath = System.IO.Path.GetDirectoryName(testPath) ?? testPath; // two levels to get to the element that has a hidden element as child
        string encodedPath = Uri.EscapeDataString(testPath);
        bool includeHiddenElements = true;
        try
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/v1/directories/get-directories?path={encodedPath}&includeHiddenElements={includeHiddenElements}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string content = await response.Content.ReadAsStringAsync();
            List<DirectoryResponse>? directories = JsonSerializer.Deserialize<List<DirectoryResponse>>(content, _jsonOptions);

            Assert.NotNull(directories);
            Assert.NotNull(directories);

            string[] pathSegments = testPath.Split(System.IO.Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);


            DirectoryResponse firstDirectory = directories!.First();
            Assert.Equal((s_isUnix ? "." : string.Empty) + "NestedDirectory_2", directories!.First().Name);
            Assert.Equal(System.IO.Path.Combine(testPath, (s_isUnix ? "." : string.Empty) + "NestedDirectory_2" + System.IO.Path.DirectorySeparatorChar), directories.First().Path);
            Assert.Single(directories); // only directories
        }
        finally
        {
            fileSystemFixture.CleanupFileSystemStructure();
        }
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithInvalidPath_ShouldReturnForbiddenResult()
    {
        // Arrange
        string invalidPath = "invalid:path";
        string encodedPath = Uri.EscapeDataString(invalidPath);
        bool includeHiddenElements = true;

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/directories/get-directories?path={encodedPath}&includeHiddenElements={includeHiddenElements}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Failure", problemDetails["title"].GetString());
        Assert.Equal("/api/v1/directories/get-directories", problemDetails["instance"].GetString());
        Assert.Equal("UnauthorizedAccess", problemDetails["detail"].GetString());
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails["type"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithEmptyPath_ShouldReturnValidationProblemResult()
    {
        // Arrange
        string emptyPath = "";
        string encodedPath = Uri.EscapeDataString(emptyPath);
        bool includeHiddenElements = true;

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/directories/get-directories?path={encodedPath}&includeHiddenElements={includeHiddenElements}");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableContent, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("/api/v1/directories/get-directories", problemDetails["instance"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", problemDetails["type"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString());

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        Assert.NotNull(errors);
        Assert.Contains("General.Validation", errors.Keys);
        Assert.Contains("PathCannotBeEmpty", errors["General.Validation"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithCancellationToken_ShouldCompletesuccessfuly()
    {
        // Arrange
        string testPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "testDirectory");
        string encodedPath = Uri.EscapeDataString(testPath);
        bool includeHiddenElements = true;
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));

        // Act & Assert
        Exception? exception = await Record.ExceptionAsync(async () =>
            await _client.GetAsync($"/api/v1/directories/get-directories?path={encodedPath}&includeHiddenElements={includeHiddenElements}", cts.Token)
        );
        Assert.Null(exception);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationTokenIsCanceled_ShouldThrowTaskCanceledException()
    {
        // Arrange
        string testPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "testDirectory");
        string encodedPath = Uri.EscapeDataString(testPath);
        bool includeHiddenElements = true;
        using CancellationTokenSource cts = new();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            cts.Cancel();
            await _client.GetAsync($"/api/v1/directories/get-directories?path={encodedPath}&includeHiddenElements={includeHiddenElements}", cts.Token);
        });
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public Task DisposeAsync()
    {
        _apiFactory.Dispose();
        return Task.CompletedTask;
    }
}
