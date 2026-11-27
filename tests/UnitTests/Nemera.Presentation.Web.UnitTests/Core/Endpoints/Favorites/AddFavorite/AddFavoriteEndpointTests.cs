#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Favorites;
using Lumina.Presentation.Web.Common.Enums.Favorites;
using Lumina.Presentation.Web.Common.Requests.Favorites;
using Lumina.Presentation.Web.Core.Endpoints.Favorites.AddFavorite;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Favorites;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Favorites;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Favorites.AddFavorite;

/// <summary>
/// Contains unit tests for the <see cref="AddFavoriteEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddFavoriteEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly AddFavoriteEndpoint _sut;
    private readonly AddFavoriteRequestFixture _addFavoriteRequestFixture = new();
    private readonly FavoriteDtoFixture _favoriteDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AddFavoriteEndpointTests"/> class.
    /// </summary>
    public AddFavoriteEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<AddFavoriteEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiSucceeds_ShouldReturnSuccessJsonWithTheCreatedFavorite()
    {
        // Arrange
        AddFavoriteRequest request = _addFavoriteRequestFixture.Create(
            mediaKind: FavoriteMediaKind.MusicTrack,
            mediaItemId: Guid.NewGuid());
        FavoriteDto expectedFavorite = _favoriteDtoFixture.Create(
            mediaKind: request.MediaKind!.Value,
            mediaItemId: request.MediaItemId!.Value);
        _mockApiHttpClient.PostAsync<FavoriteDto, AddFavoriteRequest>(Arg.Any<string>(), Arg.Any<AddFavoriteRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedFavorite);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = jsonDocument.RootElement.GetProperty("data");
        Assert.Equal(expectedFavorite.Id, data.GetProperty("id").GetGuid());
        Assert.Equal((int)expectedFavorite.MediaKind, data.GetProperty("mediaKind").GetInt32());
        Assert.Equal(expectedFavorite.MediaItemId, data.GetProperty("mediaItemId").GetGuid());
        Assert.Equal(expectedFavorite.FavoritedOnUtc, data.GetProperty("favoritedOnUtc").GetDateTime());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldPostTheRequestPayloadOnceToTheDocumentedEndpoint()
    {
        // Arrange
        AddFavoriteRequest request = _addFavoriteRequestFixture.Create(
            mediaKind: FavoriteMediaKind.Book,
            mediaItemId: Guid.NewGuid());
        _mockApiHttpClient.PostAsync<FavoriteDto, AddFavoriteRequest>(Arg.Any<string>(), Arg.Any<AddFavoriteRequest>(), Arg.Any<CancellationToken>())
            .Returns(_favoriteDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<FavoriteDto, AddFavoriteRequest>(
            Arg.Is<string>(endpoint => endpoint == "users/me/favorites"),
            Arg.Is<AddFavoriteRequest>(payload =>
                payload.MediaKind == request.MediaKind &&
                payload.MediaItemId == request.MediaItemId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        AddFavoriteRequest request = _addFavoriteRequestFixture.Create();
        FavoriteDto expectedFavorite = _favoriteDtoFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
        _mockApiHttpClient.PostAsync<FavoriteDto, AddFavoriteRequest>(Arg.Any<string>(), Arg.Any<AddFavoriteRequest>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.ArgAt<CancellationToken>(2).ThrowIfCancellationRequested();
                return expectedFavorite;
            }, callInfo.ArgAt<CancellationToken>(2)));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(request, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
