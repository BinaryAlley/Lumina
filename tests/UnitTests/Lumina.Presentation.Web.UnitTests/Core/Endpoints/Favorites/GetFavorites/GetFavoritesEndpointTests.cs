#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Favorites;
using Lumina.Presentation.Web.Common.Enums.Favorites;
using Lumina.Presentation.Web.Common.Requests.Favorites;
using Lumina.Presentation.Web.Core.Endpoints.Favorites.GetFavorites;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
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

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Favorites.GetFavorites;

/// <summary>
/// Contains unit tests for the <see cref="GetFavoritesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFavoritesEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetFavoritesEndpoint _sut;
    private readonly GetFavoritesRequestFixture _getFavoritesRequestFixture = new();
    private readonly PaginatedDtoFixture<FavoriteItemDto> _paginatedDtoFixture = new();
    private readonly FavoriteItemDtoFixture _favoriteItemDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFavoritesEndpointTests"/> class.
    /// </summary>
    public GetFavoritesEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetFavoritesEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsFavorites_ShouldReturnSuccessJsonWithPaginatedFavorites()
    {
        // Arrange
        FavoriteItemDto expectedItem = _favoriteItemDtoFixture.Create(
            mediaKind: FavoriteMediaKind.MusicTrack,
            isAvailable: true,
            title: "Time",
            collectionTitle: "The Dark Side of the Moon",
            creatorName: "Pink Floyd",
            coverPath: "covers/dark-side-of-the-moon.jpg");
        PaginatedDto<FavoriteItemDto> expectedPage = _paginatedDtoFixture.Create(
            data: [expectedItem],
            currentPage: 2,
            perPage: 50,
            count: 51,
            numberOfPages: 2);
        _mockApiHttpClient.GetAsync<PaginatedDto<FavoriteItemDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedPage);

        // Act
        IResult result = await _sut.ExecuteAsync(_getFavoritesRequestFixture.Create(), CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = jsonDocument.RootElement.GetProperty("data");
        Assert.Equal(2, data.GetProperty("currentPage").GetInt32());
        Assert.Equal(50, data.GetProperty("perPage").GetInt32());
        Assert.Equal(51, data.GetProperty("count").GetInt32());
        Assert.Equal(2, data.GetProperty("numberOfPages").GetInt32());
        JsonElement item = Assert.Single(data.GetProperty("data").EnumerateArray());
        Assert.Equal((int)expectedItem.MediaKind, item.GetProperty("mediaKind").GetInt32());
        Assert.Equal(expectedItem.MediaItemId, item.GetProperty("mediaItemId").GetGuid());
        Assert.True(item.GetProperty("isAvailable").GetBoolean());
        Assert.Equal(expectedItem.LibraryId, item.GetProperty("libraryId").GetGuid());
        Assert.Equal(expectedItem.Title, item.GetProperty("title").GetString());
        Assert.Equal(expectedItem.CollectionId, item.GetProperty("collectionId").GetGuid());
        Assert.Equal(expectedItem.CollectionTitle, item.GetProperty("collectionTitle").GetString());
        Assert.Equal(expectedItem.CreatorId, item.GetProperty("creatorId").GetGuid());
        Assert.Equal(expectedItem.CreatorName, item.GetProperty("creatorName").GetString());
        Assert.Equal(expectedItem.CoverPath, item.GetProperty("coverPath").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPageAndSizeAreOmitted_ShouldRequestTheDefaultFirstPage()
    {
        // Arrange
        GetFavoritesRequest request = _getFavoritesRequestFixture.Create(includeCurrentPage: false, includePerPage: false, includeMediaKind: false);
        _mockApiHttpClient.GetAsync<PaginatedDto<FavoriteItemDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_paginatedDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PaginatedDto<FavoriteItemDto>>(
            Arg.Is<string>(endpoint => endpoint == "users/me/favorites?currentPage=1&perPage=200"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyThePageIsSupplied_ShouldForwardItWithTheDefaultPageSize()
    {
        // Arrange
        GetFavoritesRequest request = _getFavoritesRequestFixture.Create(
            currentPage: 3,
            includeMediaKind: false,
            includePerPage: false);
        _mockApiHttpClient.GetAsync<PaginatedDto<FavoriteItemDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_paginatedDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PaginatedDto<FavoriteItemDto>>(
            Arg.Is<string>(endpoint => endpoint == "users/me/favorites?currentPage=3&perPage=200"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyThePageSizeIsSupplied_ShouldForwardItWithTheDefaultFirstPage()
    {
        // Arrange
        GetFavoritesRequest request = _getFavoritesRequestFixture.Create(
            perPage: 25,
            includeMediaKind: false,
            includeCurrentPage: false);
        _mockApiHttpClient.GetAsync<PaginatedDto<FavoriteItemDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_paginatedDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PaginatedDto<FavoriteItemDto>>(
            Arg.Is<string>(endpoint => endpoint == "users/me/favorites?currentPage=1&perPage=25"),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(2, 50, "users/me/favorites?currentPage=2&perPage=50")] // both the page and the size are supplied
    [InlineData(-1, 0, "users/me/favorites?currentPage=-1&perPage=0")] // out of range values are preserved, so the API reports its own validation
    public async Task ExecuteAsync_WhenPaginationValuesAreSupplied_ShouldForwardTheNormalizedQueryString(int currentPage, int perPage, string expectedEndpoint)
    {
        // Arrange
        GetFavoritesRequest request = _getFavoritesRequestFixture.Create(
            currentPage: currentPage,
            perPage: perPage,
            includeMediaKind: false);
        _mockApiHttpClient.GetAsync<PaginatedDto<FavoriteItemDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_paginatedDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PaginatedDto<FavoriteItemDto>>(
            Arg.Is<string>(endpoint => endpoint == expectedEndpoint),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(FavoriteMediaKind.MusicArtist, "users/me/favorites?currentPage=1&perPage=200&mediaKind=MusicArtist")] // artist favorites
    [InlineData(FavoriteMediaKind.MusicAlbum, "users/me/favorites?currentPage=1&perPage=200&mediaKind=MusicAlbum")] // album favorites
    [InlineData(FavoriteMediaKind.MusicTrack, "users/me/favorites?currentPage=1&perPage=200&mediaKind=MusicTrack")] // track favorites
    [InlineData(FavoriteMediaKind.Book, "users/me/favorites?currentPage=1&perPage=200&mediaKind=Book")] // book favorites
    public async Task ExecuteAsync_WhenMediaKindIsSupplied_ShouldAppendTheCanonicalKindToTheQueryString(FavoriteMediaKind mediaKind, string expectedEndpoint)
    {
        // Arrange
        GetFavoritesRequest request = _getFavoritesRequestFixture.Create(
            mediaKind: mediaKind,
            includeCurrentPage: false,
            includePerPage: false);
        _mockApiHttpClient.GetAsync<PaginatedDto<FavoriteItemDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_paginatedDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PaginatedDto<FavoriteItemDto>>(
            Arg.Is<string>(endpoint => endpoint == expectedEndpoint),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediaKindIsOmitted_ShouldNotAppendAMediaKindToTheQueryString()
    {
        // Arrange
        GetFavoritesRequest request = _getFavoritesRequestFixture.Create(
            includeMediaKind: false,
            includeCurrentPage: false,
            includePerPage: false);
        _mockApiHttpClient.GetAsync<PaginatedDto<FavoriteItemDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_paginatedDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PaginatedDto<FavoriteItemDto>>(
            Arg.Is<string>(endpoint => !endpoint.Contains("mediaKind", StringComparison.Ordinal)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSetPrivateNoStoreAndCookieVaryHeaders()
    {
        // Arrange
        _mockApiHttpClient.GetAsync<PaginatedDto<FavoriteItemDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_paginatedDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(_getFavoritesRequestFixture.Create(), CancellationToken.None);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("Cookie", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        PaginatedDto<FavoriteItemDto> expectedPage = _paginatedDtoFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
        _mockApiHttpClient.GetAsync<PaginatedDto<FavoriteItemDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.ArgAt<CancellationToken>(1).ThrowIfCancellationRequested();
                return expectedPage;
            }, callInfo.ArgAt<CancellationToken>(1)));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(_getFavoritesRequestFixture.Create(), cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
