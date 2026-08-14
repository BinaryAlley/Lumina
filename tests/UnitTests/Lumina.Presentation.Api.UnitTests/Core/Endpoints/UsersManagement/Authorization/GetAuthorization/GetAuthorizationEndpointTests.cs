#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.UsersManagement.Authorization.Queries.GetAuthorization;
using Lumina.Contracts.Requests.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Authorization;
using Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authorization.GetAuthorization;
using Lumina.Presentation.Api.UnitTests.Core.Endpoints.UsersManagement.Authorization.GetAuthorization.Fixtures;
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

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.UsersManagement.Authorization.GetAuthorization;

/// <summary>
/// Contains unit tests for the <see cref="GetAuthorizationEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAuthorizationEndpointTests
{
    private readonly IQueryHandler<GetAuthorizationQuery, Result<AuthorizationResponse>> _mockHandler;
    private readonly GetAuthorizationEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAuthorizationEndpointTests"/> class.
    /// </summary>
    public GetAuthorizationEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetAuthorizationQuery, Result<AuthorizationResponse>>>();
        _sut = Factory.Create<GetAuthorizationEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithAuthorizationResponse()
    {
        // Arrange
        GetAuthorizationRequest request = GetAuthorizationRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        AuthorizationResponse expectedResponse = new(
            request.UserId!.Value,
            "Admin",
            new HashSet<AuthorizationPermission> { AuthorizationPermission.CanViewUsers });

        _mockHandler.HandleAsync(Arg.Any<GetAuthorizationQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<AuthorizationResponse> okResult = Assert.IsType<Ok<AuthorizationResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        GetAuthorizationRequest request = GetAuthorizationRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Unauthorized("Authorization.Failed", "User is not authorized.");

        _mockHandler.HandleAsync(Arg.Any<GetAuthorizationQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemDetails.ProblemDetails);

        Assert.Equal("Authorization.Failed", problemDetails.ProblemDetails.Title);
        Assert.Equal("User is not authorized.", problemDetails.ProblemDetails.Detail);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails.ProblemDetails.Status);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails.ProblemDetails.Type);
        Assert.NotNull(problemDetails.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetAuthorizationQueryToSender()
    {
        // Arrange
        GetAuthorizationRequest request = GetAuthorizationRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;

        _mockHandler.HandleAsync(Arg.Any<GetAuthorizationQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(new AuthorizationResponse(
                request.UserId!.Value,
                string.Empty,
                new HashSet<AuthorizationPermission>().ToHashSet()
            )));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetAuthorizationQuery>(query =>
                query.UserId == request.UserId),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetAuthorizationRequest request = GetAuthorizationRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetAuthorizationQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(new AuthorizationResponse(
                    request.UserId!.Value,
                    string.Empty,
                    new HashSet<AuthorizationPermission>().ToHashSet()
                ));
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
