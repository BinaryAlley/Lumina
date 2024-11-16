#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
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

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.FileSystemManagement.Path;

/// <summary>
/// Contains integration tests for the <see cref="GetPathParentEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathParentEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
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
    /// Initializes a new instance of the <see cref="GetPathParentEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetPathParentEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task ExecuteAsync_WhenCalledWithValidPath_ShouldReturnOkResultWithPathSegmentResponses()
    {
        // Arrange
        string testPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "testDirectory", "testFile.txt");
        string encodedPath = Uri.EscapeDataString(testPath);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/path/get-path-parent?path={encodedPath}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        IEnumerable<PathSegmentResponse>? result = JsonSerializer.Deserialize<IEnumerable<PathSegmentResponse>>(content, _jsonOptions);
        result.Should().NotBeNull();
        result!.Should().NotBeEmpty();

        List<string> hierarchy = [];
        string path = System.IO.Path.GetDirectoryName(testPath)!;
        while (!string.IsNullOrEmpty(path))
        {
            string segment = s_isUnix && path == System.IO.Path.DirectorySeparatorChar.ToString() ? path : System.IO.Path.GetFileName(path);
            if (string.IsNullOrEmpty(segment))
            {
                hierarchy.Add(System.IO.Path.GetPathRoot(path)!.TrimEnd(System.IO.Path.DirectorySeparatorChar));
                break;
            }
            hierarchy.Add(segment);
            path = System.IO.Path.GetDirectoryName(path)!;
        }
        hierarchy.Reverse();

        result!.Select(r => r.Path).Should().BeEquivalentTo(hierarchy, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithRootPath_ShouldReturnForbiddenResult()
    {
        // Arrange
        string rootPath = System.IO.Path.GetPathRoot(System.IO.Path.GetTempPath())!;
        string encodedPath = Uri.EscapeDataString(rootPath);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/path/get-path-parent?path={encodedPath}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        string content = await response.Content.ReadAsStringAsync();

        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!["status"].GetInt32().Should().Be(StatusCodes.Status403Forbidden);
        problemDetails["title"].GetString().Should().Be("General.Failure");
        problemDetails["instance"].GetString().Should().Be("/api/v1/path/get-path-parent");
        problemDetails["detail"].GetString().Should().Be("CannotNavigateUp");
        problemDetails["type"].GetString().Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.4");
        problemDetails["traceId"].GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithInvalidPath_ShouldReturnValidationProblemResult()
    {
        // Arrange
        string invalidPath = "invalid:path";
        string encodedPath = Uri.EscapeDataString(invalidPath);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/path/get-path-parent?path={encodedPath}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableContent);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!["status"].GetInt32().Should().Be(StatusCodes.Status422UnprocessableEntity);
        problemDetails["title"].GetString().Should().Be("General.Validation");
        problemDetails["instance"].GetString().Should().Be("/api/v1/path/get-path-parent");
        problemDetails["detail"].GetString().Should().Be("OneOrMoreValidationErrorsOccurred");
        problemDetails["type"].GetString().Should().Be("https://tools.ietf.org/html/rfc4918#section-11.2");
        problemDetails["traceId"].GetString().Should().NotBeNullOrWhiteSpace();

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        errors.Should().ContainKey("General.Validation").WhoseValue.Should().BeEquivalentTo(["InvalidPath"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithEmptyPath_ShouldReturnValidationProblemResult()
    {
        // Arrange
        string emptyPath = "";
        string encodedPath = Uri.EscapeDataString(emptyPath);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/path/get-path-parent?path={encodedPath}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableContent);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!["status"].GetInt32().Should().Be(StatusCodes.Status422UnprocessableEntity);
        problemDetails["title"].GetString().Should().Be("General.Validation");
        problemDetails["instance"].GetString().Should().Be("/api/v1/path/get-path-parent");
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
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));

        // Act
        Func<Task> act = async () => await _client.GetAsync($"/api/v1/path/get-path-parent?path={encodedPath}", cts.Token);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationTokenIsCancelled_ShouldThrowTaskCanceledException()
    {
        // Arrange
        string testPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "testDirectory");
        string encodedPath = Uri.EscapeDataString(testPath);
        using CancellationTokenSource cts = new();

        // Act
        Func<Task> act = async () =>
        {
            cts.Cancel(); // Cancel the token immediately
            await _client.GetAsync($"/api/v1/path/get-path-parent?path={encodedPath}", cts.Token);
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
