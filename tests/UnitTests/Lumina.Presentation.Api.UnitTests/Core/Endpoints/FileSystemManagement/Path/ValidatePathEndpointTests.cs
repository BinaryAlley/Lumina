#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.FileSystemManagement.Paths.Queries.ValidatePath;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Path.ValidatePath;
using Lumina.Presentation.Api.UnitTests.Core.Endpoints.FileSystemManagement.Fixtures;
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
/// Contains unit tests for the <see cref="ValidatePathEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ValidatePathEndpointTests
{
    private readonly IQueryHandler<ValidatePathQuery, Result<PathValidResponse>> _mockHandler;
    private readonly ValidatePathEndpoint _sut;
    private readonly ValidatePathRequestFixture _validatePathRequestFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatePathEndpointTests"/> class.
    /// </summary>
    public ValidatePathEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<ValidatePathQuery, Result<PathValidResponse>>>();
        _sut = Factory.Create<ValidatePathEndpoint>(_mockHandler);
        _validatePathRequestFixture = new ValidatePathRequestFixture();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldReturnOkResultWithPathValidResponse()
    {
        // Arrange
        ValidatePathRequest request = _validatePathRequestFixture.Create(@"C:\Users\TestUser\Documents");
        CancellationToken cancellationToken = CancellationToken.None;
        PathValidResponse expectedResponse = new(true);
        _mockHandler.HandleAsync(Arg.Any<ValidatePathQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<PathValidResponse> okResult = Assert.IsType<Ok<PathValidResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendValidatePathQueryToHandler()
    {
        // Arrange
        ValidatePathRequest request = _validatePathRequestFixture.Create(@"C:\Users\TestUser\Documents");
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<ValidatePathQuery>(), Arg.Any<CancellationToken>())
            .Returns(new PathValidResponse(true));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<ValidatePathQuery>(q => q.Path == request.Path),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsValidationError_ShouldReturnValidationProblemResult()
    {
        // Arrange
        ValidatePathRequest request = _validatePathRequestFixture.Create(@"C:\Users\TestUser\Documents");
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Validation("Path.Invalid", "The provided path is invalid.");
        _mockHandler.HandleAsync(Arg.Any<ValidatePathQuery>(), Arg.Any<CancellationToken>())
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
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        ValidatePathRequest request = _validatePathRequestFixture.Create(@"C:\Users\TestUser\Documents");
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<ValidatePathQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(new PathValidResponse(true));
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
