#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Core.Endpoints.Playlists.RemovePlaylistEntry;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Playlists.RemovePlaylistEntry;

/// <summary>
/// Contains unit tests for the <see cref="RemovePlaylistEntryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RemovePlaylistEntryEndpointTests
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private static readonly Guid s_playlistId = Guid.Parse("ABCDEFAB-CDEF-ABCD-EFAB-CDEFABCDEFAB");
    private static readonly Guid s_playlistEntryId = Guid.Parse("FEDCBAFE-DCBA-FEDC-BAFE-DCBAFEDCBAFE");

    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly RemovePlaylistEntryEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemovePlaylistEntryEndpointTests"/> class.
    /// </summary>
    public RemovePlaylistEntryEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<RemovePlaylistEntryEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiSucceeds_ShouldReturnSuccessJsonWithoutADataPayload()
    {
        // Arrange
        SetRouteValues(s_playlistId, s_playlistEntryId);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.False(jsonDocument.RootElement.TryGetProperty("data", out _));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldDeleteTheDocumentedEndpointOnce()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        Guid playlistEntryId = Guid.NewGuid();
        SetRouteValues(playlistId, playlistEntryId);

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).DeleteAsync(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{playlistId}/entries/{playlistEntryId}"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreUppercaseGuids_ShouldForwardThemInLowercase()
    {
        // Arrange
        SetRouteValues(s_playlistId.ToString().ToUpperInvariant(), s_playlistEntryId.ToString().ToUpperInvariant());

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).DeleteAsync(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{s_playlistId}/entries/{s_playlistEntryId}"),
            Arg.Any<CancellationToken>());
        await _mockApiHttpClient.DidNotReceive().DeleteAsync(
            Arg.Is<string>(endpoint => endpoint.Contains(s_playlistId.ToString().ToUpperInvariant(), StringComparison.Ordinal)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreNotGuids_ShouldForwardTheEmptyGuids()
    {
        // Arrange
        SetRouteValues("not-a-guid", "not-an-entry-guid");

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).DeleteAsync(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{ZERO_GUID}/entries/{ZERO_GUID}"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreMissing_ShouldForwardTheEmptyGuids()
    {
        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).DeleteAsync(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{ZERO_GUID}/entries/{ZERO_GUID}"),
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

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).DeleteAsync(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{ZERO_GUID}/entries/{ZERO_GUID}"),
            Arg.Any<CancellationToken>());
        await _mockApiHttpClient.DidNotReceive().DeleteAsync(
            Arg.Is<string>(endpoint => endpoint.Contains(payload, StringComparison.Ordinal)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
        SetRouteValues(s_playlistId, s_playlistEntryId);
        _mockApiHttpClient.DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.ArgAt<CancellationToken>(1).ThrowIfCancellationRequested();
            }, callInfo.ArgAt<CancellationToken>(1)));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(EmptyRequest.Instance, cts.Token);
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
