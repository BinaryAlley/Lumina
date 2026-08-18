#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibrary;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.Management;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.Management.GetLibrary;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.Management.GetLibrary;

/// <summary>
/// Contains unit tests for the <see cref="GetLibraryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryEndpointTests
{
    private readonly IQueryHandler<GetLibraryQuery, Result<LibraryResponse>> _mockHandler;
    private readonly GetLibraryEndpoint _sut;
    private readonly GetLibraryRequestFixture _getLibraryRequestFixture = new();
    private readonly LibraryResponseFixture _libraryResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryEndpointTests"/> class.
    /// </summary>
    public GetLibraryEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetLibraryQuery, Result<LibraryResponse>>>();
        _sut = Factory.Create<GetLibraryEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithLibraryResponse()
    {
        // Arrange
        GetLibraryRequest request = _getLibraryRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        LibraryResponse expectedResponse = _libraryResponseFixture.Create(id: request.Id);
        _mockHandler.HandleAsync(Arg.Any<GetLibraryQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<LibraryResponse> okResult = Assert.IsType<Ok<LibraryResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        GetLibraryRequest request = _getLibraryRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.NotFound("Library.NotFound", "The requested library was not found.");
        _mockHandler.HandleAsync(Arg.Any<GetLibraryQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemDetails.ProblemDetails);

        Assert.Equal("Library.NotFound", problemDetails.ProblemDetails.Title);
        Assert.Equal("The requested library was not found.", problemDetails.ProblemDetails.Detail);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.ProblemDetails.Status);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problemDetails.ProblemDetails.Type);
        Assert.NotNull(problemDetails.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetLibraryQueryToSender()
    {
        // Arrange
        GetLibraryRequest request = _getLibraryRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetLibraryQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_libraryResponseFixture.Create(id: request.Id)));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetLibraryQuery>(query => query.Id == request.Id),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetLibraryRequest request = _getLibraryRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetLibraryQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_libraryResponseFixture.Create(id: request.Id));
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
