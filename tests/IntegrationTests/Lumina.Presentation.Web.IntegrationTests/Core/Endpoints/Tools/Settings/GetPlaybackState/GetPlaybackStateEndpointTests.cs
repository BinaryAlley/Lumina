#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.UsersManagement;
using Lumina.Presentation.Web.Core.Endpoints.Tools.Settings.GetPlaybackState;
using Lumina.Presentation.Web.Fixtures.Common.DTO.UsersManagement;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Tools.Settings.GetPlaybackState;

/// <summary>
/// Contains integration tests for the <see cref="GetPlaybackStateEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaybackStateEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly PlaybackStateDtoFixture _playbackStateDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaybackStateEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetPlaybackStateEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetPlaybackState_WhenCalledByAuthenticatedUser_ShouldReturnPlaybackStateFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        PlaybackStateDto expectedState = _playbackStateDtoFixture.Create(volume: 0.35, isMuted: true);
        _apiFactory.ApiClientStub.RegisterGetResponse("users/me/settings/playback-state", expectedState);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, "/en-us/tools/settings/api-get-playback-state");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = json.RootElement.GetProperty("data");
        Assert.Equal(0.35, data.GetProperty("volume").GetDouble());
        Assert.True(data.GetProperty("isMuted").GetBoolean());
        Assert.Contains(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == "users/me/settings/playback-state");
    }

    [Fact]
    public async Task GetPlaybackState_WhenApiReturnsTheDefaultState_ShouldReturnIt()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        PlaybackStateDto expectedState = _playbackStateDtoFixture.Create(volume: 1.0, isMuted: false);
        _apiFactory.ApiClientStub.RegisterGetResponse("users/me/settings/playback-state", expectedState);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, "/en-us/tools/settings/api-get-playback-state");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        using JsonDocument json = JsonDocument.Parse(content);
        JsonElement data = json.RootElement.GetProperty("data");
        Assert.Equal(1.0, data.GetProperty("volume").GetDouble());
        Assert.False(data.GetProperty("isMuted").GetBoolean());
    }
}
