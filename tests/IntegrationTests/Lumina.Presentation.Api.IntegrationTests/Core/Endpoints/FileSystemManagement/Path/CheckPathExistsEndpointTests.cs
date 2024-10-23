#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.FileSystemManagement.Path;

/// <summary>
/// Contains integration tests for the <see cref="CheckPathExistsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CheckPathExistsEndpointTests : IClassFixture<LuminaApiFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckPathExistsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public CheckPathExistsEndpointTests(LuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithExistingPath_ShouldReturnOkResultWithExistsTrue()
    {
        // Arrange
        string tempPath = System.IO.Path.GetTempPath();

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/path/check-path-exists?path={Uri.EscapeDataString(tempPath)}&includeHiddenElements=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        PathExistsResponse? result = JsonSerializer.Deserialize<PathExistsResponse>(content, _jsonOptions);
        result.Should().NotBeNull();
        result!.Exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithNonExistingPath_ShouldReturnOkResultWithExistsFalse()
    {
        // Arrange
        string nonExistingPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/path/check-path-exists?path={Uri.EscapeDataString(nonExistingPath)}&includeHiddenElements=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        PathExistsResponse? result = JsonSerializer.Deserialize<PathExistsResponse>(content, _jsonOptions);
        result.Should().NotBeNull();
        result!.Exists.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithEmptyPath_ShouldReturnOkResultWithExistsFalse()
    {
        // Arrange
        string path = "";

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/path/check-path-exists?path={Uri.EscapeDataString(path)}&includeHiddenElements=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        PathExistsResponse? result = JsonSerializer.Deserialize<PathExistsResponse>(content, _jsonOptions);
        result.Should().NotBeNull();
        result!.Exists.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithInvalidPath_ShouldReturnOkResultWithExistsFalse()
    {
        // Arrange
        string invalidPath = "invalid:path";

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/path/check-path-exists?path={Uri.EscapeDataString(invalidPath)}&includeHiddenElements=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        PathExistsResponse? result = JsonSerializer.Deserialize<PathExistsResponse>(content, _jsonOptions);
        result.Should().NotBeNull();
        result!.Exists.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationTokenIsCancelled_ShouldThrowTaskCanceledException()
    {
        // Arrange
        string testPath = System.IO.Path.GetTempPath();
        string encodedPath = Uri.EscapeDataString(testPath);
        using CancellationTokenSource cts = new();

        // Act
        Func<Task> act = async () =>
        {
            cts.Cancel(); // Cancel the token immediately
            await _client.GetAsync($"/api/v1/path/check-path-exists?path={encodedPath}&includeHiddenElements=false", cts.Token);
        };

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
    }
}
