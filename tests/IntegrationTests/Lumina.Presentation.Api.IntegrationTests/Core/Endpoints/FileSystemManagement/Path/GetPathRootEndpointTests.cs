#region ========================================================================= USING =====================================================================================
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
/// Contains integration tests for the <see cref="GetPathRootEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathRootEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
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
    /// Initializes a new instance of the <see cref="GetPathRootEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetPathRootEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task ExecuteAsync_WhenCalledWithValidPath_ShouldReturnOkResultWithPathSegmentResponse()
    {
        // Arrange
        string testPath = System.IO.Path.GetTempPath();
        string encodedPath = Uri.EscapeDataString(testPath);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/path/get-path-root?path={encodedPath}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        PathSegmentResponse? result = JsonSerializer.Deserialize<PathSegmentResponse>(content, _jsonOptions);
        Assert.NotNull(result);
        Assert.NotNull(result!.Path);
        Assert.NotEmpty(result.Path);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithInvalidPath_ShouldReturnValidationProblemResult()
    {
        // Arrange
        string invalidPath = "invalid:path";
        string encodedPath = Uri.EscapeDataString(invalidPath);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/path/get-path-root?path={encodedPath}");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableContent, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("/api/v1/path/get-path-root", problemDetails["instance"].GetString());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails["detail"].GetString());
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", problemDetails["type"].GetString());
        Assert.NotNull(problemDetails["traceId"].GetString());
        Assert.NotEmpty(problemDetails["traceId"].GetString());

        Dictionary<string, string[]>? errors = problemDetails["errors"].Deserialize<Dictionary<string, string[]>>(_jsonOptions);
        Assert.NotNull(errors);
        Assert.Contains("General.Validation", errors.Keys);
        Assert.Contains("InvalidPath", errors["General.Validation"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithEmptyPath_ShouldReturnValidationProblemResult()
    {
        // Arrange
        string emptyPath = "";
        string encodedPath = Uri.EscapeDataString(emptyPath);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/path/get-path-root?path={encodedPath}");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableContent, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        string content = await response.Content.ReadAsStringAsync();
        Dictionary<string, JsonElement>? problemDetails = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content, _jsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails!["status"].GetInt32());
        Assert.Equal("General.Validation", problemDetails["title"].GetString());
        Assert.Equal("/api/v1/path/get-path-root", problemDetails["instance"].GetString());
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
        string testPath = System.IO.Path.GetTempPath();
        string encodedPath = Uri.EscapeDataString(testPath);
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));

        // Act & Assert
        Exception? exception = await Record.ExceptionAsync(async () =>
            await _client.GetAsync($"/api/v1/path/get-path-root?path={encodedPath}", cts.Token)
        );
        Assert.Null(exception);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationTokenIsCanceled_ShouldThrowTaskCanceledException()
    {
        // Arrange
        string testPath = System.IO.Path.GetTempPath();
        string encodedPath = Uri.EscapeDataString(testPath);
        using CancellationTokenSource cts = new();

        // Act & Assert
        Exception? exception = await Record.ExceptionAsync(async () =>
        {
            cts.Cancel();
            await _client.GetAsync($"/api/v1/path/get-path-root?path={encodedPath}", cts.Token);
        });
        Assert.IsType<TaskCanceledException>(exception);
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
