#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using Lumina.Presentation.Web.Common.Requests.MediaConsumption;
using Lumina.Presentation.Web.Core.Endpoints.MediaConsumption.CompleteConsumption;
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

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.MediaConsumption.CompleteConsumption;

/// <summary>
/// Contains unit tests for the <see cref="CompleteConsumptionEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CompleteConsumptionEndpointTests
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private static readonly Guid s_recordId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly CompleteConsumptionEndpoint _sut;
    private readonly ConsumptionRecordDtoFixture _consumptionRecordDtoFixture = new();
    private readonly MediaPositionRequestFixture _mediaPositionRequestFixture = new();
    private readonly CompleteConsumptionRequestFixture _completeConsumptionRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CompleteConsumptionEndpointTests"/> class.
    /// </summary>
    public CompleteConsumptionEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<CompleteConsumptionEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiSucceeds_ShouldReturnSuccessJsonWithConsumptionRecord()
    {
        // Arrange
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create(status: ConsumptionStatus.Completed);
        CompleteConsumptionRequest request = _completeConsumptionRequestFixture.Create();
        _sut.HttpContext.Request.RouteValues["recordId"] = s_recordId.ToString();
        _mockApiHttpClient.PostAsync<ConsumptionRecordDto, CompleteConsumptionRequest>(Arg.Any<string>(), Arg.Any<CompleteConsumptionRequest>(), Arg.Any<CancellationToken>())
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
    public async Task ExecuteAsync_WhenCalled_ShouldPostTheCompletionExactlyOnceToTheDocumentedEndpoint()
    {
        // Arrange
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create();
        CompleteConsumptionRequest request = _completeConsumptionRequestFixture.Create();
        _sut.HttpContext.Request.RouteValues["recordId"] = s_recordId.ToString();
        _mockApiHttpClient.PostAsync<ConsumptionRecordDto, CompleteConsumptionRequest>(Arg.Any<string>(), Arg.Any<CompleteConsumptionRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedRecord);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<ConsumptionRecordDto, CompleteConsumptionRequest>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/consumption-records/{s_recordId}/complete"),
            Arg.Any<CompleteConsumptionRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldForwardTheReceivedRequestToTheApi()
    {
        // Arrange
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create();
        MediaPositionRequest position = _mediaPositionRequestFixture.Create(
            kind: nameof(MediaPositionKind.TimeOffset),
            includeLocation: false);
        CompleteConsumptionRequest request = _completeConsumptionRequestFixture.Create(
            position: position,
            progressFraction: 1,
            consumedDurationInSeconds: 240,
            consecutivePagesRead: 12,
            includeConsecutivePagesRead: true);
        _sut.HttpContext.Request.RouteValues["recordId"] = s_recordId.ToString();
        _mockApiHttpClient.PostAsync<ConsumptionRecordDto, CompleteConsumptionRequest>(Arg.Any<string>(), Arg.Any<CompleteConsumptionRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedRecord);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<ConsumptionRecordDto, CompleteConsumptionRequest>(
            Arg.Any<string>(),
            Arg.Is<CompleteConsumptionRequest>(forwardedRequest => forwardedRequest == request
                && forwardedRequest.Position!.Kind == nameof(MediaPositionKind.TimeOffset)
                && forwardedRequest.Position.TimeOffsetSeconds == position.TimeOffsetSeconds
                && forwardedRequest.ConsecutivePagesRead == 12),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRecordIdIsNotAGuid_ShouldUseTheEmptyGuid()
    {
        // Arrange
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create();
        CompleteConsumptionRequest request = _completeConsumptionRequestFixture.Create();
        _sut.HttpContext.Request.RouteValues["recordId"] = "not-a-guid";
        _mockApiHttpClient.PostAsync<ConsumptionRecordDto, CompleteConsumptionRequest>(Arg.Any<string>(), Arg.Any<CompleteConsumptionRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedRecord);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<ConsumptionRecordDto, CompleteConsumptionRequest>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/consumption-records/{ZERO_GUID}/complete"),
            Arg.Any<CompleteConsumptionRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRecordIdIsMissing_ShouldUseTheEmptyGuid()
    {
        // Arrange
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create();
        CompleteConsumptionRequest request = _completeConsumptionRequestFixture.Create();
        _mockApiHttpClient.PostAsync<ConsumptionRecordDto, CompleteConsumptionRequest>(Arg.Any<string>(), Arg.Any<CompleteConsumptionRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedRecord);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<ConsumptionRecordDto, CompleteConsumptionRequest>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/consumption-records/{ZERO_GUID}/complete"),
            Arg.Any<CompleteConsumptionRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRecordIdIsUppercase_ShouldNormalizeItToTheLowercaseGuid()
    {
        // Arrange
        Guid recordId = Guid.Parse("ABCDEFAB-CDEF-ABCD-EFAB-CDEFABCDEFAB");
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create();
        CompleteConsumptionRequest request = _completeConsumptionRequestFixture.Create();
        _sut.HttpContext.Request.RouteValues["recordId"] = recordId.ToString().ToUpperInvariant();
        _mockApiHttpClient.PostAsync<ConsumptionRecordDto, CompleteConsumptionRequest>(Arg.Any<string>(), Arg.Any<CompleteConsumptionRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedRecord);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<ConsumptionRecordDto, CompleteConsumptionRequest>(
            Arg.Is<string>(endpoint => endpoint == $"users/me/consumption-records/{recordId}/complete"),
            Arg.Any<CompleteConsumptionRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSetPrivateNoStoreAndCookieVaryHeaders()
    {
        // Arrange
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create();
        CompleteConsumptionRequest request = _completeConsumptionRequestFixture.Create();
        _sut.HttpContext.Request.RouteValues["recordId"] = s_recordId.ToString();
        _mockApiHttpClient.PostAsync<ConsumptionRecordDto, CompleteConsumptionRequest>(Arg.Any<string>(), Arg.Any<CompleteConsumptionRequest>(), Arg.Any<CancellationToken>())
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
        CompleteConsumptionRequest request = _completeConsumptionRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
        _sut.HttpContext.Request.RouteValues["recordId"] = s_recordId.ToString();
        _mockApiHttpClient.PostAsync<ConsumptionRecordDto, CompleteConsumptionRequest>(Arg.Any<string>(), Arg.Any<CompleteConsumptionRequest>(), Arg.Any<CancellationToken>())
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
