#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.Management.Queries.GetEnabledLibraries;
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.Management.GetEnabledLibraries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.Management.GetEnabledLibraries;

/// <summary>
/// Contains unit tests for the <see cref="GetEnabledLibrariesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetEnabledLibrariesEndpointTests
{
    private readonly IQueryHandler<GetEnabledLibrariesQuery, Result<LibraryResponse[]>> _mockHandler;
    private readonly GetEnabledLibrariesEndpoint _sut;
    private readonly LibraryResponseFixture _libraryResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetEnabledLibrariesEndpointTests"/> class.
    /// </summary>
    public GetEnabledLibrariesEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetEnabledLibrariesQuery, Result<LibraryResponse[]>>>();
        _sut = Factory.Create<GetEnabledLibrariesEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithLibraryResponses()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        LibraryResponse[] expectedResponses = [_libraryResponseFixture.Create(), _libraryResponseFixture.Create()];
        _mockHandler.HandleAsync(Arg.Any<GetEnabledLibrariesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponses));

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        Ok<LibraryResponse[]> okResult = Assert.IsType<Ok<LibraryResponse[]>>(result);
        Assert.Equal(expectedResponses, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Failure("Library.GetEnabledLibraries.Failed", "Failed to get the enabled media libraries.");
        _mockHandler.HandleAsync(Arg.Any<GetEnabledLibrariesQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemDetails.ProblemDetails);

        Assert.Equal("Library.GetEnabledLibraries.Failed", problemDetails.ProblemDetails.Title);
        Assert.Equal("Failed to get the enabled media libraries.", problemDetails.ProblemDetails.Detail);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails.ProblemDetails.Status);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails.ProblemDetails.Type);
        Assert.NotNull(problemDetails.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetEnabledLibrariesQueryToHandler()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetEnabledLibrariesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Array.Empty<LibraryResponse>()));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(Arg.Any<GetEnabledLibrariesQuery>(), Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetEnabledLibrariesQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(Array.Empty<LibraryResponse>());
            }, callInfo.Arg<CancellationToken>()));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(EmptyRequest.Instance, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
