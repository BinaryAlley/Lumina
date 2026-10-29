#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Requests.Playlists;
using Lumina.Presentation.Web.Core.Endpoints.Playlists.MovePlaylistEntry;
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

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Playlists.MovePlaylistEntry;

/// <summary>
/// Contains unit tests for the <see cref="MovePlaylistEntryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MovePlaylistEntryEndpointTests
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private static readonly Guid s_playlistId = Guid.Parse("ABCDEFAB-CDEF-ABCD-EFAB-CDEFABCDEFAB");
    private static readonly Guid s_playlistEntryId = Guid.Parse("FEDCBAFE-DCBA-FEDC-BAFE-DCBAFEDCBAFE");
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly MovePlaylistEntryEndpoint _sut;
    private readonly MovePlaylistEntryRequestFixture _movePlaylistEntryRequestFixture = new();
    private readonly PlaylistEntryMovedDtoFixture _playlistEntryMovedDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MovePlaylistEntryEndpointTests"/> class.
    /// </summary>
    public MovePlaylistEntryEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<MovePlaylistEntryEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiSucceeds_ShouldReturnSuccessJsonWithTheMovedEntry()
    {
        // Arrange
        PlaylistEntryMovedDto expectedEntry = _playlistEntryMovedDtoFixture.Create(
            playlistId: Guid.NewGuid(),
            entryId: Guid.NewGuid(),
            position: 3);
        SetRouteValues(s_playlistId, s_playlistEntryId);
        _mockApiHttpClient.PutAsync<PlaylistEntryMovedDto, MovePlaylistEntryRequest>(Arg.Any<string>(), Arg.Any<MovePlaylistEntryRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedEntry);

        // Act
        IResult result = await _sut.ExecuteAsync(_movePlaylistEntryRequestFixture.Create(), CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        PlaylistEntryMovedDto actualEntry = JsonSerializer.Deserialize<PlaylistEntryMovedDto>(
            jsonDocument.RootElement.GetProperty("data").GetRawText(), s_jsonOptions)!;
        Assert.Equal(expectedEntry, actualEntry);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldPutTheRequestPayloadOnceToTheDocumentedEndpoint()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        Guid playlistEntryId = Guid.NewGuid();
        MovePlaylistEntryRequest request = _movePlaylistEntryRequestFixture.Create(targetPosition: 5);
        SetRouteValues(playlistId, playlistEntryId);
        _mockApiHttpClient.PutAsync<PlaylistEntryMovedDto, MovePlaylistEntryRequest>(Arg.Any<string>(), Arg.Any<MovePlaylistEntryRequest>(), Arg.Any<CancellationToken>())
            .Returns(_playlistEntryMovedDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<PlaylistEntryMovedDto, MovePlaylistEntryRequest>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{playlistId}/entries/{playlistEntryId}/move"),
            Arg.Is<MovePlaylistEntryRequest>(payload => payload.TargetPosition == 5),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreUppercaseGuids_ShouldForwardThemInLowercase()
    {
        // Arrange
        SetRouteValues(s_playlistId.ToString().ToUpperInvariant(), s_playlistEntryId.ToString().ToUpperInvariant());
        _mockApiHttpClient.PutAsync<PlaylistEntryMovedDto, MovePlaylistEntryRequest>(Arg.Any<string>(), Arg.Any<MovePlaylistEntryRequest>(), Arg.Any<CancellationToken>())
            .Returns(_playlistEntryMovedDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(_movePlaylistEntryRequestFixture.Create(), CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<PlaylistEntryMovedDto, MovePlaylistEntryRequest>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{s_playlistId}/entries/{s_playlistEntryId}/move"),
            Arg.Any<MovePlaylistEntryRequest>(),
            Arg.Any<CancellationToken>());
        await _mockApiHttpClient.DidNotReceive().PutAsync<PlaylistEntryMovedDto, MovePlaylistEntryRequest>(
            Arg.Is<string>(endpoint => endpoint.Contains(s_playlistId.ToString().ToUpperInvariant(), StringComparison.Ordinal)),
            Arg.Any<MovePlaylistEntryRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreNotGuids_ShouldForwardTheEmptyGuids()
    {
        // Arrange
        SetRouteValues("not-a-guid", "not-an-entry-guid");
        _mockApiHttpClient.PutAsync<PlaylistEntryMovedDto, MovePlaylistEntryRequest>(Arg.Any<string>(), Arg.Any<MovePlaylistEntryRequest>(), Arg.Any<CancellationToken>())
            .Returns(_playlistEntryMovedDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(_movePlaylistEntryRequestFixture.Create(), CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<PlaylistEntryMovedDto, MovePlaylistEntryRequest>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{ZERO_GUID}/entries/{ZERO_GUID}/move"),
            Arg.Any<MovePlaylistEntryRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreMissing_ShouldForwardTheEmptyGuids()
    {
        // Arrange
        _mockApiHttpClient.PutAsync<PlaylistEntryMovedDto, MovePlaylistEntryRequest>(Arg.Any<string>(), Arg.Any<MovePlaylistEntryRequest>(), Arg.Any<CancellationToken>())
            .Returns(_playlistEntryMovedDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(_movePlaylistEntryRequestFixture.Create(), CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<PlaylistEntryMovedDto, MovePlaylistEntryRequest>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{ZERO_GUID}/entries/{ZERO_GUID}/move"),
            Arg.Any<MovePlaylistEntryRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("1;DROP TABLE")] // destructive injection
    [InlineData("../../admin")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    [InlineData("𝕡𝕝𝕒𝕪𝕝𝕚𝕤𝕥")] // unicode payload
    public async Task ExecuteAsync_WhenRouteValuesAreCrafted_ShouldNormalizeThemAndNeverForwardTheRawValues(string payload)
    {
        // Arrange
        SetRouteValues(payload, payload);
        _mockApiHttpClient.PutAsync<PlaylistEntryMovedDto, MovePlaylistEntryRequest>(Arg.Any<string>(), Arg.Any<MovePlaylistEntryRequest>(), Arg.Any<CancellationToken>())
            .Returns(_playlistEntryMovedDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(_movePlaylistEntryRequestFixture.Create(), CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<PlaylistEntryMovedDto, MovePlaylistEntryRequest>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{ZERO_GUID}/entries/{ZERO_GUID}/move"),
            Arg.Any<MovePlaylistEntryRequest>(),
            Arg.Any<CancellationToken>());
        await _mockApiHttpClient.DidNotReceive().PutAsync<PlaylistEntryMovedDto, MovePlaylistEntryRequest>(
            Arg.Is<string>(endpoint => endpoint.Contains(payload, StringComparison.Ordinal)),
            Arg.Any<MovePlaylistEntryRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        MovePlaylistEntryRequest request = _movePlaylistEntryRequestFixture.Create();
        PlaylistEntryMovedDto expectedEntry = _playlistEntryMovedDtoFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
        SetRouteValues(s_playlistId, s_playlistEntryId);
        _mockApiHttpClient.PutAsync<PlaylistEntryMovedDto, MovePlaylistEntryRequest>(Arg.Any<string>(), Arg.Any<MovePlaylistEntryRequest>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.ArgAt<CancellationToken>(2).ThrowIfCancellationRequested();
                return expectedEntry;
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
    /// Sets the route values of the playlist and playlist entry identifier route segments.
    /// </summary>
    /// <param name="playlistId">The value of the playlist identifier route segment.</param>
    /// <param name="playlistEntryId">The value of the playlist entry identifier route segment.</param>
    private void SetRouteValues(object playlistId, object playlistEntryId)
    {
        _sut.HttpContext.Request.RouteValues["playlistId"] = playlistId;
        _sut.HttpContext.Request.RouteValues["playlistEntryId"] = playlistEntryId;
    }
}
