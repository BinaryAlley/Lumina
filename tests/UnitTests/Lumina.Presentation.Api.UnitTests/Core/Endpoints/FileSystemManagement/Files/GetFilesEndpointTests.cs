#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using ErrorOr;
using FastEndpoints;
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
        Ok<IEnumerable<FileResponse>> okResult = Assert.IsType<Ok<IEnumerable<FileResponse>>>(result);
        Assert.Equal(expectedResponses, okResult.Value);
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
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemDetails.ProblemDetails);

        Assert.Equal("File.NotFound", problemDetails.ProblemDetails.Title);
        Assert.Equal("The requested file was not found.", problemDetails.ProblemDetails.Detail);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.ProblemDetails.Status);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problemDetails.ProblemDetails.Type);
        Assert.NotNull(problemDetails.ProblemDetails.Extensions["traceId"]);
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
