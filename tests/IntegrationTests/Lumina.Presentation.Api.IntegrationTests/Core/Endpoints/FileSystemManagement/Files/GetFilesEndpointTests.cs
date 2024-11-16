#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Contracts.Responses.FileSystemManagement.Files;
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

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.FileSystemManagement.Files;

/// <summary>
/// Contains integration tests for the <see cref="GetFilesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFilesEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
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
    /// Initializes a new instance of the <see cref="GetFilesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetFilesEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task ExecuteAsync_WhenCalledWithValidPathAndNotIncludeHiddenElements_ShouldReturnFilesWithoutHiddenElements()
    {
        // Arrange
        FileSystemStructureFixture fileSystemFixture = new();
        string testPath = fileSystemFixture.CreateFileSystemStructure();
        testPath = System.IO.Path.GetDirectoryName(testPath) ?? testPath;
        bool includeHiddenElements = false;
        try
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/v1/files/get-files?path={Uri.EscapeDataString(testPath)}&includeHiddenElements={includeHiddenElements}");

            // Assert
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            string content = await response.Content.ReadAsStringAsync();
            List<FileResponse>? files = JsonSerializer.Deserialize<List<FileResponse>>(content, _jsonOptions);

            files.Should().NotBeNull();
            files!.Should().NotBeEmpty();

            string[] pathSegments = testPath.Split(System.IO.Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);


            FileResponse firstDirectory = files!.First();
            firstDirectory.Name.Should().Be("TestFile_2.txt");
            firstDirectory.Path.Should().Be(System.IO.Path.Combine(testPath, "TestFile_2.txt"));
            files!.Count.Should().Be(1); // only files
        }
        finally
        {
            fileSystemFixture.CleanupFileSystemStructure();
        }
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithValidPathAndHiddenChildrenAndNotIncludeHiddenElements_ShouldReturnNoFiles()
    {
        // Arrange
        FileSystemStructureFixture fileSystemFixture = new();
        string testPath = fileSystemFixture.CreateFileSystemStructure();
        testPath = System.IO.Path.GetDirectoryName(testPath) ?? testPath;
        testPath = System.IO.Path.GetDirectoryName(testPath) ?? testPath; // two levels to get to the element that has a hidden element as child
        bool includeHiddenElements = false;
        try
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/v1/files/get-files?path={Uri.EscapeDataString(testPath)}&includeHiddenElements={includeHiddenElements}");

            // Assert
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            string content = await response.Content.ReadAsStringAsync();
            List<FileResponse>? files = JsonSerializer.Deserialize<List<FileResponse>>(content, _jsonOptions);

            files.Should().NotBeNull();
            FileResponse firstDirectory = files!.First();
            firstDirectory.Name.Should().Be("TestFile_2.txt");
            firstDirectory.Path.Should().Be(System.IO.Path.Combine(testPath, "TestFile_2.txt"));
        }
        finally
        {
            fileSystemFixture.CleanupFileSystemStructure();
        }
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithValidPathAndWithIncludeHiddenElements_ShouldReturnFilesWithHiddenElements()
    {
        // Arrange
        FileSystemStructureFixture fileSystemFixture = new();
        string testPath = fileSystemFixture.CreateFileSystemStructure();
        testPath = System.IO.Path.GetDirectoryName(testPath) ?? testPath;
        testPath = System.IO.Path.GetDirectoryName(testPath) ?? testPath; // two levels to get to the element that has a hidden element as child
        bool includeHiddenElements = true;
        try
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/v1/files/get-files?path={Uri.EscapeDataString(testPath)}&includeHiddenElements={includeHiddenElements}");

            // Assert
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            string content = await response.Content.ReadAsStringAsync();
            List<FileResponse>? files = JsonSerializer.Deserialize<List<FileResponse>>(content, _jsonOptions);

            files.Should().NotBeNull();
            files!.Should().NotBeEmpty();

            string[] pathSegments = testPath.Split(System.IO.Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

            FileResponse firstDirectory = files!.First();
            firstDirectory.Name.Should().Be((s_isUnix ? "." : string.Empty) + "TestFile_1.txt");
            firstDirectory.Path.Should().Be(System.IO.Path.Combine(testPath, (s_isUnix ? "." : string.Empty) + "TestFile_1.txt"));
            FileResponse secondDirectory = files!.Last();
            secondDirectory.Name.Should().Be("TestFile_2.txt");
            secondDirectory.Path.Should().Be(System.IO.Path.Combine(testPath, "TestFile_2.txt"));
            files!.Count.Should().Be(2);
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
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/files/get-files?path={encodedPath}&includeHiddenElements={includeHiddenElements}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!["status"].GetInt32().Should().Be(StatusCodes.Status403Forbidden);
        problemDetails["title"].GetString().Should().Be("General.Failure");
        problemDetails["instance"].GetString().Should().Be("/api/v1/files/get-files");
        problemDetails["detail"].GetString().Should().Be("UnauthorizedAccess");
        problemDetails["type"].GetString().Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.4");
        problemDetails["traceId"].GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithEmptyPath_ShouldReturnValidationProblemResult()
    {
        // Arrange
        string emptyPath = "";
        string encodedPath = Uri.EscapeDataString(emptyPath);
        bool includeHiddenElements = true;

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/files/get-files?path={encodedPath}&includeHiddenElements={includeHiddenElements}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableContent);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!["status"].GetInt32().Should().Be(StatusCodes.Status422UnprocessableEntity);
        problemDetails["title"].GetString().Should().Be("General.Validation");
        problemDetails["instance"].GetString().Should().Be("/api/v1/files/get-files");
        problemDetails["detail"].GetString().Should().Be("OneOrMoreValidationErrorsOccurred");
        problemDetails["type"].GetString().Should().Be("https://tools.ietf.org/html/rfc4918#section-11.2");
        problemDetails["traceId"].GetString().Should().NotBeNullOrWhiteSpace();

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        errors.Should().ContainKey("General.Validation").WhoseValue.Should().BeEquivalentTo(["PathCannotBeEmpty"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithCancellationToken_ShouldCompleteSuccessfully()
    {
        // Arrange
        string testPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "testDirectory");
        string encodedPath = Uri.EscapeDataString(testPath);
        bool includeHiddenElements = true;
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));

        // Act
        Func<Task> act = async () => await _client.GetAsync($"/api/v1/files/get-files?path={encodedPath}&includeHiddenElements={includeHiddenElements}", cts.Token);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationTokenIsCancelled_ShouldThrowTaskCanceledException()
    {
        // Arrange
        string testPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "testDirectory");
        string encodedPath = Uri.EscapeDataString(testPath);
        bool includeHiddenElements = true;
        using CancellationTokenSource cts = new();

        // Act
        Func<Task> act = async () =>
        {
            cts.Cancel(); // Cancel the token immediately
            await _client.GetAsync($"/api/v1/files/get-files?path={encodedPath}&includeHiddenElements={includeHiddenElements}", cts.Token);
        };

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
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
