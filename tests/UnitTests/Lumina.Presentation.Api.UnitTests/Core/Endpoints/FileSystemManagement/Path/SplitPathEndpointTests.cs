#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using FluentAssertions;
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
        IEnumerable<PathSegmentResponse> actualResponse = result.Should().BeOfType<Ok<IEnumerable<PathSegmentResponse>>>().Subject.Value!;
        actualResponse.Should().BeEquivalentTo(expectedResponse);
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
        result.Should().BeOfType<ProblemHttpResult>();
        ProblemHttpResult problemDetails = (ProblemHttpResult)result;
        problemDetails.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        problemDetails.ContentType.Should().Be("application/problem+json");
        problemDetails.ProblemDetails.Should().BeOfType<HttpValidationProblemDetails>();
        HttpValidationProblemDetails validationProblemDetails = (HttpValidationProblemDetails)problemDetails.ProblemDetails;
        validationProblemDetails.Status.Should().Be(StatusCodes.Status422UnprocessableEntity);
        validationProblemDetails.Title.Should().Be("General.Validation");
        validationProblemDetails.Detail.Should().Be("OneOrMoreValidationErrorsOccurred");
        validationProblemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc4918#section-11.2");
        validationProblemDetails.Errors.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new
            {
                Key = "Path.Invalid",
                Value = new[] { "The provided path is invalid." }
            });
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
