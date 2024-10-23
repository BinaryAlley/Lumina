#region ========================================================================= USING =====================================================================================
using FluentAssertions;
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
public class GetDirectoriesEndpointTests : IClassFixture<LuminaApiFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private static readonly bool s_isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDirectoriesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetDirectoriesEndpointTests(LuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
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
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            string content = await response.Content.ReadAsStringAsync();
            List<DirectoryResponse>? directories = JsonSerializer.Deserialize<List<DirectoryResponse>>(content, _jsonOptions);

            directories.Should().NotBeNull();
            directories!.Should().BeEmpty();
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
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            string content = await response.Content.ReadAsStringAsync();
            List<DirectoryResponse>? directories = JsonSerializer.Deserialize<List<DirectoryResponse>>(content, _jsonOptions);

            directories.Should().NotBeNull();
            directories!.Should().NotBeEmpty();

            string[] pathSegments = testPath.Split(System.IO.Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);


            DirectoryResponse firstDirectory = directories!.First();
            firstDirectory.Name.Should().Be((s_isLinux ? "." : string.Empty) + "NestedDirectory_2");
            firstDirectory.Path.Should().Be(System.IO.Path.Combine(testPath, (s_isLinux ? "." : string.Empty) + "NestedDirectory_2" + System.IO.Path.DirectorySeparatorChar));
            directories!.Count.Should().Be(1); // only directories
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
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!["status"].GetInt32().Should().Be(StatusCodes.Status403Forbidden);
        problemDetails["title"].GetString().Should().Be("General.Failure");
        problemDetails["instance"].GetString().Should().Be("/api/v1/directories/get-directories");
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
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/directories/get-directories?path={encodedPath}&includeHiddenElements={includeHiddenElements}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableContent);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!["status"].GetInt32().Should().Be(StatusCodes.Status422UnprocessableEntity);
        problemDetails["title"].GetString().Should().Be("Validation Error");
        problemDetails["instance"].GetString().Should().Be("/api/v1/directories/get-directories");
        problemDetails["detail"].GetString().Should().Be("One or more validation errors occurred.");
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
        Func<Task> act = async () => await _client.GetAsync($"/api/v1/directories/get-directories?path={encodedPath}&includeHiddenElements={includeHiddenElements}", cts.Token);

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
            await _client.GetAsync($"/api/v1/directories/get-directories?path={encodedPath}&includeHiddenElements={includeHiddenElements}", cts.Token);
        };

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
    }
}
