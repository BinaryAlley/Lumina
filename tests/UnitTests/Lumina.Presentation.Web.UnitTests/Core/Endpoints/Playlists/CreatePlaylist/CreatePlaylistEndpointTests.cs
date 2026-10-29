#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Requests.Playlists;
using Lumina.Presentation.Web.Core.Endpoints.Playlists.CreatePlaylist;
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

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Playlists.CreatePlaylist;

/// <summary>
/// Contains unit tests for the <see cref="CreatePlaylistEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CreatePlaylistEndpointTests
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly CreatePlaylistEndpoint _sut;
    private readonly CreatePlaylistRequestFixture _createPlaylistRequestFixture = new();
    private readonly PlaylistDtoFixture _playlistDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePlaylistEndpointTests"/> class.
    /// </summary>
    public CreatePlaylistEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<CreatePlaylistEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiSucceeds_ShouldReturnSuccessJsonWithTheCreatedPlaylist()
    {
        // Arrange
        PlaylistDto expectedPlaylist = _playlistDtoFixture.Create(
            name: "Road trip",
            description: "Songs for the highway.");
        _mockApiHttpClient.PostAsync<PlaylistDto, CreatePlaylistRequest>(Arg.Any<string>(), Arg.Any<CreatePlaylistRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedPlaylist);

        // Act
        IResult result = await _sut.ExecuteAsync(_createPlaylistRequestFixture.Create(), CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        PlaylistDto actualPlaylist = JsonSerializer.Deserialize<PlaylistDto>(
            jsonDocument.RootElement.GetProperty("data").GetRawText(), s_jsonOptions)!;
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
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldPostTheRequestPayloadOnceToTheDocumentedEndpoint()
    {
        // Arrange
        CreatePlaylistRequest request = _createPlaylistRequestFixture.Create(
            name: "Road trip",
            description: "Songs for the highway.");
        _mockApiHttpClient.PostAsync<PlaylistDto, CreatePlaylistRequest>(Arg.Any<string>(), Arg.Any<CreatePlaylistRequest>(), Arg.Any<CancellationToken>())
            .Returns(_playlistDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<PlaylistDto, CreatePlaylistRequest>(
            Arg.Is<string>(endpoint => endpoint == "users/me/playlists"),
            Arg.Is<CreatePlaylistRequest>(payload =>
                payload.Name == "Road trip" &&
                payload.Description == "Songs for the highway." &&
                payload.Type == request.Type),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOptionalFieldsAreMissing_ShouldPostTheRequestWithNulls()
    {
        // Arrange
        CreatePlaylistRequest request = _createPlaylistRequestFixture.Create(includeDescription: false, includeType: false);
        _mockApiHttpClient.PostAsync<PlaylistDto, CreatePlaylistRequest>(Arg.Any<string>(), Arg.Any<CreatePlaylistRequest>(), Arg.Any<CancellationToken>())
            .Returns(_playlistDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<PlaylistDto, CreatePlaylistRequest>(
            Arg.Any<string>(),
            Arg.Is<CreatePlaylistRequest>(payload =>
                payload.Name == request.Name &&
                payload.Description == null &&
                payload.Type == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CreatePlaylistRequest request = _createPlaylistRequestFixture.Create();
        PlaylistDto expectedPlaylist = _playlistDtoFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
        _mockApiHttpClient.PostAsync<PlaylistDto, CreatePlaylistRequest>(Arg.Any<string>(), Arg.Any<CreatePlaylistRequest>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.ArgAt<CancellationToken>(2).ThrowIfCancellationRequested();
                return expectedPlaylist;
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
