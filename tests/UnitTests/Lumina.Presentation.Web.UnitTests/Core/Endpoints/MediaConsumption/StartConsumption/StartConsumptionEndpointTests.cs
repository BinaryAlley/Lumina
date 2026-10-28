#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using Lumina.Presentation.Web.Core.Endpoints.MediaConsumption.StartConsumption;
using Lumina.Presentation.Web.Fixtures.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.MediaConsumption.StartConsumption;

/// <summary>
/// Contains unit tests for the <see cref="StartConsumptionEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StartConsumptionEndpointTests
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private static readonly Guid s_libraryId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid s_mediaItemId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly StartConsumptionEndpoint _sut;
    private readonly ConsumptionRecordDtoFixture _consumptionRecordDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="StartConsumptionEndpointTests"/> class.
    /// </summary>
    public StartConsumptionEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<StartConsumptionEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiSucceeds_ShouldReturnSuccessJsonWithConsumptionRecord()
    {
        // Arrange
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create(status: ConsumptionStatus.InProgress);
        SetRouteValues(s_libraryId, nameof(MediaKind.MusicTrack), s_mediaItemId);
        _mockApiHttpClient.PostAsync<ConsumptionRecordDto, EmptyRequest>(Arg.Any<string>(), Arg.Any<EmptyRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedRecord);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = jsonDocument.RootElement.GetProperty("data");
        Assert.Equal(expectedRecord.Id, data.GetProperty("id").GetGuid());
        Assert.Equal(expectedRecord.LibraryId, data.GetProperty("libraryId").GetGuid());
        Assert.Equal(expectedRecord.MediaItemId, data.GetProperty("mediaItemId").GetGuid());
        Assert.Equal((int)expectedRecord.MediaKind, data.GetProperty("mediaKind").GetInt32());
        Assert.Equal((int)expectedRecord.Status, data.GetProperty("status").GetInt32());
        Assert.Equal(expectedRecord.StartedOnUtc, data.GetProperty("startedOnUtc").GetDateTime());
        Assert.Equal(expectedRecord.LastActivityOnUtc, data.GetProperty("lastActivityOnUtc").GetDateTime());
        Assert.Equal(expectedRecord.EndedOnUtc, data.GetProperty("endedOnUtc").GetDateTime());
        Assert.Equal(expectedRecord.ConsumedDurationInSeconds, data.GetProperty("consumedDurationInSeconds").GetDouble());
        Assert.Equal(expectedRecord.LastProgressFraction, data.GetProperty("lastProgressFraction").GetDouble());
        Assert.Equal(expectedRecord.ConsecutivePagesRead, data.GetProperty("consecutivePagesRead").GetInt32());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithValidRouteValues_ShouldCallApiOnceWithTheDocumentedEndpoint()
    {
        // Arrange
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create();
        SetRouteValues(s_libraryId, nameof(MediaKind.MusicTrack), s_mediaItemId);
        _mockApiHttpClient.PostAsync<ConsumptionRecordDto, EmptyRequest>(Arg.Any<string>(), Arg.Any<EmptyRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedRecord);

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<ConsumptionRecordDto, EmptyRequest>(
            Arg.Is<string>(endpoint => endpoint == $"libraries/{s_libraryId}/media-items/MusicTrack/{s_mediaItemId}/consumption-records"),
            Arg.Any<EmptyRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreGuidObjects_ShouldFormatThemAsLowercaseGuids()
    {
        // Arrange
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create();
        _sut.HttpContext.Request.RouteValues["libraryId"] = s_libraryId;
        _sut.HttpContext.Request.RouteValues["mediaKind"] = nameof(MediaKind.Book);
        _sut.HttpContext.Request.RouteValues["mediaItemId"] = s_mediaItemId;
        _mockApiHttpClient.PostAsync<ConsumptionRecordDto, EmptyRequest>(Arg.Any<string>(), Arg.Any<EmptyRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedRecord);

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<ConsumptionRecordDto, EmptyRequest>(
            Arg.Is<string>(endpoint => endpoint == $"libraries/{s_libraryId}/media-items/Book/{s_mediaItemId}/consumption-records"),
            Arg.Any<EmptyRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenLibraryIdIsNotAGuid_ShouldUseTheEmptyGuid()
    {
        // Arrange
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create();
        SetRouteValues("not-a-guid", nameof(MediaKind.MusicTrack), s_mediaItemId);
        _mockApiHttpClient.PostAsync<ConsumptionRecordDto, EmptyRequest>(Arg.Any<string>(), Arg.Any<EmptyRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedRecord);

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<ConsumptionRecordDto, EmptyRequest>(
            Arg.Is<string>(endpoint => endpoint == $"libraries/{ZERO_GUID}/media-items/MusicTrack/{s_mediaItemId}/consumption-records"),
            Arg.Any<EmptyRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediaItemIdIsNotAGuid_ShouldUseTheEmptyGuid()
    {
        // Arrange
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create();
        SetRouteValues(s_libraryId, nameof(MediaKind.MusicTrack), "not-a-guid");
        _mockApiHttpClient.PostAsync<ConsumptionRecordDto, EmptyRequest>(Arg.Any<string>(), Arg.Any<EmptyRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedRecord);

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<ConsumptionRecordDto, EmptyRequest>(
            Arg.Is<string>(endpoint => endpoint == $"libraries/{s_libraryId}/media-items/MusicTrack/{ZERO_GUID}/consumption-records"),
            Arg.Any<EmptyRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreMissing_ShouldUseEmptyGuidsAndTheInvalidMediaKind()
    {
        // Arrange
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create();
        _mockApiHttpClient.PostAsync<ConsumptionRecordDto, EmptyRequest>(Arg.Any<string>(), Arg.Any<EmptyRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedRecord);

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<ConsumptionRecordDto, EmptyRequest>(
            Arg.Is<string>(endpoint => endpoint == $"libraries/{ZERO_GUID}/media-items/Invalid/{ZERO_GUID}/consumption-records"),
            Arg.Any<EmptyRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("MusicTrack", "MusicTrack")] // canonical member name
    [InlineData("musictrack", "MusicTrack")] // lowercase member name
    [InlineData("MUSICTRACK", "MusicTrack")] // uppercase member name
    [InlineData("tvEpisode", "TvEpisode")] // mixed case member name
    [InlineData("Book", "Book")] // another canonical member name
    [InlineData("podcastepisode", "PodcastEpisode")] // another lowercase member name
    public async Task ExecuteAsync_WhenMediaKindNameIsValid_ShouldForwardTheCanonicalMemberName(string routeMediaKind, string expectedMediaKind)
    {
        // Arrange
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create();
        SetRouteValues(s_libraryId, routeMediaKind, s_mediaItemId);
        _mockApiHttpClient.PostAsync<ConsumptionRecordDto, EmptyRequest>(Arg.Any<string>(), Arg.Any<EmptyRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedRecord);

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<ConsumptionRecordDto, EmptyRequest>(
            Arg.Is<string>(endpoint => endpoint == $"libraries/{s_libraryId}/media-items/{expectedMediaKind}/{s_mediaItemId}/consumption-records"),
            Arg.Any<EmptyRequest>(),
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
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create();
        SetRouteValues(s_libraryId, routeMediaKind, s_mediaItemId);
        _mockApiHttpClient.PostAsync<ConsumptionRecordDto, EmptyRequest>(Arg.Any<string>(), Arg.Any<EmptyRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedRecord);

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<ConsumptionRecordDto, EmptyRequest>(
            Arg.Is<string>(endpoint => endpoint == $"libraries/{s_libraryId}/media-items/Invalid/{s_mediaItemId}/consumption-records"),
            Arg.Any<EmptyRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSetPrivateNoStoreAndCookieVaryHeaders()
    {
        // Arrange
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create();
        SetRouteValues(s_libraryId, nameof(MediaKind.MusicTrack), s_mediaItemId);
        _mockApiHttpClient.PostAsync<ConsumptionRecordDto, EmptyRequest>(Arg.Any<string>(), Arg.Any<EmptyRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedRecord);

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
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
        SetRouteValues(s_libraryId, nameof(MediaKind.MusicTrack), s_mediaItemId);
        _mockApiHttpClient.PostAsync<ConsumptionRecordDto, EmptyRequest>(Arg.Any<string>(), Arg.Any<EmptyRequest>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.ArgAt<CancellationToken>(2).ThrowIfCancellationRequested();
                return expectedRecord;
            }, callInfo.ArgAt<CancellationToken>(2)));

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
    /// <param name="libraryId">The value of the library identifier route segment.</param>
    /// <param name="mediaKind">The value of the media kind route segment.</param>
    /// <param name="mediaItemId">The value of the media item identifier route segment.</param>
    private void SetRouteValues(object libraryId, object mediaKind, object mediaItemId)
    {
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId;
        _sut.HttpContext.Request.RouteValues["mediaKind"] = mediaKind;
        _sut.HttpContext.Request.RouteValues["mediaItemId"] = mediaItemId;
    }
}
