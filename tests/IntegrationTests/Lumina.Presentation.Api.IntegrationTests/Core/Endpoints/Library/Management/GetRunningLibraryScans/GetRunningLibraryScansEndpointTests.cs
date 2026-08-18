#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Presentation.Api.Core.Endpoints.Library.Management.GetRunningLibraryScans;
using Lumina.Presentation.Api.IntegrationTests.Common.Setup;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Core.Endpoints.Library.Management.GetRunningLibraryScans;

/// <summary>
/// Contains integration tests for the <see cref="GetRunningLibraryScansEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRunningLibraryScansEndpointTests : IClassFixture<AuthenticatedLuminaApiFactory>, IAsyncLifetime
{
    private HttpClient _client;
    private readonly AuthenticatedLuminaApiFactory _apiFactory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRunningLibraryScansEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected in-memory API factory.</param>
    public GetRunningLibraryScansEndpointTests(AuthenticatedLuminaApiFactory apiFactory)
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
    public async Task GetRunningLibraryScans_WhenNoScansAreRunning_ShouldReturnEmptyList()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/v1/libraries/scans/running");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        IEnumerable<MediaLibraryScanProgressResponse>? progress = await response.Content.ReadFromJsonAsync<IEnumerable<MediaLibraryScanProgressResponse>>(_jsonOptions);
        Assert.NotNull(progress);
        Assert.Empty(progress!);
    }

    /// <summary>
    /// Disposes API factory resources.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _apiFactory.RemoveTestUserAsync();
    }
}
