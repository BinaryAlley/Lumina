#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.UsersManagement;
using Lumina.Presentation.Web.Common.Requests.Common;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Tools.Settings.UpdatePlaybackState;
using Lumina.Presentation.Web.Fixtures.Common.DTO.UsersManagement;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebEmptyRequest = Lumina.Presentation.Web.Common.Requests.Common.EmptyRequest;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Tools.Settings.UpdatePlaybackState;

/// <summary>
/// Contains unit tests for the <see cref="UpdatePlaybackStateEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePlaybackStateEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly UpdatePlaybackStateEndpoint _sut;
    private readonly PlaybackStateDtoFixture _playbackStateDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePlaybackStateEndpointTests"/> class.
    /// </summary>
    public UpdatePlaybackStateEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<UpdatePlaybackStateEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldUpdatePlaybackStateViaApiAndReturnSuccess()
    {
        // Arrange
        PlaybackStateDto request = _playbackStateDtoFixture.Create(volume: 0.35, isMuted: true);
        _mockApiHttpClient.PutAsync<WebEmptyRequest, PlaybackStateDto>(Arg.Any<string>(), Arg.Any<PlaybackStateDto>(), Arg.Any<CancellationToken>())
            .Returns(new WebEmptyRequest());

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<WebEmptyRequest, PlaybackStateDto>(
            ApiRoutes.Users.UPDATE_PLAYBACK_STATE,
            Arg.Is<PlaybackStateDto>(state => state.Volume == 0.35 && state.IsMuted == true),
            Arg.Any<CancellationToken>());
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.True(jsonDocument.RootElement.GetProperty("data").GetProperty("isUpdated").GetBoolean());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldForwardTheVolumeAndMuteStateToApi()
    {
        // Arrange
        PlaybackStateDto request = _playbackStateDtoFixture.Create(volume: 0.0, isMuted: false);
        _mockApiHttpClient.PutAsync<WebEmptyRequest, PlaybackStateDto>(Arg.Any<string>(), Arg.Any<PlaybackStateDto>(), Arg.Any<CancellationToken>())
            .Returns(new WebEmptyRequest());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<WebEmptyRequest, PlaybackStateDto>(
            ApiRoutes.Users.UPDATE_PLAYBACK_STATE,
            Arg.Is<PlaybackStateDto>(state => state.Volume == 0.0 && state.IsMuted == false),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRequestHasNullValues_ShouldForwardTheNullsToApi()
    {
        // Arrange
        PlaybackStateDto request = _playbackStateDtoFixture.Create(includeVolume: false, includeIsMuted: false);
        _mockApiHttpClient.PutAsync<WebEmptyRequest, PlaybackStateDto>(Arg.Any<string>(), Arg.Any<PlaybackStateDto>(), Arg.Any<CancellationToken>())
            .Returns(new WebEmptyRequest());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<WebEmptyRequest, PlaybackStateDto>(
            ApiRoutes.Users.UPDATE_PLAYBACK_STATE,
            Arg.Is<PlaybackStateDto>(state => state.Volume == null && state.IsMuted == null),
            Arg.Any<CancellationToken>());
    }
}
