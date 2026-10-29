#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Requests.Playlists;
using Lumina.Presentation.Web.Core.Endpoints.Playlists.AddPlaylistEntry;
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

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Playlists.AddPlaylistEntry;

/// <summary>
/// Contains unit tests for the <see cref="AddPlaylistEntryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistEntryEndpointTests
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private static readonly Guid s_playlistId = Guid.Parse("ABCDEFAB-CDEF-ABCD-EFAB-CDEFABCDEFAB");
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly AddPlaylistEntryEndpoint _sut;
    private readonly AddPlaylistEntryRequestFixture _addPlaylistEntryRequestFixture = new();
    private readonly PlaylistEntryAddedDtoFixture _playlistEntryAddedDtoFixture = new();
    private readonly PlaylistEntryDtoFixture _playlistEntryDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistEntryEndpointTests"/> class.
    /// </summary>
    public AddPlaylistEntryEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<AddPlaylistEntryEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiSucceeds_ShouldReturnSuccessJsonWithTheAddedEntry()
    {
        // Arrange
        PlaylistEntryDto expectedAddedEntry = _playlistEntryDtoFixture.Create(
            position: 2,
            mediaItemTitle: "Added track");
        PlaylistEntryAddedDto expectedEntry = _playlistEntryAddedDtoFixture.Create(
            playlistId: Guid.NewGuid(),
            entry: expectedAddedEntry);
        SetRoutePlaylistId(s_playlistId);
        _mockApiHttpClient.PostAsync<PlaylistEntryAddedDto, AddPlaylistEntryRequest>(Arg.Any<string>(), Arg.Any<AddPlaylistEntryRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedEntry);

        // Act
        IResult result = await _sut.ExecuteAsync(_addPlaylistEntryRequestFixture.Create(), CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        PlaylistEntryAddedDto actualEntry = JsonSerializer.Deserialize<PlaylistEntryAddedDto>(
            jsonDocument.RootElement.GetProperty("data").GetRawText(), s_jsonOptions)!;
        Assert.Equal(expectedEntry.PlaylistId, actualEntry.PlaylistId);
        Assert.Equal(expectedEntry.UpdatedOnUtc, actualEntry.UpdatedOnUtc);
        Assert.Equal(expectedEntry.Entry, actualEntry.Entry);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldPostTheRequestPayloadOnceToTheDocumentedEndpoint()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create();
        SetRoutePlaylistId(playlistId);
        _mockApiHttpClient.PostAsync<PlaylistEntryAddedDto, AddPlaylistEntryRequest>(Arg.Any<string>(), Arg.Any<AddPlaylistEntryRequest>(), Arg.Any<CancellationToken>())
            .Returns(_playlistEntryAddedDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<PlaylistEntryAddedDto, AddPlaylistEntryRequest>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{playlistId}/entries"),
            Arg.Is<AddPlaylistEntryRequest>(payload => payload.TrackId == request.TrackId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlaylistIdIsAnUppercaseGuid_ShouldForwardItInLowercase()
    {
        // Arrange
        SetRoutePlaylistId(s_playlistId.ToString().ToUpperInvariant());
        _mockApiHttpClient.PostAsync<PlaylistEntryAddedDto, AddPlaylistEntryRequest>(Arg.Any<string>(), Arg.Any<AddPlaylistEntryRequest>(), Arg.Any<CancellationToken>())
            .Returns(_playlistEntryAddedDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(_addPlaylistEntryRequestFixture.Create(), CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<PlaylistEntryAddedDto, AddPlaylistEntryRequest>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{s_playlistId}/entries"),
            Arg.Any<AddPlaylistEntryRequest>(),
            Arg.Any<CancellationToken>());
        await _mockApiHttpClient.DidNotReceive().PostAsync<PlaylistEntryAddedDto, AddPlaylistEntryRequest>(
            Arg.Is<string>(endpoint => endpoint.Contains(s_playlistId.ToString().ToUpperInvariant(), StringComparison.Ordinal)),
            Arg.Any<AddPlaylistEntryRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlaylistIdIsNotAGuid_ShouldForwardTheEmptyGuid()
    {
        // Arrange
        SetRoutePlaylistId("not-a-guid");
        _mockApiHttpClient.PostAsync<PlaylistEntryAddedDto, AddPlaylistEntryRequest>(Arg.Any<string>(), Arg.Any<AddPlaylistEntryRequest>(), Arg.Any<CancellationToken>())
            .Returns(_playlistEntryAddedDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(_addPlaylistEntryRequestFixture.Create(), CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<PlaylistEntryAddedDto, AddPlaylistEntryRequest>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{ZERO_GUID}/entries"),
            Arg.Any<AddPlaylistEntryRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlaylistIdRouteValueIsMissing_ShouldForwardTheEmptyGuid()
    {
        // Arrange
        _mockApiHttpClient.PostAsync<PlaylistEntryAddedDto, AddPlaylistEntryRequest>(Arg.Any<string>(), Arg.Any<AddPlaylistEntryRequest>(), Arg.Any<CancellationToken>())
            .Returns(_playlistEntryAddedDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(_addPlaylistEntryRequestFixture.Create(), CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<PlaylistEntryAddedDto, AddPlaylistEntryRequest>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{ZERO_GUID}/entries"),
            Arg.Any<AddPlaylistEntryRequest>(),
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
        _mockApiHttpClient.PostAsync<PlaylistEntryAddedDto, AddPlaylistEntryRequest>(Arg.Any<string>(), Arg.Any<AddPlaylistEntryRequest>(), Arg.Any<CancellationToken>())
            .Returns(_playlistEntryAddedDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(_addPlaylistEntryRequestFixture.Create(), CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<PlaylistEntryAddedDto, AddPlaylistEntryRequest>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{ZERO_GUID}/entries"),
            Arg.Any<AddPlaylistEntryRequest>(),
            Arg.Any<CancellationToken>());
        await _mockApiHttpClient.DidNotReceive().PostAsync<PlaylistEntryAddedDto, AddPlaylistEntryRequest>(
            Arg.Is<string>(endpoint => endpoint.Contains(payload, StringComparison.Ordinal)),
            Arg.Any<AddPlaylistEntryRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create();
        PlaylistEntryAddedDto expectedEntry = _playlistEntryAddedDtoFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
        SetRoutePlaylistId(s_playlistId);
        _mockApiHttpClient.PostAsync<PlaylistEntryAddedDto, AddPlaylistEntryRequest>(Arg.Any<string>(), Arg.Any<AddPlaylistEntryRequest>(), Arg.Any<CancellationToken>())
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
    /// Sets the route value of the playlist identifier route segment.
    /// </summary>
    /// <param name="playlistId">The value of the playlist identifier route segment.</param>
    private void SetRoutePlaylistId(object playlistId)
    {
        _sut.HttpContext.Request.RouteValues["playlistId"] = playlistId;
    }
}
