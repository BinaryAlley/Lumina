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
/// Contains integration tests for the <see cref="ValidatePathEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ValidatePathEndpointTests : IClassFixture<LuminaApiFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatePathEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public ValidatePathEndpointTests(LuminaApiFactory apiFactory)
    {
        _client = apiFactory.CreateClient();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithValidPath_ShouldReturnOkResultWithIsValidTrue()
    {
        // Arrange
        string tempPath = System.IO.Path.GetTempPath();
        string testPath = System.IO.Path.Combine(tempPath, "testFolder", "testFile.txt");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/path/validate?path={Uri.EscapeDataString(testPath)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        PathValidResponse? result = JsonSerializer.Deserialize<PathValidResponse>(content, _jsonOptions);
        result.Should().NotBeNull();
        result!.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithInvalidPath_ShouldReturnOkResultWithIsValidFalse()
    {
        // Arrange
        string invalidPath = "invalid:path";

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/path/validate?path={Uri.EscapeDataString(invalidPath)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        PathValidResponse? result = JsonSerializer.Deserialize<PathValidResponse>(content, _jsonOptions);
        result.Should().NotBeNull();
        result!.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithEmptyPath_ShouldReturnOkResultWithIsValidFalse()
    {
        // Arrange
        string path = "";

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/v1/path/validate?path={Uri.EscapeDataString(path)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        PathValidResponse? result = JsonSerializer.Deserialize<PathValidResponse>(content, _jsonOptions);
        result.Should().NotBeNull();
        result!.IsValid.Should().BeFalse();
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
            await _client.GetAsync($"/api/v1/path/validate?path={encodedPath}", cts.Token);
        };

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
    }
}
