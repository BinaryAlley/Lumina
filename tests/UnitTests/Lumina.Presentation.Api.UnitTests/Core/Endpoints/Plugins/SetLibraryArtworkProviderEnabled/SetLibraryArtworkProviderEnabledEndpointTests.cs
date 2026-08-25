#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Plugins.Commands.SetLibraryArtworkProviderEnabled;
using Lumina.Contracts.Fixtures.Core.Requests.Plugins;
using Lumina.Contracts.Requests.Plugins;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Plugins.SetLibraryArtworkProviderEnabled;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Plugins.SetLibraryArtworkProviderEnabled;

/// <summary>
/// Contains unit tests for the <see cref="SetLibraryArtworkProviderEnabledEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetLibraryArtworkProviderEnabledEndpointTests
{
    private readonly ICommandHandler<SetLibraryArtworkProviderEnabledCommand, Result<Success>> _mockHandler;
    private readonly SetLibraryArtworkProviderEnabledEndpoint _sut;
    private readonly SetLibraryArtworkProviderEnabledRequestFixture _setLibraryArtworkProviderEnabledRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SetLibraryArtworkProviderEnabledEndpointTests"/> class.
    /// </summary>
    public SetLibraryArtworkProviderEnabledEndpointTests()
    {
        _mockHandler = Substitute.For<ICommandHandler<SetLibraryArtworkProviderEnabledCommand, Result<Success>>>();
        _sut = FastEndpoints.Factory.Create<SetLibraryArtworkProviderEnabledEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithSuccess()
    {
        // Arrange
        SetLibraryArtworkProviderEnabledRequest request = _setLibraryArtworkProviderEnabledRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<SetLibraryArtworkProviderEnabledCommand>(), Arg.Any<CancellationToken>())
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
        SetLibraryArtworkProviderEnabledRequest request = _setLibraryArtworkProviderEnabledRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.NotFound("LibraryArtworkProviderConfiguration.NotFound", "The artwork provider configuration was not found.");
        _mockHandler.HandleAsync(Arg.Any<SetLibraryArtworkProviderEnabledCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemDetails.ProblemDetails);

        Assert.Equal("LibraryArtworkProviderConfiguration.NotFound", problemDetails.ProblemDetails.Title);
        Assert.Equal("The artwork provider configuration was not found.", problemDetails.ProblemDetails.Detail);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.ProblemDetails.Status);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problemDetails.ProblemDetails.Type);
        Assert.NotNull(problemDetails.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendSetLibraryArtworkProviderEnabledCommandToSender()
    {
        // Arrange
        SetLibraryArtworkProviderEnabledRequest request = _setLibraryArtworkProviderEnabledRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<SetLibraryArtworkProviderEnabledCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Success));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<SetLibraryArtworkProviderEnabledCommand>(command =>
                command.LibraryId == request.LibraryId &&
                command.PluginId == request.PluginId &&
                command.IsEnabled == request.IsEnabled),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        SetLibraryArtworkProviderEnabledRequest request = _setLibraryArtworkProviderEnabledRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<SetLibraryArtworkProviderEnabledCommand>(), Arg.Any<CancellationToken>())
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
