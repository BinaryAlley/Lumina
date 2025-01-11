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
/// Contains integration tests for the <see cref="GetPathSeparatorEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathSeparatorEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
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
    /// Initializes a new instance of the <see cref="GetPathSeparatorEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetPathSeparatorEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task ExecuteAsync_WhenCalled_ShouldReturnOkResultWithPathSeparatorResponse()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/path/get-path-separator");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        PathSeparatorResponse? result = JsonSerializer.Deserialize<PathSeparatorResponse>(content, _jsonOptions);
        result.Should().NotBeNull();
        result!.Separator.Should().BeOneOf("/", "\\");
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledMultipleTimes_ShouldReturnConsistentResults()
    {
        // Act
        HttpResponseMessage response1 = await _client.GetAsync("/api/v1/path/get-path-separator");
        HttpResponseMessage response2 = await _client.GetAsync("/api/v1/path/get-path-separator");

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        string content1 = await response1.Content.ReadAsStringAsync();
        string content2 = await response2.Content.ReadAsStringAsync();

        PathSeparatorResponse? result1 = JsonSerializer.Deserialize<PathSeparatorResponse>(content1, _jsonOptions);
        PathSeparatorResponse? result2 = JsonSerializer.Deserialize<PathSeparatorResponse>(content2, _jsonOptions);

        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1.Should().BeEquivalentTo(result2);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithCancellationToken_ShouldCompletesuccessfuly()
    {
        // Arrange
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));

        // Act
        Func<Task> act = async () => await _client.GetAsync("/api/v1/path/get-path-separator", cts.Token);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationTokenIsCancelled_ShouldThrowTaskCanceledException()
    {
        // Arrange
        using CancellationTokenSource cts = new();

        // Act
        Func<Task> act = async () =>
        {
            cts.Cancel(); // Cancel the token immediately
            await _client.GetAsync("/api/v1/path/get-path-separator", cts.Token);
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
