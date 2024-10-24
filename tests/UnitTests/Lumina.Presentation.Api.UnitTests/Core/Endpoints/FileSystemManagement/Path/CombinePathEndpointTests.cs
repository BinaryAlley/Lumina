#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using FluentAssertions;
using Lumina.Application.Core.FileSystemManagement.Paths.Commands.CombinePath;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Path.CombinePath;
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
/// Contains unit tests for the <see cref="CombinePathEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CombinePathEndpointTests
{
    private readonly ISender _mockSender;
    private readonly CombinePathEndpoint _sut;
    private readonly PathSegmentResponseFixture _pathSegmentResponseFixture;
    private readonly CombinePathRequestFixture _combinePathRequestFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="CombinePathEndpointTests"/> class.
    /// </summary>
    public CombinePathEndpointTests()
    {
        _mockSender = Substitute.For<ISender>();
        _sut = Factory.Create<CombinePathEndpoint>(_mockSender);
        _pathSegmentResponseFixture = new PathSegmentResponseFixture();
        _combinePathRequestFixture = new CombinePathRequestFixture();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldReturnOkResultWithPathSegmentResponse()
    {
        // Arrange
        CombinePathRequest request = _combinePathRequestFixture.Create("C:\\Users", "TestUser");
        CancellationToken cancellationToken = CancellationToken.None;
        PathSegmentResponse expectedResponse = _pathSegmentResponseFixture.Create();
        _mockSender.Send(Arg.Any<CombinePathCommand>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        PathSegmentResponse actualResponse = result.Should().BeOfType<Ok<PathSegmentResponse>>().Subject.Value!;
        actualResponse.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediatorReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        CombinePathRequest request = _combinePathRequestFixture.Create("InvalidPath", "TestUser");
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.NotFound("Path.NotFound", "The requested path was not found.");
        _mockSender.Send(Arg.Any<CombinePathCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        result.Should().BeOfType<ProblemHttpResult>();
        ProblemHttpResult problemDetails = (ProblemHttpResult)result;
        problemDetails.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.ContentType.Should().Be("application/problem+json");
        problemDetails.ProblemDetails.Should().BeOfType<Microsoft.AspNetCore.Mvc.ProblemDetails>();

        problemDetails.ProblemDetails.Title.Should().Be("Path.NotFound");
        problemDetails.ProblemDetails.Detail.Should().Be("The requested path was not found.");
        problemDetails.ProblemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.ProblemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.5");
        problemDetails.ProblemDetails.Extensions["traceId"].Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediatorReturnsValidationError_ShouldReturnValidationProblemResult()
    {
        // Arrange
        CombinePathRequest request = _combinePathRequestFixture.Create("InvalidPath", "TestUser");
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Validation("Path.Invalid", "The provided path is invalid.");
        _mockSender.Send(Arg.Any<CombinePathCommand>(), Arg.Any<CancellationToken>())
              .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        result.Should().BeOfType<ProblemHttpResult>();
        ProblemHttpResult problemDetails = (ProblemHttpResult)result;
        problemDetails.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        problemDetails.ContentType.Should().Be("application/problem+json");
        problemDetails.ProblemDetails.Should().BeOfType<HttpValidationProblemDetails>();
        HttpValidationProblemDetails validationProblemDetails = (HttpValidationProblemDetails)problemDetails.ProblemDetails;
        validationProblemDetails.Status.Should().Be(StatusCodes.Status422UnprocessableEntity);
        validationProblemDetails.Title.Should().Be("Validation Error");
        validationProblemDetails.Detail.Should().Be("One or more validation errors occurred.");
        validationProblemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc4918#section-11.2");
        validationProblemDetails.Errors.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new
            {
                Key = "Path.Invalid",
                Value = new[] { "The provided path is invalid." }
            });
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendCombinePathCommandToMediator()
    {
        // Arrange
        CombinePathRequest request = _combinePathRequestFixture.Create("C:\\Users", "TestUser");
        CancellationToken cancellationToken = CancellationToken.None;
        _mockSender.Send(Arg.Any<CombinePathCommand>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(_pathSegmentResponseFixture.Create()));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockSender.Received(1).Send(
            Arg.Is<CombinePathCommand>(c => c.OriginalPath == request.OriginalPath && c.NewPath == request.NewPath),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CombinePathRequest request = _combinePathRequestFixture.Create("C:\\Users", "TestUser");
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockSender.Send(Arg.Any<CombinePathCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => new ValueTask<ErrorOr<PathSegmentResponse>>(Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return ErrorOrFactory.From(_pathSegmentResponseFixture.Create());
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
