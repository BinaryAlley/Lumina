#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using Lumina.Application.Core.FileSystemManagement.Paths.Commands.SplitPath;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Path.SplitPath;
using Lumina.Presentation.Api.UnitTests.Core.Endpoints.FileSystemManagement.Fixtures;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.FileSystemManagement.Path;

/// <summary>
/// Contains unit tests for the <see cref="SplitPathEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SplitPathEndpointTests
{
    private readonly ISender _mockSender;
    private readonly SplitPathEndpoint _sut;
    private readonly PathSegmentResponseFixture _pathSegmentResponseFixture;
    private readonly SplitPathRequestFixture _splitPathRequestFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="SplitPathEndpointTests"/> class.
    /// </summary>
    public SplitPathEndpointTests()
    {
        _mockSender = Substitute.For<ISender>();
        _sut = Factory.Create<SplitPathEndpoint>(_mockSender);
        _pathSegmentResponseFixture = new PathSegmentResponseFixture();
        _splitPathRequestFixture = new SplitPathRequestFixture();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldReturnOkResultWithPathSegmentResponses()
    {
        // Arrange
        SplitPathRequest request = _splitPathRequestFixture.Create(@"C:\Users\TestUser\Documents");
        CancellationToken cancellationToken = CancellationToken.None;
        IEnumerable<PathSegmentResponse> expectedResponse = _pathSegmentResponseFixture.CreateMany();
        _mockSender.Send(Arg.Any<SplitPathCommand>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<IEnumerable<PathSegmentResponse>> okResult = Assert.IsType<Ok<IEnumerable<PathSegmentResponse>>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediatorReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        SplitPathRequest request = _splitPathRequestFixture.Create(@"InvalidPath");
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Validation("Path.Invalid", "The provided path is invalid.");
        _mockSender.Send(Arg.Any<SplitPathCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        HttpValidationProblemDetails validationProblemDetails = Assert.IsType<HttpValidationProblemDetails>(problemDetails.ProblemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, validationProblemDetails.Status);
        Assert.Equal("General.Validation", validationProblemDetails.Title);
        Assert.Equal("OneOrMoreValidationErrorsOccurred", validationProblemDetails.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", validationProblemDetails.Type);
        Assert.Single(validationProblemDetails.Errors);
        Assert.Equal(new[] { "The provided path is invalid." }, validationProblemDetails.Errors["Path.Invalid"]);
        Assert.NotNull(problemDetails.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendSplitPathCommandToMediator()
    {
        // Arrange
        SplitPathRequest request = _splitPathRequestFixture.Create(@"C:\Users\TestUser\Documents");
        CancellationToken cancellationToken = CancellationToken.None;
        _mockSender.Send(Arg.Any<SplitPathCommand>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(_pathSegmentResponseFixture.CreateMany().AsEnumerable()));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockSender.Received(1).Send(
            Arg.Is<SplitPathCommand>(c => c.Path == request.Path),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        SplitPathRequest request = _splitPathRequestFixture.Create(@"C:\Users\TestUser\Documents");
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockSender.Send(Arg.Any<SplitPathCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => new ValueTask<ErrorOr<IEnumerable<PathSegmentResponse>>>(Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return ErrorOrFactory.From(new List<PathSegmentResponse>
                {
                    _pathSegmentResponseFixture.Create()
                }.AsEnumerable());
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
