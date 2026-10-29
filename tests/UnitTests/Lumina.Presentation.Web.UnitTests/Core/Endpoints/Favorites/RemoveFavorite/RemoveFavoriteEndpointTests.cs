#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Core.Endpoints.Favorites.RemoveFavorite;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Favorites.RemoveFavorite;

/// <summary>
/// Contains unit tests for the <see cref="RemoveFavoriteEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RemoveFavoriteEndpointTests
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private static readonly Guid s_mediaItemId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly RemoveFavoriteEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveFavoriteEndpointTests"/> class.
    /// </summary>
    public RemoveFavoriteEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<RemoveFavoriteEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiSucceeds_ShouldReturnSuccessJsonWithoutPayload()
    {
        // Arrange
        SetRouteValues("MusicTrack", s_mediaItemId.ToString());

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.False(jsonDocument.RootElement.TryGetProperty("data", out JsonElement _));
        await _mockApiHttpClient.Received(1).DeleteAsync(
            Arg.Is<string>(endpoint => endpoint == $"users/me/favorites/MusicTrack/{s_mediaItemId}"),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("MusicArtist", "MusicArtist")] // canonical member name
    [InlineData("MusicAlbum", "MusicAlbum")] // another canonical member name
    [InlineData("MusicTrack", "MusicTrack")] // another canonical member name
    [InlineData("Book", "Book")] // another canonical member name
    [InlineData("musictrack", "MusicTrack")] // lowercase member name
    [InlineData("MUSICTRACK", "MusicTrack")] // uppercase member name
    [InlineData("musicTrack", "MusicTrack")] // mixed case member name
    public async Task ExecuteAsync_WhenMediaKindNameIsValid_ShouldForwardTheCanonicalMemberName(string routeMediaKind, string expectedMediaKind)
    {
        // Arrange
        SetRouteValues(routeMediaKind, s_mediaItemId.ToString());

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).DeleteAsync(
            Arg.Is<string>(endpoint => endpoint == $"users/me/favorites/{expectedMediaKind}/{s_mediaItemId}"),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("1")] // numeric enum value
    [InlineData("0")] // numeric enum value
    [InlineData("NotAFavoriteMediaKind")] // undefined member name
    [InlineData("None")] // the default value is not a kind that can be favorited
    [InlineData("none")] // the default value, matched case insensitively
    [InlineData("MusicTrack,Book")] // comma separated member names
    [InlineData("Music Track")] // member name with a space
    [InlineData("MusicTrack/../..")] // path traversal attempt
    [InlineData("..")] // dot segment
    [InlineData("MusicTrack%2F..")] // encoded slash traversal attempt
    public async Task ExecuteAsync_WhenMediaKindNameIsRejected_ShouldForwardTheInvalidPlaceholder(string routeMediaKind)
    {
        // Arrange
        SetRouteValues(routeMediaKind, s_mediaItemId.ToString());

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).DeleteAsync(
            Arg.Is<string>(endpoint => endpoint == $"users/me/favorites/Invalid/{s_mediaItemId}"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediaItemIdIsAnUppercaseGuid_ShouldForwardItLowercase()
    {
        // Arrange
        string uppercaseMediaItemId = s_mediaItemId.ToString().ToUpperInvariant();
        SetRouteValues("MusicTrack", uppercaseMediaItemId);

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).DeleteAsync(
            Arg.Is<string>(endpoint => endpoint == $"users/me/favorites/MusicTrack/{s_mediaItemId}"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediaItemIdIsNotAGuid_ShouldForwardTheEmptyGuid()
    {
        // Arrange
        SetRouteValues("MusicTrack", "not-a-guid");

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).DeleteAsync(
            Arg.Is<string>(endpoint => endpoint == $"users/me/favorites/MusicTrack/{ZERO_GUID}"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreMissing_ShouldForwardTheInvalidPlaceholderAndTheEmptyGuid()
    {
        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).DeleteAsync(
            Arg.Is<string>(endpoint => endpoint == $"users/me/favorites/Invalid/{ZERO_GUID}"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        SetRouteValues("MusicTrack", s_mediaItemId.ToString());
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
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
    /// Sets the route values of the endpoint under test.
    /// </summary>
    /// <param name="mediaKind">The value of the media kind route segment.</param>
    /// <param name="mediaItemId">The value of the media item identifier route segment.</param>
    private void SetRouteValues(object mediaKind, object mediaItemId)
    {
        _sut.HttpContext.Request.RouteValues["mediaKind"] = mediaKind;
        _sut.HttpContext.Request.RouteValues["mediaItemId"] = mediaItemId;
    }
}
