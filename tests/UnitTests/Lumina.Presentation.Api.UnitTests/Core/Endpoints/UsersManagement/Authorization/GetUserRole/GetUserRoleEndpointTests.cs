#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.UsersManagement.Authorization.Queries.GetUserRole;
using Lumina.Contracts.Fixtures.Core.Requests.Authorization;
using Lumina.Contracts.Fixtures.Core.Responses.Authorization;
using Lumina.Contracts.Requests.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authorization.GetUserRole;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.UsersManagement.Authorization.GetUserRole;

/// <summary>
/// Contains unit tests for the <see cref="GetUserRoleEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserRoleEndpointTests
{
    private readonly IQueryHandler<GetUserRoleQuery, Result<RoleResponse?>> _mockHandler;
    private readonly GetUserRoleEndpoint _sut;
    private readonly RoleResponseFixture _roleResponseFixture = new();
    private readonly GetUserRoleRequestFixture _getUserRoleRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserRoleEndpointTests"/> class.
    /// </summary>
    public GetUserRoleEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetUserRoleQuery, Result<RoleResponse?>>>();
        _sut = Factory.Create<GetUserRoleEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithRole()
    {
        // Arrange
        GetUserRoleRequest request = _getUserRoleRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        RoleResponse expectedResponse = _roleResponseFixture.Create(roleName: "Admin");
        _mockHandler.HandleAsync(Arg.Any<GetUserRoleQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<RoleResponse?>(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<RoleResponse> okResult = Assert.IsType<Ok<RoleResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        GetUserRoleRequest request = _getUserRoleRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Failure("User.Role.NotFound", "User role not found.");
        _mockHandler.HandleAsync(Arg.Any<GetUserRoleQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemResult.ProblemDetails);

        Assert.Equal("User.Role.NotFound", problemResult.ProblemDetails.Title);
        Assert.Equal("User role not found.", problemResult.ProblemDetails.Detail);
        Assert.Equal(StatusCodes.Status403Forbidden, problemResult.ProblemDetails.Status);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemResult.ProblemDetails.Type);
        Assert.NotNull(problemResult.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetUserRoleQueryToSender()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        GetUserRoleRequest request = _getUserRoleRequestFixture.Create(userId: userId);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetUserRoleQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From<RoleResponse?>(_roleResponseFixture.Create(roleName: "Admin")));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetUserRoleQuery>(cmd => cmd.UserId == request.UserId),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetUserRoleQuery>(), Arg.Any<CancellationToken>())
            .Returns(info => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                info.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From<RoleResponse?>(_roleResponseFixture.Create(roleName: "Admin"));
            }, info.Arg<CancellationToken>()));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(_getUserRoleRequestFixture.Create(), cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
