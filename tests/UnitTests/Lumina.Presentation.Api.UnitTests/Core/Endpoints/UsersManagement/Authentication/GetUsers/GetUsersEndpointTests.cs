#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.UsersManagement.Authentication.Queries.GetUsers;
using Lumina.Contracts.Responses.UsersManagement.Users;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authentication.GetUsers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.UsersManagement.Authentication.GetUsers;

/// <summary>
/// Contains unit tests for the <see cref="GetUsersEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUsersEndpointTests
{
    private readonly IQueryHandler<GetUsersQuery, Result<IEnumerable<UserResponse>>> _mockHandler;
    private readonly GetUsersEndpoint _sut;
    /// <summary>
    /// Initializes a new instance of the <see cref="GetUsersEndpointTests"/> class.
    /// </summary>
    public GetUsersEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetUsersQuery, Result<IEnumerable<UserResponse>>>>();
        _sut = Factory.Create<GetUsersEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithUsers()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        IEnumerable<UserResponse> expectedResponse =
        [
        new UserResponse(Guid.NewGuid(), "testUser", DateTime.UtcNow, null)
    ];
        _mockHandler.HandleAsync(Arg.Any<GetUsersQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        Ok<IEnumerable<UserResponse>> okResult = Assert.IsType<Ok<IEnumerable<UserResponse>>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Failure("Users.NotFound", "No users found.");
        _mockHandler.HandleAsync(Arg.Any<GetUsersQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemResult.ProblemDetails);

        Assert.Equal("Users.NotFound", problemResult.ProblemDetails.Title);
        Assert.Equal("No users found.", problemResult.ProblemDetails.Detail);
        Assert.Equal(StatusCodes.Status403Forbidden, problemResult.ProblemDetails.Status);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemResult.ProblemDetails.Type);
        Assert.NotNull(problemResult.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetUsersQueryToSender()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetUsersQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(Array.Empty<UserResponse>() as IEnumerable<UserResponse>));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Any<GetUsersQuery>(),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetUsersQuery>(), Arg.Any<CancellationToken>())
            .Returns(info => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                info.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(Array.Empty<UserResponse>() as IEnumerable<UserResponse>);
            }, info.Arg<CancellationToken>()));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(EmptyRequest.Instance, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
