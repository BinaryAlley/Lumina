#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Plugins.Commands.SetLibraryBookReaderEnabled;
using Lumina.Contracts.Fixtures.Core.Requests.Plugins;
using Lumina.Contracts.Requests.Plugins;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Plugins.SetLibraryBookReaderEnabled;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Plugins.SetLibraryBookReaderEnabled;

/// <summary>
/// Contains unit tests for the <see cref="SetLibraryBookReaderEnabledEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetLibraryBookReaderEnabledEndpointTests
{
    private readonly ICommandHandler<SetLibraryBookReaderEnabledCommand, Result<Success>> _mockHandler;
    private readonly SetLibraryBookReaderEnabledEndpoint _sut;
    private readonly SetLibraryBookReaderEnabledRequestFixture _setLibraryBookReaderEnabledRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SetLibraryBookReaderEnabledEndpointTests"/> class.
    /// </summary>
    public SetLibraryBookReaderEnabledEndpointTests()
    {
        _mockHandler = Substitute.For<ICommandHandler<SetLibraryBookReaderEnabledCommand, Result<Success>>>();
        _sut = FastEndpoints.Factory.Create<SetLibraryBookReaderEnabledEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithSuccess()
    {
        // Arrange
        SetLibraryBookReaderEnabledRequest request = _setLibraryBookReaderEnabledRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<SetLibraryBookReaderEnabledCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Success));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<Success> okResult = Assert.IsType<Ok<Success>>(result);
        Assert.Equal(Result.Success, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        SetLibraryBookReaderEnabledRequest request = _setLibraryBookReaderEnabledRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Failure("Authorization.NotAuthorized", "NotAuthorized");
        _mockHandler.HandleAsync(Arg.Any<SetLibraryBookReaderEnabledCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        Assert.Equal("Authorization.NotAuthorized", problemResult.ProblemDetails.Title);
        Assert.Equal("NotAuthorized", problemResult.ProblemDetails.Detail);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendSetLibraryBookReaderEnabledCommandToSender()
    {
        // Arrange
        SetLibraryBookReaderEnabledRequest request = _setLibraryBookReaderEnabledRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<SetLibraryBookReaderEnabledCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Success));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<SetLibraryBookReaderEnabledCommand>(command =>
                command.LibraryId == request.LibraryId &&
                command.PluginId == request.PluginId &&
                command.IsEnabled == request.IsEnabled),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        SetLibraryBookReaderEnabledRequest request = _setLibraryBookReaderEnabledRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<SetLibraryBookReaderEnabledCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(Result.Success);
            }, callInfo.Arg<CancellationToken>()));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(request, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
