#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ErrorOr;
using FastEndpoints;
using FluentAssertions;
using Lumina.Application.Core.FileSystemManagement.Directories.Queries.GetDirectories;
using Lumina.Contracts.Requests.FileSystemManagement.Directories;
using Lumina.Contracts.Responses.FileSystemManagement.Directories;
using Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Directories.GetDirectories;
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

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.FileSystemManagement.Directories;

/// <summary>
/// Contains unit tests for the <see cref="GetDirectoriesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDirectoriesEndpointTests
{
    private readonly IFixture _fixture;
    private readonly ISender _mockSender;
    private readonly GetDirectoriesEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDirectoriesEndpointTests"/> class.
    /// </summary>
    public GetDirectoriesEndpointTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _mockSender = Substitute.For<ISender>();
        _sut = Factory.Create<GetDirectoriesEndpoint>(_mockSender);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldReturnOkResultWithDirectoryResponses()
    {
        // Arrange
        GetDirectoriesRequest request = _fixture.Create<GetDirectoriesRequest>();
        CancellationToken cancellationToken = CancellationToken.None;
        List<DirectoryResponse> expectedResponses = _fixture.CreateMany<DirectoryResponse>(3).ToList();
        _mockSender.Send(Arg.Any<GetDirectoriesQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponses.AsEnumerable()));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        IEnumerable<DirectoryResponse> actualResponses = result.Should().BeOfType<Ok<IEnumerable<DirectoryResponse>>>().Subject.Value!;
        actualResponses.Should().BeEquivalentTo(expectedResponses);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediatorReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        GetDirectoriesRequest request = _fixture.Create<GetDirectoriesRequest>();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.NotFound("Directory.NotFound", "The requested directory was not found.");
        _mockSender.Send(Arg.Any<GetDirectoriesQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        result.Should().BeOfType<ProblemHttpResult>();
        ProblemHttpResult problemDetails = (ProblemHttpResult)result;
        problemDetails.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.ContentType.Should().Be("application/problem+json");
        problemDetails.ProblemDetails.Should().BeOfType<Microsoft.AspNetCore.Mvc.ProblemDetails>();
        
        problemDetails.ProblemDetails.Title.Should().Be("Directory.NotFound");
        problemDetails.ProblemDetails.Detail.Should().Be("The requested directory was not found.");
        problemDetails.ProblemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.ProblemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.5");
        problemDetails.ProblemDetails.Extensions["traceId"].Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediatorReturnsValidationError_ShouldReturnValidationProblemResult()
    {
        // Arrange
        GetDirectoriesRequest request = _fixture.Create<GetDirectoriesRequest>();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Validation("Path.Invalid", "The provided path is invalid.");
        _mockSender.Send(Arg.Any<GetDirectoriesQuery>(), Arg.Any<CancellationToken>())
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
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetDirectoriesQueryToMediator()
    {
        // Arrange
        GetDirectoriesRequest request = _fixture.Create<GetDirectoriesRequest>();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockSender.Send(Arg.Any<GetDirectoriesQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(Enumerable.Empty<DirectoryResponse>()));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockSender.Received(1).Send(
            Arg.Is<GetDirectoriesQuery>(q => q.Path == request.Path && q.IncludeHiddenElements == request.IncludeHiddenElements),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetDirectoriesRequest request = _fixture.Create<GetDirectoriesRequest>();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockSender.Send(Arg.Any<GetDirectoriesQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => new ValueTask<ErrorOr<IEnumerable<DirectoryResponse>>>(Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return ErrorOrFactory.From(_fixture.CreateMany<DirectoryResponse>(3).AsEnumerable());
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
