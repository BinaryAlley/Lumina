#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.UsersManagement;
using Lumina.Presentation.Web.Common.Requests.Common;
using Lumina.Presentation.Web.Core.Endpoints.Tools.Settings.UpdatePlaybackState;
using Lumina.Presentation.Web.Fixtures.Common.DTO.UsersManagement;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Tools.Settings.UpdatePlaybackState;

/// <summary>
/// Contains integration tests for the <see cref="UpdatePlaybackStateEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePlaybackStateEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly PlaybackStateDtoFixture _playbackStateDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePlaybackStateEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public UpdatePlaybackStateEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task UpdatePlaybackState_WhenCalledByAuthenticatedUser_ShouldUpdatePlaybackState()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterPutResponseFactory("users/me/settings/playback-state", _ => new EmptyRequest());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        PlaybackStateDto state = _playbackStateDtoFixture.Create(volume: 0.35, isMuted: true);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(CreateUpdatePlaybackStateRequest(state, webClient.AntiforgeryToken));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.True(json.RootElement.GetProperty("data").GetProperty("isUpdated").GetBoolean());
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == "users/me/settings/playback-state");
    }

    [Fact]
    public async Task UpdatePlaybackState_WhenCalledByAuthenticatedUser_ShouldForwardTheStateToApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterPutResponseFactory("users/me/settings/playback-state", _ => new EmptyRequest());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        PlaybackStateDto state = _playbackStateDtoFixture.Create(volume: 0.0, isMuted: false);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(CreateUpdatePlaybackStateRequest(state, webClient.AntiforgeryToken));

        // Assert
        response.EnsureSuccessStatusCode();
        (string Endpoint, object? Data) putRequest = Assert.Single(
            _apiFactory.ApiClientStub.PutRequests,
            request => request.Endpoint == "users/me/settings/playback-state");
        PlaybackStateDto forwardedState = Assert.IsType<PlaybackStateDto>(putRequest.Data);
        Assert.Equal(0.0, forwardedState.Volume);
        Assert.False(forwardedState.IsMuted);
    }

    /// <summary>
    /// Builds the update request that sends the given <paramref name="state"/> to the update playback state endpoint.
    /// </summary>
    /// <param name="state">The playback state to send.</param>
    /// <param name="antiforgeryToken">The antiforgery token to include in the request.</param>
    /// <returns>The configured update request.</returns>
    private static HttpRequestMessage CreateUpdatePlaybackStateRequest(PlaybackStateDto state, string antiforgeryToken)
    {
        HttpRequestMessage updateRequest = new(HttpMethod.Post, "/en-us/tools/settings/api-update-playback-state")
        {
            Content = JsonContent.Create(state)
        };
        updateRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        updateRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        updateRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);
        return updateRequest;
    }
}
