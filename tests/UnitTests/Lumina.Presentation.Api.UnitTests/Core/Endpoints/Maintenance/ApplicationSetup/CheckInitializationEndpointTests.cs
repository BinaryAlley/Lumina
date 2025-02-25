#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Core.Maintenance.ApplicationSetup.Queries.CheckInitialization;
using Lumina.Contracts.Responses.UsersManagement;
using Lumina.Presentation.Api.Core.Endpoints.Maintenance.ApplicationSetup;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Maintenance.ApplicationSetup;

/// <summary>
/// Contains unit tests for the <see cref="CheckInitializationEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CheckInitializationEndpointTests
{
    private readonly ISender _mockSender;
    private readonly CheckInitializationEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckInitializationEndpointTests"/> class.
    /// </summary>
    public CheckInitializationEndpointTests()
    {
        _mockSender = Substitute.For<ISender>();
        _sut = Factory.Create<CheckInitializationEndpoint>(_mockSender);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldReturnOkResultWithInitializationResponse()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        InitializationResponse expectedResponse = new(IsInitialized: true);
        _mockSender.Send(Arg.Any<CheckInitializationQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        IResult result = await _sut.ExecuteAsync(new EmptyRequest(), cancellationToken);

        // Assert
        Ok<InitializationResponse> okResult = Assert.IsType<Ok<InitializationResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendCheckInitializationQueryToMediator()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockSender.Send(Arg.Any<CheckInitializationQuery>(), Arg.Any<CancellationToken>())
            .Returns(new InitializationResponse(IsInitialized: true));

        // Act
        await _sut.ExecuteAsync(new EmptyRequest(), cancellationToken);

        // Assert
        await _mockSender.Received(1).Send(
            Arg.Any<CheckInitializationQuery>(),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockSender.Send(Arg.Any<CheckInitializationQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => new ValueTask<InitializationResponse>(Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return new InitializationResponse(IsInitialized: true);
            }, callInfo.Arg<CancellationToken>())));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(new EmptyRequest(), cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
