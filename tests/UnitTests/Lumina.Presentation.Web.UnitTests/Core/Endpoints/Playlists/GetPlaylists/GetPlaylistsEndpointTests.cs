#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Core.Endpoints.Playlists.GetPlaylists;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Playlists;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Playlists.GetPlaylists;

/// <summary>
/// Contains unit tests for the <see cref="GetPlaylistsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaylistsEndpointTests
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetPlaylistsEndpoint _sut;
    private readonly PlaylistSummaryDtoFixture _playlistSummaryDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaylistsEndpointTests"/> class.
    /// </summary>
    public GetPlaylistsEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetPlaylistsEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsPlaylists_ShouldReturnSuccessJsonWithTheSummaries()
    {
        // Arrange
        PlaylistSummaryDto[] expectedSummaries =
        [
            _playlistSummaryDtoFixture.Create(name: "Morning", entryCount: 12),
            _playlistSummaryDtoFixture.Create(name: "Evening", entryCount: 4)
        ];
        _mockApiHttpClient.GetAsync<PlaylistSummaryDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedSummaries);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        PlaylistSummaryDto[] actualSummaries = JsonSerializer.Deserialize<PlaylistSummaryDto[]>(
            jsonDocument.RootElement.GetProperty("data").GetRawText(), s_jsonOptions)!;
        Assert.Equal(expectedSummaries, actualSummaries);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheUserHasNoPlaylists_ShouldReturnSuccessJsonWithAnEmptyArray()
    {
        // Arrange
        _mockApiHttpClient.GetAsync<PlaylistSummaryDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(0, jsonDocument.RootElement.GetProperty("data").GetArrayLength());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldGetTheDocumentedEndpointOnce()
    {
        // Arrange
        PlaylistSummaryDto[] expectedSummaries = [.. _playlistSummaryDtoFixture.CreateMany()];
        _mockApiHttpClient.GetAsync<PlaylistSummaryDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedSummaries);

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PlaylistSummaryDto[]>(
            Arg.Is<string>(endpoint => endpoint == "users/me/playlists"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSetPrivateNoStoreAndCookieVaryHeaders()
    {
        // Arrange
        PlaylistSummaryDto[] expectedSummaries = [.. _playlistSummaryDtoFixture.CreateMany()];
        _mockApiHttpClient.GetAsync<PlaylistSummaryDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedSummaries);

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("Cookie", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        PlaylistSummaryDto[] expectedSummaries = [.. _playlistSummaryDtoFixture.CreateMany(1)];
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
        _mockApiHttpClient.GetAsync<PlaylistSummaryDto[]>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.ArgAt<CancellationToken>(1).ThrowIfCancellationRequested();
                return expectedSummaries;
            }, callInfo.ArgAt<CancellationToken>(1)));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(EmptyRequest.Instance, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
