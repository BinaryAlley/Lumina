#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Requests.Playlists;
using Lumina.Presentation.Web.Core.Endpoints.Playlists.UpdatePlaylist;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Playlists;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Playlists;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Playlists.UpdatePlaylist;

/// <summary>
/// Contains unit tests for the <see cref="UpdatePlaylistEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePlaylistEndpointTests
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private static readonly Guid s_playlistId = Guid.Parse("ABCDEFAB-CDEF-ABCD-EFAB-CDEFABCDEFAB");
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly UpdatePlaylistEndpoint _sut;
    private readonly UpdatePlaylistRequestFixture _updatePlaylistRequestFixture = new();
    private readonly PlaylistDetailsDtoFixture _playlistDetailsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePlaylistEndpointTests"/> class.
    /// </summary>
    public UpdatePlaylistEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<UpdatePlaylistEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiSucceeds_ShouldReturnSuccessJsonWithTheUpdatedDetails()
    {
        // Arrange
        PlaylistDetailsDto expectedDetails = _playlistDetailsDtoFixture.Create(
            name: "Late night",
            description: "Quiet tracks.");
        SetRoutePlaylistId(s_playlistId);
        _mockApiHttpClient.PutAsync<PlaylistDetailsDto, UpdatePlaylistRequest>(Arg.Any<string>(), Arg.Any<UpdatePlaylistRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedDetails);

        // Act
        IResult result = await _sut.ExecuteAsync(_updatePlaylistRequestFixture.Create(), CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        PlaylistDetailsDto actualDetails = JsonSerializer.Deserialize<PlaylistDetailsDto>(
            jsonDocument.RootElement.GetProperty("data").GetRawText(), s_jsonOptions)!;
        Assert.Equal(expectedDetails, actualDetails);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldPutTheRequestPayloadOnceToTheDocumentedEndpoint()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        UpdatePlaylistRequest request = _updatePlaylistRequestFixture.Create(
            name: "Late night",
            description: "Quiet tracks.");
        SetRoutePlaylistId(playlistId);
        _mockApiHttpClient.PutAsync<PlaylistDetailsDto, UpdatePlaylistRequest>(Arg.Any<string>(), Arg.Any<UpdatePlaylistRequest>(), Arg.Any<CancellationToken>())
            .Returns(_playlistDetailsDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<PlaylistDetailsDto, UpdatePlaylistRequest>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{playlistId}"),
            Arg.Is<UpdatePlaylistRequest>(payload =>
                payload.Name == "Late night" &&
                payload.Description == "Quiet tracks."),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlaylistIdIsAnUppercaseGuid_ShouldForwardItInLowercase()
    {
        // Arrange
        SetRoutePlaylistId(s_playlistId.ToString().ToUpperInvariant());
        _mockApiHttpClient.PutAsync<PlaylistDetailsDto, UpdatePlaylistRequest>(Arg.Any<string>(), Arg.Any<UpdatePlaylistRequest>(), Arg.Any<CancellationToken>())
            .Returns(_playlistDetailsDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(_updatePlaylistRequestFixture.Create(), CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<PlaylistDetailsDto, UpdatePlaylistRequest>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{s_playlistId}"),
            Arg.Any<UpdatePlaylistRequest>(),
            Arg.Any<CancellationToken>());
        await _mockApiHttpClient.DidNotReceive().PutAsync<PlaylistDetailsDto, UpdatePlaylistRequest>(
            Arg.Is<string>(endpoint => endpoint.Contains(s_playlistId.ToString().ToUpperInvariant(), StringComparison.Ordinal)),
            Arg.Any<UpdatePlaylistRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlaylistIdIsNotAGuid_ShouldForwardTheEmptyGuid()
    {
        // Arrange
        SetRoutePlaylistId("not-a-guid");
        _mockApiHttpClient.PutAsync<PlaylistDetailsDto, UpdatePlaylistRequest>(Arg.Any<string>(), Arg.Any<UpdatePlaylistRequest>(), Arg.Any<CancellationToken>())
            .Returns(_playlistDetailsDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(_updatePlaylistRequestFixture.Create(), CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<PlaylistDetailsDto, UpdatePlaylistRequest>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{ZERO_GUID}"),
            Arg.Any<UpdatePlaylistRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlaylistIdRouteValueIsMissing_ShouldForwardTheEmptyGuid()
    {
        // Arrange
        _mockApiHttpClient.PutAsync<PlaylistDetailsDto, UpdatePlaylistRequest>(Arg.Any<string>(), Arg.Any<UpdatePlaylistRequest>(), Arg.Any<CancellationToken>())
            .Returns(_playlistDetailsDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(_updatePlaylistRequestFixture.Create(), CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<PlaylistDetailsDto, UpdatePlaylistRequest>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{ZERO_GUID}"),
            Arg.Any<UpdatePlaylistRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("1;DROP TABLE")] // destructive injection
    [InlineData("../../admin")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    [InlineData("𝕡𝕝𝕒𝕪𝕝𝕚𝕤𝕥")] // unicode payload
    public async Task ExecuteAsync_WhenPlaylistIdIsCrafted_ShouldNormalizeItAndNeverForwardTheRawValue(string payload)
    {
        // Arrange
        SetRoutePlaylistId(payload);
        _mockApiHttpClient.PutAsync<PlaylistDetailsDto, UpdatePlaylistRequest>(Arg.Any<string>(), Arg.Any<UpdatePlaylistRequest>(), Arg.Any<CancellationToken>())
            .Returns(_playlistDetailsDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(_updatePlaylistRequestFixture.Create(), CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<PlaylistDetailsDto, UpdatePlaylistRequest>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{ZERO_GUID}"),
            Arg.Any<UpdatePlaylistRequest>(),
            Arg.Any<CancellationToken>());
        await _mockApiHttpClient.DidNotReceive().PutAsync<PlaylistDetailsDto, UpdatePlaylistRequest>(
            Arg.Is<string>(endpoint => endpoint.Contains(payload, StringComparison.Ordinal)),
            Arg.Any<UpdatePlaylistRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        UpdatePlaylistRequest request = _updatePlaylistRequestFixture.Create();
        PlaylistDetailsDto expectedDetails = _playlistDetailsDtoFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
        SetRoutePlaylistId(s_playlistId);
        _mockApiHttpClient.PutAsync<PlaylistDetailsDto, UpdatePlaylistRequest>(Arg.Any<string>(), Arg.Any<UpdatePlaylistRequest>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.ArgAt<CancellationToken>(2).ThrowIfCancellationRequested();
                return expectedDetails;
            }, callInfo.ArgAt<CancellationToken>(2)));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(request, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }

    /// <summary>
    /// Sets the route value of the playlist identifier route segment.
    /// </summary>
    /// <param name="playlistId">The value of the playlist identifier route segment.</param>
    private void SetRoutePlaylistId(object playlistId)
    {
        _sut.HttpContext.Request.RouteValues["playlistId"] = playlistId;
    }
}
