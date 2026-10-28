#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using Lumina.Presentation.Web.Common.Requests.MediaConsumption;
using Lumina.Presentation.Web.Core.Endpoints.MediaConsumption.StopConsumption;
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

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.MediaConsumption.StopConsumption;

/// <summary>
/// Contains unit tests for the <see cref="StopConsumptionEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StopConsumptionEndpointTests
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private static readonly Guid s_recordId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly StopConsumptionEndpoint _sut;
    private readonly ConsumptionRecordDtoFixture _consumptionRecordDtoFixture = new();
    private readonly MediaPositionRequestFixture _mediaPositionRequestFixture = new();
    private readonly StopConsumptionRequestFixture _stopConsumptionRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="StopConsumptionEndpointTests"/> class.
    /// </summary>
    public StopConsumptionEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<StopConsumptionEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiSucceeds_ShouldReturnSuccessJsonWithConsumptionRecord()
    {
        // Arrange
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create(status: ConsumptionStatus.Stopped);
        StopConsumptionRequest request = _stopConsumptionRequestFixture.Create();
        _sut.HttpContext.Request.RouteValues["recordId"] = s_recordId.ToString();
        _mockApiHttpClient.PostAsync<ConsumptionRecordDto, StopConsumptionRequest>(Arg.Any<string>(), Arg.Any<StopConsumptionRequest>(), Arg.Any<CancellationToken>())
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
        Assert.Equal(expectedRecord.EndedOnUtc, data.GetProperty("endedOnUtc").GetDateTime());
        Assert.Equal(expectedRecord.ConsumedDurationInSeconds, data.GetProperty("consumedDurationInSeconds").GetDouble());
        Assert.Equal(expectedRecord.LastProgressFraction, data.GetProperty("lastProgressFraction").GetDouble());
        Assert.Equal(expectedRecord.ConsecutivePagesRead, data.GetProperty("consecutivePagesRead").GetInt32());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldPostTheStopExactlyOnceToTheDocumentedEndpoint()
    {
        // Arrange
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create();
        StopConsumptionRequest request = _stopConsumptionRequestFixture.Create();
        _sut.HttpContext.Request.RouteValues["recordId"] = s_recordId.ToString();
        _mockApiHttpClient.PostAsync<ConsumptionRecordDto, StopConsumptionRequest>(Arg.Any<string>(), Arg.Any<StopConsumptionRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedRecord);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<ConsumptionRecordDto, StopConsumptionRequest>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/consumption-records/{s_recordId}/stop"),
            Arg.Any<StopConsumptionRequest>(),
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
        StopConsumptionRequest request = _stopConsumptionRequestFixture.Create(
            position: position,
            progressFraction: 0.3,
            consumedDurationInSeconds: 95,
            consecutivePagesRead: 8,
            includeConsecutivePagesRead: true);
        _sut.HttpContext.Request.RouteValues["recordId"] = s_recordId.ToString();
        _mockApiHttpClient.PostAsync<ConsumptionRecordDto, StopConsumptionRequest>(Arg.Any<string>(), Arg.Any<StopConsumptionRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedRecord);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<ConsumptionRecordDto, StopConsumptionRequest>(
            Arg.Any<string>(),
            Arg.Is<StopConsumptionRequest>(forwardedRequest => forwardedRequest == request
                && forwardedRequest.Position!.Kind == nameof(MediaPositionKind.Location)
                && forwardedRequest.Position.Location == position.Location
                && forwardedRequest.ConsecutivePagesRead == 8),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRecordIdIsNotAGuid_ShouldUseTheEmptyGuid()
    {
        // Arrange
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create();
        StopConsumptionRequest request = _stopConsumptionRequestFixture.Create();
        _sut.HttpContext.Request.RouteValues["recordId"] = "not-a-guid";
        _mockApiHttpClient.PostAsync<ConsumptionRecordDto, StopConsumptionRequest>(Arg.Any<string>(), Arg.Any<StopConsumptionRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedRecord);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<ConsumptionRecordDto, StopConsumptionRequest>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/consumption-records/{ZERO_GUID}/stop"),
            Arg.Any<StopConsumptionRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRecordIdIsMissing_ShouldUseTheEmptyGuid()
    {
        // Arrange
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create();
        StopConsumptionRequest request = _stopConsumptionRequestFixture.Create();
        _mockApiHttpClient.PostAsync<ConsumptionRecordDto, StopConsumptionRequest>(Arg.Any<string>(), Arg.Any<StopConsumptionRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedRecord);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<ConsumptionRecordDto, StopConsumptionRequest>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/consumption-records/{ZERO_GUID}/stop"),
            Arg.Any<StopConsumptionRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRecordIdIsUppercase_ShouldNormalizeItToTheLowercaseGuid()
    {
        // Arrange
        Guid recordId = Guid.Parse("ABCDEFAB-CDEF-ABCD-EFAB-CDEFABCDEFAB");
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create();
        StopConsumptionRequest request = _stopConsumptionRequestFixture.Create();
        _sut.HttpContext.Request.RouteValues["recordId"] = recordId.ToString().ToUpperInvariant();
        _mockApiHttpClient.PostAsync<ConsumptionRecordDto, StopConsumptionRequest>(Arg.Any<string>(), Arg.Any<StopConsumptionRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedRecord);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<ConsumptionRecordDto, StopConsumptionRequest>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/consumption-records/{recordId}/stop"),
            Arg.Any<StopConsumptionRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSetPrivateNoStoreAndCookieVaryHeaders()
    {
        // Arrange
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create();
        StopConsumptionRequest request = _stopConsumptionRequestFixture.Create();
        _sut.HttpContext.Request.RouteValues["recordId"] = s_recordId.ToString();
        _mockApiHttpClient.PostAsync<ConsumptionRecordDto, StopConsumptionRequest>(Arg.Any<string>(), Arg.Any<StopConsumptionRequest>(), Arg.Any<CancellationToken>())
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
        StopConsumptionRequest request = _stopConsumptionRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
        _sut.HttpContext.Request.RouteValues["recordId"] = s_recordId.ToString();
        _mockApiHttpClient.PostAsync<ConsumptionRecordDto, StopConsumptionRequest>(Arg.Any<string>(), Arg.Any<StopConsumptionRequest>(), Arg.Any<CancellationToken>())
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
