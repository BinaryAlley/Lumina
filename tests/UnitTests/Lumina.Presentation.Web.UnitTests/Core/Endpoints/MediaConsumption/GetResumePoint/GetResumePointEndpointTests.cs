#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Core.Endpoints.MediaConsumption.GetResumePoint;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
using Lumina.Presentation.Web.Fixtures.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.MediaConsumption.GetResumePoint;

/// <summary>
/// Contains unit tests for the <see cref="GetResumePointEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetResumePointEndpointTests
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private static readonly Guid s_resumePointId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid s_libraryId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid s_mediaItemId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetResumePointEndpoint _sut;
    private readonly ResumePointDtoFixture _resumePointDtoFixture = new();
    private readonly MediaPositionDtoFixture _mediaPositionDtoFixture = new();
    private readonly ProblemDetailsDtoFixture _problemDetailsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetResumePointEndpointTests"/> class.
    /// </summary>
    public GetResumePointEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetResumePointEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiSucceeds_ShouldReturnSuccessJsonWithResumePoint()
    {
        // Arrange
        MediaPositionDto expectedPosition = _mediaPositionDtoFixture.Create(
            kind: MediaPositionKind.Location,
            location: "epubcfi(/6/4!/2/2)",
            includeTimeOffsetSeconds: false);
        ResumePointDto expectedResumePoint = _resumePointDtoFixture.Create(
            id: s_resumePointId,
            libraryId: s_libraryId,
            mediaItemId: s_mediaItemId,
            mediaKind: MediaKind.Book,
            lastPosition: expectedPosition,
            lastProgressFraction: 0.42);
        SetRouteValues(nameof(MediaKind.Book), s_mediaItemId);
        _mockApiHttpClient.GetAsync<ResumePointDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedResumePoint);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = jsonDocument.RootElement.GetProperty("data");
        Assert.Equal(expectedResumePoint.Id, data.GetProperty("id").GetGuid());
        Assert.Equal(expectedResumePoint.LibraryId, data.GetProperty("libraryId").GetGuid());
        Assert.Equal(expectedResumePoint.MediaItemId, data.GetProperty("mediaItemId").GetGuid());
        Assert.Equal((int)expectedResumePoint.MediaKind, data.GetProperty("mediaKind").GetInt32());
        JsonElement position = data.GetProperty("lastPosition");
        Assert.Equal((int)expectedPosition.Kind, position.GetProperty("kind").GetInt32());
        Assert.Equal(JsonValueKind.Null, position.GetProperty("timeOffsetSeconds").ValueKind);
        Assert.Equal(expectedPosition.Location, position.GetProperty("location").GetString());
        Assert.Equal(expectedResumePoint.LastProgressFraction, data.GetProperty("lastProgressFraction").GetDouble());
        Assert.Equal(expectedResumePoint.LastConsumedOnUtc, data.GetProperty("lastConsumedOnUtc").GetDateTime());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPositionIsATimeOffset_ShouldSerializeTheTimeOffsetAndANullLocation()
    {
        // Arrange
        MediaPositionDto expectedPosition = _mediaPositionDtoFixture.Create(
            kind: MediaPositionKind.TimeOffset,
            timeOffsetSeconds: 123.5,
            includeLocation: false);
        ResumePointDto expectedResumePoint = _resumePointDtoFixture.Create(
            mediaKind: MediaKind.MusicTrack,
            lastPosition: expectedPosition);
        SetRouteValues(nameof(MediaKind.MusicTrack), s_mediaItemId);
        _mockApiHttpClient.GetAsync<ResumePointDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedResumePoint);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        JsonElement position = jsonDocument.RootElement.GetProperty("data").GetProperty("lastPosition");
        Assert.Equal((int)MediaPositionKind.TimeOffset, position.GetProperty("kind").GetInt32());
        Assert.Equal(123.5, position.GetProperty("timeOffsetSeconds").GetDouble());
        Assert.Equal(JsonValueKind.Null, position.GetProperty("location").ValueKind);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithValidRouteValues_ShouldCallApiOnceWithTheDocumentedEndpoint()
    {
        // Arrange
        SetRouteValues(nameof(MediaKind.MusicTrack), s_mediaItemId);
        _mockApiHttpClient.GetAsync<ResumePointDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_resumePointDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<ResumePointDto>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/media-resume-points/MusicTrack/{s_mediaItemId}"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreGuidObjects_ShouldFormatThemAsLowercaseGuids()
    {
        // Arrange
        _sut.HttpContext.Request.RouteValues["mediaKind"] = nameof(MediaKind.Book);
        _sut.HttpContext.Request.RouteValues["mediaItemId"] = s_mediaItemId;
        _mockApiHttpClient.GetAsync<ResumePointDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_resumePointDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<ResumePointDto>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/media-resume-points/Book/{s_mediaItemId}"),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("Book", "Book")] // canonical member name
    [InlineData("book", "Book")] // lowercase member name
    [InlineData("BOOK", "Book")] // uppercase member name
    [InlineData("MusicTrack", "MusicTrack")] // another canonical member name
    [InlineData("musicTrack", "MusicTrack")] // mixed case member name
    [InlineData("TvEpisode", "TvEpisode")] // another canonical member name
    [InlineData("tvEpisode", "TvEpisode")] // another mixed case member name
    [InlineData("PodcastEpisode", "PodcastEpisode")] // another canonical member name
    [InlineData("podcastepisode", "PodcastEpisode")] // another lowercase member name
    public async Task ExecuteAsync_WhenMediaKindNameIsValid_ShouldForwardTheCanonicalMemberName(string routeMediaKind, string expectedMediaKind)
    {
        // Arrange
        SetRouteValues(routeMediaKind, s_mediaItemId);
        _mockApiHttpClient.GetAsync<ResumePointDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_resumePointDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<ResumePointDto>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/media-resume-points/{expectedMediaKind}/{s_mediaItemId}"),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("1")] // numeric enum value
    [InlineData("0")] // numeric enum value, the None member
    [InlineData("NotAMediaKind")] // undefined member name
    [InlineData("MusicTrack,Book")] // comma separated member names
    [InlineData("Music Track")] // member name with a space
    [InlineData("MusicTrack/../..")] // path traversal attempt
    [InlineData("..")] // dot segment
    [InlineData("MusicTrack%2F..")] // encoded slash traversal attempt
    public async Task ExecuteAsync_WhenMediaKindNameIsRejected_ShouldForwardTheInvalidPlaceholder(string routeMediaKind)
    {
        // Arrange
        SetRouteValues(routeMediaKind, s_mediaItemId);
        _mockApiHttpClient.GetAsync<ResumePointDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_resumePointDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<ResumePointDto>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/media-resume-points/Invalid/{s_mediaItemId}"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediaItemIdIsNotAGuid_ShouldUseTheEmptyGuid()
    {
        // Arrange
        SetRouteValues(nameof(MediaKind.MusicTrack), "not-a-guid");
        _mockApiHttpClient.GetAsync<ResumePointDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_resumePointDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<ResumePointDto>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/media-resume-points/MusicTrack/{ZERO_GUID}"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreMissing_ShouldUseTheEmptyGuidAndTheInvalidMediaKind()
    {
        // Arrange
        _mockApiHttpClient.GetAsync<ResumePointDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_resumePointDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<ResumePointDto>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/media-resume-points/Invalid/{ZERO_GUID}"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSetPrivateNoStoreAndCookieVaryHeaders()
    {
        // Arrange
        SetRouteValues(nameof(MediaKind.MusicTrack), s_mediaItemId);
        _mockApiHttpClient.GetAsync<ResumePointDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_resumePointDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("Cookie", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Theory]
    [InlineData("MediaResumePointNotFound")] // the user has no resume point for the media item
    [InlineData("BookNotFound")] // the book of the resume point no longer exists
    [InlineData("TrackNotFound")] // the track of the resume point no longer exists
    public async Task ExecuteAsync_WhenApiReportsAnAbsentResumePoint_ShouldReturnSuccessWithNullData(string detail)
    {
        // Arrange
        SetRouteValues(nameof(MediaKind.Book), s_mediaItemId);
        ApiException apiException = new(_problemDetailsDtoFixture.Create(detail: detail), HttpStatusCode.NotFound);
        _mockApiHttpClient.GetAsync<ResumePointDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<Task<ResumePointDto>>(_ => throw apiException);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(JsonValueKind.Null, jsonDocument.RootElement.GetProperty("data").ValueKind);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsAnUnrelatedNotFound_ShouldRethrow()
    {
        // Arrange
        SetRouteValues(nameof(MediaKind.Book), s_mediaItemId);
        ApiException apiException = new(_problemDetailsDtoFixture.Create(detail: "RouteNotFound"), HttpStatusCode.NotFound);
        _mockApiHttpClient.GetAsync<ResumePointDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<Task<ResumePointDto>>(_ => throw apiException);

        // Act
        Func<Task<IResult>> act = () => _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        ApiException exception = await Assert.ThrowsAsync<ApiException>(act);
        Assert.Equal(HttpStatusCode.NotFound, exception.HttpStatusCode);
        Assert.Equal("RouteNotFound", exception.ProblemDetails?.Detail);
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden)] // authorization failure reported by the API
    [InlineData(HttpStatusCode.InternalServerError)] // unexpected API failure
    public async Task ExecuteAsync_WhenApiReturnsAnErrorOtherThanAnAbsentResumePoint_ShouldRethrow(HttpStatusCode httpStatusCode)
    {
        // Arrange
        SetRouteValues(nameof(MediaKind.MusicTrack), s_mediaItemId);
        ApiException apiException = new(_problemDetailsDtoFixture.Create(), httpStatusCode);
        _mockApiHttpClient.GetAsync<ResumePointDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<Task<ResumePointDto>>(_ => throw apiException);

        // Act
        Func<Task<IResult>> act = () => _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        ApiException exception = await Assert.ThrowsAsync<ApiException>(act);
        Assert.Equal(httpStatusCode, exception.HttpStatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        ResumePointDto expectedResumePoint = _resumePointDtoFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
        SetRouteValues(nameof(MediaKind.MusicTrack), s_mediaItemId);
        _mockApiHttpClient.GetAsync<ResumePointDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.ArgAt<CancellationToken>(1).ThrowIfCancellationRequested();
                return expectedResumePoint;
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
