#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.FileSystemManagement.FileSystem;
using Lumina.Domain.Common.Enums.FileSystem;
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
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        FileSystemTypeResponse? result = JsonSerializer.Deserialize<FileSystemTypeResponse>(content, _jsonOptions);
        Assert.NotNull(result);
        Assert.True(result!.PlatformType == PlatformType.Unix || result.PlatformType == PlatformType.Windows);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledMultipleTimes_ShouldReturnConsistentResults()
    {
        // Act
        HttpResponseMessage response1 = await _client.GetAsync("/api/v1/file-system/get-type");
        HttpResponseMessage response2 = await _client.GetAsync("/api/v1/file-system/get-type");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        string content1 = await response1.Content.ReadAsStringAsync();
        string content2 = await response2.Content.ReadAsStringAsync();

        FileSystemTypeResponse? result1 = JsonSerializer.Deserialize<FileSystemTypeResponse>(content1, _jsonOptions);
        FileSystemTypeResponse? result2 = JsonSerializer.Deserialize<FileSystemTypeResponse>(content2, _jsonOptions);

        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(result1.PlatformType, result2.PlatformType);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithCancellationToken_ShouldCompletesuccessfuly()
    {
        // Arrange
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));

        // Act & Assert
        Exception? exception = await Record.ExceptionAsync(async () =>
            await _client.GetAsync("/api/v1/file-system/get-type", cts.Token)
        );
        Assert.Null(exception);
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
