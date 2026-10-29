#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Requests.Playlists;
using Lumina.Presentation.Web.Core.Endpoints.Playlists.AddPlaylistEntries;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Playlists;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Playlists;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Playlists.AddPlaylistEntries;

/// <summary>
/// Contains unit tests for the <see cref="AddPlaylistEntriesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistEntriesEndpointTests
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private static readonly Guid s_playlistId = Guid.Parse("ABCDEFAB-CDEF-ABCD-EFAB-CDEFABCDEFAB");
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly AddPlaylistEntriesEndpoint _sut;
    private readonly AddPlaylistEntriesRequestFixture _addPlaylistEntriesRequestFixture = new();
    private readonly PlaylistEntriesAddedDtoFixture _playlistEntriesAddedDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistEntriesEndpointTests"/> class.
    /// </summary>
    public AddPlaylistEntriesEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<AddPlaylistEntriesEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiSucceeds_ShouldReturnSuccessJsonWithTheAddedEntriesSummary()
    {
        // Arrange
        PlaylistEntriesAddedDto expectedSummary = _playlistEntriesAddedDtoFixture.Create(
            playlistId: Guid.NewGuid(),
            addedCount: 4);
        SetRoutePlaylistId(s_playlistId);
        _mockApiHttpClient.PostAsync<PlaylistEntriesAddedDto, AddPlaylistEntriesRequest>(Arg.Any<string>(), Arg.Any<AddPlaylistEntriesRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedSummary);

        // Act
        IResult result = await _sut.ExecuteAsync(_addPlaylistEntriesRequestFixture.Create(), CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        PlaylistEntriesAddedDto actualSummary = JsonSerializer.Deserialize<PlaylistEntriesAddedDto>(
            jsonDocument.RootElement.GetProperty("data").GetRawText(), s_jsonOptions)!;
        Assert.Equal(expectedSummary, actualSummary);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldPostTheRequestPayloadOnceToTheDocumentedEndpoint()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        AddPlaylistEntriesRequest request = _addPlaylistEntriesRequestFixture.Create();
        SetRoutePlaylistId(playlistId);
        _mockApiHttpClient.PostAsync<PlaylistEntriesAddedDto, AddPlaylistEntriesRequest>(Arg.Any<string>(), Arg.Any<AddPlaylistEntriesRequest>(), Arg.Any<CancellationToken>())
            .Returns(_playlistEntriesAddedDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<PlaylistEntriesAddedDto, AddPlaylistEntriesRequest>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{playlistId}/entries-batch"),
            Arg.Is<AddPlaylistEntriesRequest>(payload => payload.TrackIds != null && payload.TrackIds.SequenceEqual(request.TrackIds!)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlaylistIdIsAnUppercaseGuid_ShouldForwardItInLowercase()
    {
        // Arrange
        SetRoutePlaylistId(s_playlistId.ToString().ToUpperInvariant());
        _mockApiHttpClient.PostAsync<PlaylistEntriesAddedDto, AddPlaylistEntriesRequest>(Arg.Any<string>(), Arg.Any<AddPlaylistEntriesRequest>(), Arg.Any<CancellationToken>())
            .Returns(_playlistEntriesAddedDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(_addPlaylistEntriesRequestFixture.Create(), CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<PlaylistEntriesAddedDto, AddPlaylistEntriesRequest>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{s_playlistId}/entries-batch"),
            Arg.Any<AddPlaylistEntriesRequest>(),
            Arg.Any<CancellationToken>());
        await _mockApiHttpClient.DidNotReceive().PostAsync<PlaylistEntriesAddedDto, AddPlaylistEntriesRequest>(
            Arg.Is<string>(endpoint => endpoint.Contains(s_playlistId.ToString().ToUpperInvariant(), StringComparison.Ordinal)),
            Arg.Any<AddPlaylistEntriesRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlaylistIdIsNotAGuid_ShouldForwardTheEmptyGuid()
    {
        // Arrange
        SetRoutePlaylistId("not-a-guid");
        _mockApiHttpClient.PostAsync<PlaylistEntriesAddedDto, AddPlaylistEntriesRequest>(Arg.Any<string>(), Arg.Any<AddPlaylistEntriesRequest>(), Arg.Any<CancellationToken>())
            .Returns(_playlistEntriesAddedDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(_addPlaylistEntriesRequestFixture.Create(), CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<PlaylistEntriesAddedDto, AddPlaylistEntriesRequest>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{ZERO_GUID}/entries-batch"),
            Arg.Any<AddPlaylistEntriesRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlaylistIdRouteValueIsMissing_ShouldForwardTheEmptyGuid()
    {
        // Arrange
        _mockApiHttpClient.PostAsync<PlaylistEntriesAddedDto, AddPlaylistEntriesRequest>(Arg.Any<string>(), Arg.Any<AddPlaylistEntriesRequest>(), Arg.Any<CancellationToken>())
            .Returns(_playlistEntriesAddedDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(_addPlaylistEntriesRequestFixture.Create(), CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<PlaylistEntriesAddedDto, AddPlaylistEntriesRequest>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{ZERO_GUID}/entries-batch"),
            Arg.Any<AddPlaylistEntriesRequest>(),
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
        _mockApiHttpClient.PostAsync<PlaylistEntriesAddedDto, AddPlaylistEntriesRequest>(Arg.Any<string>(), Arg.Any<AddPlaylistEntriesRequest>(), Arg.Any<CancellationToken>())
            .Returns(_playlistEntriesAddedDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(_addPlaylistEntriesRequestFixture.Create(), CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<PlaylistEntriesAddedDto, AddPlaylistEntriesRequest>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{ZERO_GUID}/entries-batch"),
            Arg.Any<AddPlaylistEntriesRequest>(),
            Arg.Any<CancellationToken>());
        await _mockApiHttpClient.DidNotReceive().PostAsync<PlaylistEntriesAddedDto, AddPlaylistEntriesRequest>(
            Arg.Is<string>(endpoint => endpoint.Contains(payload, StringComparison.Ordinal)),
            Arg.Any<AddPlaylistEntriesRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        AddPlaylistEntriesRequest request = _addPlaylistEntriesRequestFixture.Create();
        PlaylistEntriesAddedDto expectedSummary = _playlistEntriesAddedDtoFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
        SetRoutePlaylistId(s_playlistId);
        _mockApiHttpClient.PostAsync<PlaylistEntriesAddedDto, AddPlaylistEntriesRequest>(Arg.Any<string>(), Arg.Any<AddPlaylistEntriesRequest>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.ArgAt<CancellationToken>(2).ThrowIfCancellationRequested();
                return expectedSummary;
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
