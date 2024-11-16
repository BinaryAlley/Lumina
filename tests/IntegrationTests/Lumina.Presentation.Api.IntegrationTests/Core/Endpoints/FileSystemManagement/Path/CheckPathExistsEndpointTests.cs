#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
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
public class CheckPathExistsEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
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
    public CheckPathExistsEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task ExecuteAsync_WhenCalledWithEmptyPath_ShouldReturnValidationProblemResult()
    {
        // Arrange
        string path = "";

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/path/check-path-exists?path={Uri.EscapeDataString(path)}&includeHiddenElements=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableContent);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!["status"].GetInt32().Should().Be(StatusCodes.Status422UnprocessableEntity);
        problemDetails["title"].GetString().Should().Be("General.Validation");
        problemDetails["instance"].GetString().Should().Be("/api/v1/path/check-path-exists");
        problemDetails["detail"].GetString().Should().Be("OneOrMoreValidationErrorsOccurred");
        problemDetails["type"].GetString().Should().Be("https://tools.ietf.org/html/rfc4918#section-11.2");
        problemDetails["traceId"].GetString().Should().NotBeNullOrWhiteSpace();

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        errors.Should().ContainKey("General.Validation").WhoseValue.Should().BeEquivalentTo(["PathCannotBeEmpty"]);
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

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public Task DisposeAsync()
    {
        _apiFactory.Dispose();
        return Task.CompletedTask;
    }
}
