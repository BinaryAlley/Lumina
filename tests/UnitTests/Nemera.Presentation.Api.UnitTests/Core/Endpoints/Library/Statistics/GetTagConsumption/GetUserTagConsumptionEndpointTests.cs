#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaConsumption.Queries.GetTagConsumption;
using Lumina.Contracts.Fixtures.Core.Requests.MediaConsumption;
using Lumina.Contracts.Fixtures.Core.Responses.MediaConsumption;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.GetTagConsumption;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.Statistics.GetTagConsumption;

/// <summary>
/// Contains unit tests for the <see cref="GetUserTagConsumptionEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserTagConsumptionEndpointTests
{
    private readonly IQueryHandler<GetTagConsumptionQuery, Result<IReadOnlyList<TagConsumptionResponse>>> _mockHandler;
    private readonly GetUserTagConsumptionEndpoint _sut;
    private readonly GetTagConsumptionRequestFixture _getTagConsumptionRequestFixture = new();
    private readonly TagConsumptionResponseFixture _tagConsumptionResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserTagConsumptionEndpointTests"/> class.
    /// </summary>
    public GetUserTagConsumptionEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetTagConsumptionQuery, Result<IReadOnlyList<TagConsumptionResponse>>>>();
        _sut = FastEndpoints.Factory.Create<GetUserTagConsumptionEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithTagConsumptionResponses()
    {
        // Arrange
        GetTagConsumptionRequest request = _getTagConsumptionRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        IReadOnlyList<TagConsumptionResponse> expectedResponse = _tagConsumptionResponseFixture.CreateMany();
        _mockHandler.HandleAsync(Arg.Any<GetTagConsumptionQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));
        _sut.HttpContext.Request.RouteValues["userId"] = Guid.NewGuid().ToString();

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<IReadOnlyList<TagConsumptionResponse>> okResult = Assert.IsType<Ok<IReadOnlyList<TagConsumptionResponse>>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldSetPrivateNoStoreCacheHeaders()
    {
        // Arrange
        GetTagConsumptionRequest request = _getTagConsumptionRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetTagConsumptionQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<TagConsumptionResponse>>(_tagConsumptionResponseFixture.CreateMany()));
        _sut.HttpContext.Request.RouteValues["userId"] = Guid.NewGuid().ToString();

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Contains("Authorization", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldStillSetPrivateNoStoreCacheHeaders()
    {
        // Arrange
        GetTagConsumptionRequest request = _getTagConsumptionRequestFixture.Create();
        Error expectedError = ApplicationErrors.Authorization.NotAuthorized;
        _mockHandler.HandleAsync(Arg.Any<GetTagConsumptionQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);
        _sut.HttpContext.Request.RouteValues["userId"] = Guid.NewGuid().ToString();

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Contains("Authorization", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsValidationErrors_ShouldReturnValidationProblemResultWithErrorsGroupedByCode()
    {
        // Arrange
        GetTagConsumptionRequest request = _getTagConsumptionRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        List<Error> expectedErrors = [DomainErrors.MediaConsumption.TargetUserIdIsInvalid, DomainErrors.Library.LibraryIdCannotBeEmpty];
        _mockHandler.HandleAsync(Arg.Any<GetTagConsumptionQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedErrors);
        _sut.HttpContext.Request.RouteValues["userId"] = "not-a-user-guid";

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        HttpValidationProblemDetails validationProblemDetails = Assert.IsType<HttpValidationProblemDetails>(problemResult.ProblemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, validationProblemDetails.Status);
        Assert.Equal("General.Validation", validationProblemDetails.Title);
        Assert.Equal("OneOrMoreValidationErrorsOccurred", validationProblemDetails.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", validationProblemDetails.Type);
        Assert.NotNull(validationProblemDetails.Extensions["traceId"]);
        KeyValuePair<string, string[]> singleErrorGroup = Assert.Single(validationProblemDetails.Errors);
        Assert.Equal("General.Validation", singleErrorGroup.Key);
        Assert.Equal(new[] { "TargetUserIdIsInvalid", "LibraryIdCannotBeEmpty" }, singleErrorGroup.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotAuthorizedError_ShouldReturnForbiddenProblemResult()
    {
        // Arrange
        GetTagConsumptionRequest request = _getTagConsumptionRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = ApplicationErrors.Authorization.NotAuthorized;
        _mockHandler.HandleAsync(Arg.Any<GetTagConsumptionQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);
        _sut.HttpContext.Request.RouteValues["userId"] = Guid.NewGuid().ToString();

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails = Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemResult.ProblemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails.Status);
        Assert.Equal("General.Unauthorized", problemDetails.Title);
        Assert.Equal("NotAuthorized", problemDetails.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails.Type);
        Assert.NotNull(problemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetTagConsumptionQueryWithRouteUserIdAndMappedRequest()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string libraryId = Guid.NewGuid().ToString();
        string fromUtc = "2026-01-01T00:00:00Z";
        string toUtc = "2026-02-01T00:00:00Z";
        GetTagConsumptionRequest request = _getTagConsumptionRequestFixture.Create(
            libraryId: libraryId,
            mediaKind: nameof(MediaKind.Video),
            fromUtc: fromUtc,
            toUtc: toUtc);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetTagConsumptionQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<TagConsumptionResponse>>(_tagConsumptionResponseFixture.CreateMany()));
        _sut.HttpContext.Request.RouteValues["userId"] = userId.ToString();

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetTagConsumptionQuery>(query =>
                query.TargetUserId == userId.ToString() &&
                query.LibraryId == libraryId &&
                query.MediaKind == nameof(MediaKind.Video) &&
                query.FromUtc == fromUtc &&
                query.ToUtc == toUtc),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValueIsMissing_ShouldSendQueryWithNullTargetUserId()
    {
        // Arrange
        GetTagConsumptionRequest request = _getTagConsumptionRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetTagConsumptionQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<IReadOnlyList<TagConsumptionResponse>>(_tagConsumptionResponseFixture.CreateMany()));
        _sut.HttpContext.Request.RouteValues.Remove("userId");

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetTagConsumptionQuery>(query =>
                query.TargetUserId == null &&
                query.LibraryId == request.LibraryId &&
                query.MediaKind == request.MediaKind &&
                query.FromUtc == request.FromUtc &&
                query.ToUtc == request.ToUtc),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetTagConsumptionRequest request = _getTagConsumptionRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetTagConsumptionQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From<IReadOnlyList<TagConsumptionResponse>>(_tagConsumptionResponseFixture.CreateMany());
            }, callInfo.Arg<CancellationToken>()));
        _sut.HttpContext.Request.RouteValues["userId"] = Guid.NewGuid().ToString();

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(request, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
