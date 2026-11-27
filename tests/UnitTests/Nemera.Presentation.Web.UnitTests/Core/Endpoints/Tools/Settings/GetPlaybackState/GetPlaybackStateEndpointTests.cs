#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.UsersManagement;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Tools.Settings.GetPlaybackState;
using Lumina.Presentation.Web.Fixtures.Common.DTO.UsersManagement;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Tools.Settings.GetPlaybackState;

/// <summary>
/// Contains unit tests for the <see cref="GetPlaybackStateEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaybackStateEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetPlaybackStateEndpoint _sut;
    private readonly PlaybackStateDtoFixture _playbackStateDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaybackStateEndpointTests"/> class.
    /// </summary>
    public GetPlaybackStateEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetPlaybackStateEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsState_ShouldReturnSuccessJsonWithState()
    {
        // Arrange
        PlaybackStateDto expectedState = _playbackStateDtoFixture.Create(volume: 0.35, isMuted: true);
        _mockApiHttpClient.GetAsync<PlaybackStateDto>(ApiRoutes.Users.GET_PLAYBACK_STATE, Arg.Any<CancellationToken>())
            .Returns(expectedState);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = jsonDocument.RootElement.GetProperty("data");
        Assert.Equal(0.35, data.GetProperty("volume").GetDouble());
        Assert.True(data.GetProperty("isMuted").GetBoolean());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldRequestPlaybackStateFromApi()
    {
        // Arrange
        _mockApiHttpClient.GetAsync<PlaybackStateDto>(ApiRoutes.Users.GET_PLAYBACK_STATE, Arg.Any<CancellationToken>())
            .Returns(_playbackStateDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PlaybackStateDto>(ApiRoutes.Users.GET_PLAYBACK_STATE, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenStateHasNullValues_ShouldStillReturnSuccessJson()
    {
        // Arrange
        PlaybackStateDto expectedState = _playbackStateDtoFixture.Create(includeVolume: false, includeIsMuted: false);
        _mockApiHttpClient.GetAsync<PlaybackStateDto>(ApiRoutes.Users.GET_PLAYBACK_STATE, Arg.Any<CancellationToken>())
            .Returns(expectedState);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = jsonDocument.RootElement.GetProperty("data");
        Assert.Equal(JsonValueKind.Null, data.GetProperty("volume").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("isMuted").ValueKind);
    }
}
