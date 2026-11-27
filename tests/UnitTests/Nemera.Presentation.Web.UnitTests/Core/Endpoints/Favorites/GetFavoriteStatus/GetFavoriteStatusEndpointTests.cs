#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Favorites;
using Lumina.Presentation.Web.Common.Enums.Favorites;
using Lumina.Presentation.Web.Common.Requests.Favorites;
using Lumina.Presentation.Web.Core.Endpoints.Favorites.GetFavoriteStatus;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Favorites;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Favorites;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Favorites.GetFavoriteStatus;

/// <summary>
/// Contains unit tests for the <see cref="GetFavoriteStatusEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFavoriteStatusEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetFavoriteStatusEndpoint _sut;
    private readonly GetFavoriteStatusRequestFixture _getFavoriteStatusRequestFixture = new();
    private readonly FavoriteStatusDtoFixture _favoriteStatusDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFavoriteStatusEndpointTests"/> class.
    /// </summary>
    public GetFavoriteStatusEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetFavoriteStatusEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiSucceeds_ShouldReturnSuccessJsonWithTheFavoritedMediaItemIds()
    {
        // Arrange
        List<Guid> expectedIds = [Guid.NewGuid(), Guid.NewGuid()];
        GetFavoriteStatusRequest request = _getFavoriteStatusRequestFixture.Create(
            mediaKind: FavoriteMediaKind.MusicTrack,
            mediaItemIds: expectedIds);
        _mockApiHttpClient.GetAsync<FavoriteStatusDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_favoriteStatusDtoFixture.Create(favoritedMediaItemIds: expectedIds));

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = jsonDocument.RootElement.GetProperty("data");
        List<Guid> actualIds = [.. data.GetProperty("favoritedMediaItemIds").EnumerateArray().Select(element => element.GetGuid())];
        Assert.Equal(expectedIds, actualIds);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediaKindAndIdsAreSupplied_ShouldForwardTheDocumentedQueryString()
    {
        // Arrange
        List<Guid> mediaItemIds = [Guid.NewGuid(), Guid.NewGuid()];
        GetFavoriteStatusRequest request = _getFavoriteStatusRequestFixture.Create(
            mediaKind: FavoriteMediaKind.MusicTrack,
            mediaItemIds: mediaItemIds);
        string expectedEndpoint = $"users/me/favorites/status?mediaKind=MusicTrack&mediaItemIds={mediaItemIds[0]}&mediaItemIds={mediaItemIds[1]}";
        _mockApiHttpClient.GetAsync<FavoriteStatusDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_favoriteStatusDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<FavoriteStatusDto>(
            Arg.Is<string>(endpoint => endpoint == expectedEndpoint),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediaKindAndIdsAreOmitted_ShouldForwardTheBareStatusRoute()
    {
        // Arrange
        GetFavoriteStatusRequest request = _getFavoriteStatusRequestFixture.Create(
            includeMediaKind: false,
            includeMediaItemIds: false);
        _mockApiHttpClient.GetAsync<FavoriteStatusDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_favoriteStatusDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<FavoriteStatusDto>(
            Arg.Is<string>(endpoint => endpoint == "users/me/favorites/status"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediaKindIsOmitted_ShouldForwardOnlyTheIds()
    {
        // Arrange
        List<Guid> mediaItemIds = [Guid.NewGuid()];
        GetFavoriteStatusRequest request = _getFavoriteStatusRequestFixture.Create(
            mediaItemIds: mediaItemIds,
            includeMediaKind: false);
        _mockApiHttpClient.GetAsync<FavoriteStatusDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_favoriteStatusDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<FavoriteStatusDto>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/favorites/status?mediaItemIds={mediaItemIds[0]}"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenIdsAreOmitted_ShouldForwardOnlyTheMediaKind()
    {
        // Arrange
        GetFavoriteStatusRequest request = _getFavoriteStatusRequestFixture.Create(
            mediaKind: FavoriteMediaKind.Book,
            includeMediaItemIds: false);
        _mockApiHttpClient.GetAsync<FavoriteStatusDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_favoriteStatusDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<FavoriteStatusDto>(
            Arg.Is<string>(endpoint => endpoint == "users/me/favorites/status?mediaKind=Book"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenMoreThanTheMaximumIdsAreSupplied_ShouldForwardOnlyTheFirstOneHundredAndOne()
    {
        // Arrange
        List<Guid> mediaItemIds = [.. Enumerable.Range(1, 102).Select(index => new Guid(index, 0, 0, new byte[8]))];
        GetFavoriteStatusRequest request = _getFavoriteStatusRequestFixture.Create(
            mediaKind: FavoriteMediaKind.MusicTrack,
            mediaItemIds: mediaItemIds);
        string expectedIdsQuery = string.Join('&', mediaItemIds.Take(101).Select(mediaItemId => $"mediaItemIds={mediaItemId}"));
        string expectedEndpoint = $"users/me/favorites/status?mediaKind=MusicTrack&{expectedIdsQuery}";
        _mockApiHttpClient.GetAsync<FavoriteStatusDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_favoriteStatusDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<FavoriteStatusDto>(
            Arg.Is<string>(endpoint => endpoint == expectedEndpoint),
            Arg.Any<CancellationToken>());
        await _mockApiHttpClient.Received(1).GetAsync<FavoriteStatusDto>(
            Arg.Is<string>(endpoint => !endpoint.Contains(mediaItemIds[101].ToString(), StringComparison.Ordinal)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSetPrivateNoStoreAndCookieVaryHeaders()
    {
        // Arrange
        _mockApiHttpClient.GetAsync<FavoriteStatusDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_favoriteStatusDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(_getFavoriteStatusRequestFixture.Create(), CancellationToken.None);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("Cookie", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        FavoriteStatusDto expectedStatus = _favoriteStatusDtoFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
        _mockApiHttpClient.GetAsync<FavoriteStatusDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.ArgAt<CancellationToken>(1).ThrowIfCancellationRequested();
                return expectedStatus;
            }, callInfo.ArgAt<CancellationToken>(1)));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(_getFavoriteStatusRequestFixture.Create(), cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
