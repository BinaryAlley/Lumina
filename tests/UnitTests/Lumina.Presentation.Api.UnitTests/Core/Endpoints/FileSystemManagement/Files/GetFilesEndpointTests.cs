#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ErrorOr;
using FastEndpoints;
using FluentAssertions;
using Lumina.Application.Core.FileSystemManagement.Files.Queries.GetFiles;
using Lumina.Contracts.Requests.FileSystemManagement.Files;
using Lumina.Contracts.Responses.FileSystemManagement.Files;
using Lumina.Presentation.Api.Common.Http;
using Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Files.GetFiles;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.FileSystemManagement.Files;

/// <summary>
/// Contains unit tests for the <see cref="GetFilesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFilesEndpointTests
{
    private readonly IFixture _fixture;
    private readonly ISender _mockSender;
    private readonly GetFilesEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFilesEndpointTests"/> class.
    /// </summary>
    public GetFilesEndpointTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _mockSender = Substitute.For<ISender>();
        _sut = Factory.Create<GetFilesEndpoint>(_mockSender);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldReturnOkResultWithFileResponses()
    {
        // Arrange
        GetFilesRequest request = _fixture.Create<GetFilesRequest>();
        CancellationToken cancellationToken = CancellationToken.None;
        List<FileResponse> expectedResponses = _fixture.CreateMany<FileResponse>(3).ToList();
        _mockSender.Send(Arg.Any<GetFilesQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponses.AsEnumerable()));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        IEnumerable<FileResponse> actualResponses = result.Should().BeOfType<Ok<IEnumerable<FileResponse>>>().Subject.Value!;
        actualResponses.Should().BeEquivalentTo(expectedResponses);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediatorReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        GetFilesRequest request = _fixture.Create<GetFilesRequest>();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.NotFound("File.NotFound", "The requested file was not found.");
        _mockSender.Send(Arg.Any<GetFilesQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        result.Should().BeOfType<ProblemHttpResult>();
        ProblemHttpResult problemDetails = (ProblemHttpResult)result;
        problemDetails.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.ContentType.Should().Be("application/problem+json");
        problemDetails.ProblemDetails.Should().BeOfType<Microsoft.AspNetCore.Mvc.ProblemDetails>();

        problemDetails.ProblemDetails.Title.Should().Be("File.NotFound");
        problemDetails.ProblemDetails.Detail.Should().Be("The requested file was not found.");
        problemDetails.ProblemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.ProblemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.5");
        problemDetails.ProblemDetails.Extensions["traceId"].Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediatorReturnsValidationError_ShouldReturnValidationProblemResult()
    {
        // Arrange
        GetFilesRequest request = _fixture.Create<GetFilesRequest>();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Validation("Path.Invalid", "The provided path is invalid.");
        _mockSender.Send(Arg.Any<GetFilesQuery>(), Arg.Any<CancellationToken>())
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
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetFilesQueryToSender()
    {
        // Arrange
        GetFilesRequest request = _fixture.Create<GetFilesRequest>();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockSender.Send(Arg.Any<GetFilesQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(Enumerable.Empty<FileResponse>()));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockSender.Received(1).Send(Arg.Is<GetFilesQuery>(q =>
            q.Path == request.Path &&
            q.IncludeHiddenElements == request.IncludeHiddenElements),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetFilesRequest request = _fixture.Create<GetFilesRequest>();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockSender.Send(Arg.Any<GetFilesQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => new ValueTask<ErrorOr<IEnumerable<FileResponse>>>(Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return ErrorOrFactory.From(_fixture.CreateMany<FileResponse>(3).AsEnumerable());
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
