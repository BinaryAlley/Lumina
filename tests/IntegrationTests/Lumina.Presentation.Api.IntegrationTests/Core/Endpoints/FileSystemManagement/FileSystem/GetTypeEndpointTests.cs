#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Domain.Common.Enums.FileSystem;
using Lumina.Contracts.Responses.FileSystemManagement.FileSystem;
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

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.FileSystemManagement.FileSystem;

/// <summary>
/// Contains integration tests for the <see cref="GetTypeEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTypeEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
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
    /// Initializes a new instance of the <see cref="GetTypeEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetTypeEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task ExecuteAsync_WhenCalled_ShouldReturnOkResultWithFileSystemTypeResponse()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/file-system/get-type");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        FileSystemTypeResponse? result = JsonSerializer.Deserialize<FileSystemTypeResponse>(content, _jsonOptions);
        result.Should().NotBeNull();
        result!.PlatformType.Should().BeOneOf(PlatformType.Unix, PlatformType.Windows);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledMultipleTimes_ShouldReturnConsistentResults()
    {
        // Act
        HttpResponseMessage response1 = await _client.GetAsync("/api/v1/file-system/get-type");
        HttpResponseMessage response2 = await _client.GetAsync("/api/v1/file-system/get-type");

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        string content1 = await response1.Content.ReadAsStringAsync();
        string content2 = await response2.Content.ReadAsStringAsync();

        FileSystemTypeResponse? result1 = JsonSerializer.Deserialize<FileSystemTypeResponse>(content1, _jsonOptions);
        FileSystemTypeResponse? result2 = JsonSerializer.Deserialize<FileSystemTypeResponse>(content2, _jsonOptions);

        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1.Should().BeEquivalentTo(result2);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithCancellationToken_ShouldCompleteSuccessfully()
    {
        // Arrange
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));

        // Act
        Func<Task> act = async () => await _client.GetAsync("/api/v1/file-system/get-type", cts.Token);

        // Assert
        await act.Should().NotThrowAsync();
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
