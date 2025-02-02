#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.ValidatePath;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Path.ValidatePath;
using Lumina.Presentation.Api.UnitTests.Core.Endpoints.FileSystemManagement.Fixtures;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.FileSystemManagement.Path;

/// <summary>
/// Contains unit tests for the <see cref="ValidatePathEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ValidatePathEndpointTests
{
    private readonly ISender _mockSender;
    private readonly ValidatePathEndpoint _sut;
    private readonly ValidatePathRequestFixture _validatePathRequestFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatePathEndpointTests"/> class.
    /// </summary>
    public ValidatePathEndpointTests()
    {
        _mockSender = Substitute.For<ISender>();
        _sut = Factory.Create<ValidatePathEndpoint>(_mockSender);
        _validatePathRequestFixture = new ValidatePathRequestFixture();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldReturnOkResultWithPathValidResponse()
    {
        // Arrange
        ValidatePathRequest request = _validatePathRequestFixture.Create(@"C:\Users\TestUser\Documents");
        CancellationToken cancellationToken = CancellationToken.None;
        PathValidResponse expectedResponse = new(true);
        _mockSender.Send(Arg.Any<ValidatePathQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<PathValidResponse> okResult = Assert.IsType<Ok<PathValidResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendValidatePathQueryToMediator()
    {
        // Arrange
        ValidatePathRequest request = _validatePathRequestFixture.Create(@"C:\Users\TestUser\Documents");
        CancellationToken cancellationToken = CancellationToken.None;
        _mockSender.Send(Arg.Any<ValidatePathQuery>(), Arg.Any<CancellationToken>())
            .Returns(new PathValidResponse(true));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockSender.Received(1).Send(
            Arg.Is<ValidatePathQuery>(q => q.Path == request.Path),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        ValidatePathRequest request = _validatePathRequestFixture.Create(@"C:\Users\TestUser\Documents");
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockSender.Send(Arg.Any<ValidatePathQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => new ValueTask<PathValidResponse>(Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return new PathValidResponse(true);
            }, callInfo.Arg<CancellationToken>())));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(request, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
