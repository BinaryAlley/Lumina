#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Enums.Playlists;
using Lumina.Presentation.Web.Core.Endpoints.Playlists.GetPlaylist;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Playlists;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Playlists;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Playlists.GetPlaylist;

/// <summary>
/// Contains unit tests for the <see cref="GetPlaylistEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaylistEndpointTests
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private static readonly Guid s_playlistId = Guid.Parse("ABCDEFAB-CDEF-ABCD-EFAB-CDEFABCDEFAB");

    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetPlaylistEndpoint _sut;
    private readonly GetPlaylistRequestFixture _getPlaylistRequestFixture = new();
    private readonly PlaylistDtoFixture _playlistDtoFixture = new();
    private readonly PlaylistEntryDtoFixture _playlistEntryDtoFixture = new();
    private readonly PaginatedDtoFixture<PlaylistEntryDto> _paginatedDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaylistEndpointTests"/> class.
    /// </summary>
    public GetPlaylistEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetPlaylistEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiSucceeds_ShouldReturnSuccessJsonWithPlaylistAndPaginationMetadata()
    {
        // Arrange
        PlaylistEntryDto expectedEntry = _playlistEntryDtoFixture.Create(
            position: 0,
            mediaItemTitle: "First track",
            mediaItemType: PlaylistMediaItemType.MusicTrack);
        PlaylistDto expectedPlaylist = _playlistDtoFixture.Create(
            entries: _paginatedDtoFixture.Create(
                data: [expectedEntry],
                currentPage: 2,
                perPage: 10,
                count: 42,
                numberOfPages: 5));
        SetRoutePlaylistId(s_playlistId);
        _mockApiHttpClient.GetAsync<PlaylistDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedPlaylist);

        // Act
        IResult result = await _sut.ExecuteAsync(_getPlaylistRequestFixture.Create(), CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        JsonSerializerOptions jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
        PlaylistDto actualPlaylist = JsonSerializer.Deserialize<PlaylistDto>(
            jsonDocument.RootElement.GetProperty("data").GetRawText(),
            jsonOptions)!;
        Assert.Equal(expectedPlaylist.Id, actualPlaylist.Id);
        Assert.Equal(expectedPlaylist.Name, actualPlaylist.Name);
        Assert.Equal(expectedPlaylist.Description, actualPlaylist.Description);
        Assert.Equal(expectedPlaylist.Type, actualPlaylist.Type);
        Assert.Equal(expectedPlaylist.CreatedOnUtc, actualPlaylist.CreatedOnUtc);
        Assert.Equal(expectedPlaylist.UpdatedOnUtc, actualPlaylist.UpdatedOnUtc);
        Assert.Equal(expectedPlaylist.Entries.CurrentPage, actualPlaylist.Entries.CurrentPage);
        Assert.Equal(expectedPlaylist.Entries.PerPage, actualPlaylist.Entries.PerPage);
        Assert.Equal(expectedPlaylist.Entries.Count, actualPlaylist.Entries.Count);
        Assert.Equal(expectedPlaylist.Entries.NumberOfPages, actualPlaylist.Entries.NumberOfPages);
        PlaylistEntryDto actualEntry = Assert.Single(actualPlaylist.Entries.Data);
        Assert.Equal(expectedEntry.Id, actualEntry.Id);
        Assert.Equal(expectedEntry.MediaItemId, actualEntry.MediaItemId);
        Assert.Equal(expectedEntry.MediaItemType, actualEntry.MediaItemType);
        Assert.Equal(expectedEntry.Position, actualEntry.Position);
        Assert.Equal(expectedEntry.MediaItemTitle, actualEntry.MediaItemTitle);
        Assert.Equal(expectedEntry.CollectionTitle, actualEntry.CollectionTitle);
        Assert.Equal(expectedEntry.CreatorName, actualEntry.CreatorName);
        Assert.Equal(expectedEntry.DurationInSeconds, actualEntry.DurationInSeconds);
        Assert.Equal(expectedEntry.CoverPath, actualEntry.CoverPath);
    }

    [Fact]
    public async Task ExecuteAsync_WhenBothPageParametersAreMissing_ShouldRequestTheFirstPageWithTheDefaultSize()
    {
        // Arrange
        SetRoutePlaylistId(s_playlistId);
        _mockApiHttpClient.GetAsync<PlaylistDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_playlistDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(
            _getPlaylistRequestFixture.Create(includeCurrentPage: false, includePerPage: false),
            CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PlaylistDto>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{s_playlistId}?currentPage=1&perPage=200"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyCurrentPageIsProvided_ShouldUseTheProvidedPageAndTheDefaultSize()
    {
        // Arrange
        SetRoutePlaylistId(s_playlistId);
        _mockApiHttpClient.GetAsync<PlaylistDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_playlistDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(
            _getPlaylistRequestFixture.Create(currentPage: 3, includePerPage: false),
            CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PlaylistDto>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{s_playlistId}?currentPage=3&perPage=200"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyPerPageIsProvided_ShouldUseTheFirstPageAndTheProvidedSize()
    {
        // Arrange
        SetRoutePlaylistId(s_playlistId);
        _mockApiHttpClient.GetAsync<PlaylistDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_playlistDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(
            _getPlaylistRequestFixture.Create(perPage: 50, includeCurrentPage: false),
            CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PlaylistDto>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{s_playlistId}?currentPage=1&perPage=50"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenBothPageParametersAreProvided_ShouldPreserveBoth()
    {
        // Arrange
        SetRoutePlaylistId(s_playlistId);
        _mockApiHttpClient.GetAsync<PlaylistDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_playlistDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(
            _getPlaylistRequestFixture.Create(currentPage: 4, perPage: 25),
            CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PlaylistDto>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{s_playlistId}?currentPage=4&perPage=25"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlaylistIdIsAnUppercaseGuid_ShouldForwardItInLowercase()
    {
        // Arrange
        SetRoutePlaylistId(s_playlistId.ToString().ToUpperInvariant());
        _mockApiHttpClient.GetAsync<PlaylistDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_playlistDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(
            _getPlaylistRequestFixture.Create(includeCurrentPage: false, includePerPage: false),
            CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PlaylistDto>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{s_playlistId}?currentPage=1&perPage=200"),
            Arg.Any<CancellationToken>());
        await _mockApiHttpClient.DidNotReceive().GetAsync<PlaylistDto>(
            Arg.Is<string>(endpoint => endpoint.Contains(s_playlistId.ToString().ToUpperInvariant(), StringComparison.Ordinal)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlaylistIdIsNotAGuid_ShouldForwardTheEmptyGuid()
    {
        // Arrange
        SetRoutePlaylistId("not-a-guid");
        _mockApiHttpClient.GetAsync<PlaylistDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_playlistDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(
            _getPlaylistRequestFixture.Create(includeCurrentPage: false, includePerPage: false),
            CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PlaylistDto>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{ZERO_GUID}?currentPage=1&perPage=200"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlaylistIdRouteValueIsMissing_ShouldForwardTheEmptyGuid()
    {
        // Arrange
        _mockApiHttpClient.GetAsync<PlaylistDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_playlistDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(
            _getPlaylistRequestFixture.Create(includeCurrentPage: false, includePerPage: false),
            CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PlaylistDto>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{ZERO_GUID}?currentPage=1&perPage=200"),
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
        _mockApiHttpClient.GetAsync<PlaylistDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_playlistDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(
            _getPlaylistRequestFixture.Create(includeCurrentPage: false, includePerPage: false),
            CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PlaylistDto>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/playlists/{ZERO_GUID}?currentPage=1&perPage=200"),
            Arg.Any<CancellationToken>());
        await _mockApiHttpClient.DidNotReceive().GetAsync<PlaylistDto>(
            Arg.Is<string>(endpoint => endpoint.Contains(payload, StringComparison.Ordinal)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSetPrivateNoStoreAndCookieVaryHeaders()
    {
        // Arrange
        SetRoutePlaylistId(s_playlistId);
        _mockApiHttpClient.GetAsync<PlaylistDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_playlistDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(_getPlaylistRequestFixture.Create(), CancellationToken.None);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("Cookie", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        PlaylistDto expectedPlaylist = _playlistDtoFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
        SetRoutePlaylistId(s_playlistId);
        _mockApiHttpClient.GetAsync<PlaylistDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.ArgAt<CancellationToken>(1).ThrowIfCancellationRequested();
                return expectedPlaylist;
            }, callInfo.ArgAt<CancellationToken>(1)));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(_getPlaylistRequestFixture.Create(), cts.Token);
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
