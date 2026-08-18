#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Plugins.Commands.ReorderLibraryMetadataProviders;
using Lumina.Contracts.Fixtures.Core.Requests.Plugins;
using Lumina.Contracts.Requests.Plugins;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Plugins.ReorderLibraryMetadataProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Plugins.ReorderLibraryMetadataProviders;

/// <summary>
/// Contains unit tests for the <see cref="ReorderLibraryMetadataProvidersEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReorderLibraryMetadataProvidersEndpointTests
{
    private readonly ICommandHandler<ReorderLibraryMetadataProvidersCommand, Result<Success>> _mockHandler;
    private readonly ReorderLibraryMetadataProvidersEndpoint _sut;
    private readonly ReorderLibraryMetadataProvidersRequestFixture _reorderLibraryMetadataProvidersRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ReorderLibraryMetadataProvidersEndpointTests"/> class.
    /// </summary>
    public ReorderLibraryMetadataProvidersEndpointTests()
    {
        _mockHandler = Substitute.For<ICommandHandler<ReorderLibraryMetadataProvidersCommand, Result<Success>>>();
        _sut = FastEndpoints.Factory.Create<ReorderLibraryMetadataProvidersEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithSuccess()
    {
        // Arrange
        ReorderLibraryMetadataProvidersRequest request = _reorderLibraryMetadataProvidersRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<ReorderLibraryMetadataProvidersCommand>(), Arg.Any<CancellationToken>())
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
        ReorderLibraryMetadataProvidersRequest request = _reorderLibraryMetadataProvidersRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.NotFound("LibraryMetadataProviderConfiguration.NotFound", "The metadata provider configuration was not found.");
        _mockHandler.HandleAsync(Arg.Any<ReorderLibraryMetadataProvidersCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemDetails.ProblemDetails);

        Assert.Equal("LibraryMetadataProviderConfiguration.NotFound", problemDetails.ProblemDetails.Title);
        Assert.Equal("The metadata provider configuration was not found.", problemDetails.ProblemDetails.Detail);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.ProblemDetails.Status);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problemDetails.ProblemDetails.Type);
        Assert.NotNull(problemDetails.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendReorderLibraryMetadataProvidersCommandToSender()
    {
        // Arrange
        ReorderLibraryMetadataProvidersRequest request = _reorderLibraryMetadataProvidersRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<ReorderLibraryMetadataProvidersCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Result.Success));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<ReorderLibraryMetadataProvidersCommand>(command =>
                command.LibraryId == request.LibraryId &&
                command.PluginIds.SequenceEqual(request.PluginIds)),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        ReorderLibraryMetadataProvidersRequest request = _reorderLibraryMetadataProvidersRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<ReorderLibraryMetadataProvidersCommand>(), Arg.Any<CancellationToken>())
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
