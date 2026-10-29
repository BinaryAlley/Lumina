#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using Lumina.Presentation.Web.Common.Requests.MediaConsumption;
using Lumina.Presentation.Web.Core.Endpoints.MediaConsumption.ReportConsumptionProgress;
using Lumina.Presentation.Web.Fixtures.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Fixtures.Common.Requests.MediaConsumption;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.MediaConsumption.ReportConsumptionProgress;

/// <summary>
/// Contains unit tests for the <see cref="ReportConsumptionProgressEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReportConsumptionProgressEndpointTests
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private static readonly Guid s_recordId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly ReportConsumptionProgressEndpoint _sut;
    private readonly ConsumptionRecordDtoFixture _consumptionRecordDtoFixture = new();
    private readonly MediaPositionRequestFixture _mediaPositionRequestFixture = new();
    private readonly ReportConsumptionProgressRequestFixture _reportConsumptionProgressRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportConsumptionProgressEndpointTests"/> class.
    /// </summary>
    public ReportConsumptionProgressEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<ReportConsumptionProgressEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiSucceeds_ShouldReturnSuccessJsonWithConsumptionRecord()
    {
        // Arrange
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create(status: ConsumptionStatus.InProgress);
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create();
        _sut.HttpContext.Request.RouteValues["recordId"] = s_recordId.ToString();
        _mockApiHttpClient.PutAsync<ConsumptionRecordDto, ReportConsumptionProgressRequest>(Arg.Any<string>(), Arg.Any<ReportConsumptionProgressRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedRecord);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
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
        Assert.Equal(expectedRecord.ConsumedDurationInSeconds, data.GetProperty("consumedDurationInSeconds").GetDouble());
        Assert.Equal(expectedRecord.LastProgressFraction, data.GetProperty("lastProgressFraction").GetDouble());
        Assert.Equal(expectedRecord.ConsecutivePagesRead, data.GetProperty("consecutivePagesRead").GetInt32());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldPutTheProgressExactlyOnceToTheDocumentedEndpoint()
    {
        // Arrange
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create();
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create();
        _sut.HttpContext.Request.RouteValues["recordId"] = s_recordId.ToString();
        _mockApiHttpClient.PutAsync<ConsumptionRecordDto, ReportConsumptionProgressRequest>(Arg.Any<string>(), Arg.Any<ReportConsumptionProgressRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedRecord);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<ConsumptionRecordDto, ReportConsumptionProgressRequest>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/consumption-records/{s_recordId}/progress"),
            Arg.Any<ReportConsumptionProgressRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldForwardTheReceivedRequestToTheApi()
    {
        // Arrange
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create();
        MediaPositionRequest position = _mediaPositionRequestFixture.Create(
            kind: nameof(MediaPositionKind.Location),
            includeTimeOffsetSeconds: false);
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create(
            position: position,
            progressFraction: 0.42,
            consumedDurationInSeconds: 120,
            consecutivePagesRead: 17);
        _sut.HttpContext.Request.RouteValues["recordId"] = s_recordId.ToString();
        _mockApiHttpClient.PutAsync<ConsumptionRecordDto, ReportConsumptionProgressRequest>(Arg.Any<string>(), Arg.Any<ReportConsumptionProgressRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedRecord);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<ConsumptionRecordDto, ReportConsumptionProgressRequest>(
            Arg.Any<string>(),
            Arg.Is<ReportConsumptionProgressRequest>(forwardedRequest => forwardedRequest == request
                && forwardedRequest.Position!.Kind == nameof(MediaPositionKind.Location)
                && forwardedRequest.Position.Location == position.Location
                && forwardedRequest.Position.TimeOffsetSeconds == null
                && forwardedRequest.ConsecutivePagesRead == 17),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRecordIdIsNotAGuid_ShouldUseTheEmptyGuid()
    {
        // Arrange
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create();
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create();
        _sut.HttpContext.Request.RouteValues["recordId"] = "not-a-guid";
        _mockApiHttpClient.PutAsync<ConsumptionRecordDto, ReportConsumptionProgressRequest>(Arg.Any<string>(), Arg.Any<ReportConsumptionProgressRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedRecord);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<ConsumptionRecordDto, ReportConsumptionProgressRequest>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/consumption-records/{ZERO_GUID}/progress"),
            Arg.Any<ReportConsumptionProgressRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRecordIdIsMissing_ShouldUseTheEmptyGuid()
    {
        // Arrange
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create();
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create();
        _mockApiHttpClient.PutAsync<ConsumptionRecordDto, ReportConsumptionProgressRequest>(Arg.Any<string>(), Arg.Any<ReportConsumptionProgressRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedRecord);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<ConsumptionRecordDto, ReportConsumptionProgressRequest>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/consumption-records/{ZERO_GUID}/progress"),
            Arg.Any<ReportConsumptionProgressRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRecordIdIsUppercase_ShouldNormalizeItToTheLowercaseGuid()
    {
        // Arrange
        Guid recordId = Guid.Parse("ABCDEFAB-CDEF-ABCD-EFAB-CDEFABCDEFAB");
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create();
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create();
        _sut.HttpContext.Request.RouteValues["recordId"] = recordId.ToString().ToUpperInvariant();
        _mockApiHttpClient.PutAsync<ConsumptionRecordDto, ReportConsumptionProgressRequest>(Arg.Any<string>(), Arg.Any<ReportConsumptionProgressRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedRecord);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<ConsumptionRecordDto, ReportConsumptionProgressRequest>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/consumption-records/{recordId}/progress"),
            Arg.Any<ReportConsumptionProgressRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSetPrivateNoStoreAndCookieVaryHeaders()
    {
        // Arrange
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create();
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create();
        _sut.HttpContext.Request.RouteValues["recordId"] = s_recordId.ToString();
        _mockApiHttpClient.PutAsync<ConsumptionRecordDto, ReportConsumptionProgressRequest>(Arg.Any<string>(), Arg.Any<ReportConsumptionProgressRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedRecord);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("Cookie", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create();
        ReportConsumptionProgressRequest request = _reportConsumptionProgressRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
        _sut.HttpContext.Request.RouteValues["recordId"] = s_recordId.ToString();
        _mockApiHttpClient.PutAsync<ConsumptionRecordDto, ReportConsumptionProgressRequest>(Arg.Any<string>(), Arg.Any<ReportConsumptionProgressRequest>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.ArgAt<CancellationToken>(2).ThrowIfCancellationRequested();
                return expectedRecord;
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
