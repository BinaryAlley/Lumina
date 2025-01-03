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
/// Contains integration tests for the <see cref="CombinePathEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CombinePathEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
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
    /// Initializes a new instance of the <see cref="CombinePathEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public CombinePathEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task ExecuteAsync_WhenCalledWithValidPaths_ShouldReturnOkResultWithCombinedPath()
    {
        // Arrange
        string originalPath = System.IO.Path.GetTempPath();
        string newPath = "testDirectory";
        string expectedPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "testDirectory");
        if (!expectedPath.EndsWith(System.IO.Path.DirectorySeparatorChar))
            expectedPath += System.IO.Path.DirectorySeparatorChar;

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/path/combine?originalPath={Uri.EscapeDataString(originalPath)}&newPath={Uri.EscapeDataString(newPath)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        PathSegmentResponse? result = JsonSerializer.Deserialize<PathSegmentResponse>(content, _jsonOptions);
        result.Should().NotBeNull();
        result!.Path.Should().Be(expectedPath);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithEmptyPaths_ShouldReturnValidationProblemResult()
    {
        // Arrange
        string originalPath = "";
        string newPath = "";

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/path/combine?originalPath={Uri.EscapeDataString(originalPath)}&newPath={Uri.EscapeDataString(newPath)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableContent);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!["status"].GetInt32().Should().Be(StatusCodes.Status422UnprocessableEntity);
        problemDetails["title"].GetString().Should().Be("General.Validation");
        problemDetails["instance"].GetString().Should().Be("/api/v1/path/combine");
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
        string originalPath = "/home/user";
        string newPath = "documents";
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));

        // Act
        Func<Task> act = async () => await _client.GetAsync($"/api/v1/path/combine?originalPath={Uri.EscapeDataString(originalPath)}&newPath={Uri.EscapeDataString(newPath)}", cts.Token);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationTokenIsCancelled_ShouldThrowTaskCanceledException()
    {
        // Arrange
        string originalPath = System.IO.Path.GetTempPath();
        string newPath = "testFile.txt";
        using CancellationTokenSource cts = new();

        // Act
        Func<Task> act = async () =>
        {
            cts.Cancel(); // Cancel the token immediately
            await _client.GetAsync($"/api/v1/path/combine?originalPath={Uri.EscapeDataString(originalPath)}&newPath={Uri.EscapeDataString(newPath)}", cts.Token);
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
