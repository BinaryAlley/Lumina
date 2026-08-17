#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Admin.Authorization.Permissions.Queries.GetPermissions;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Authorization;
using Lumina.Presentation.Api.Core.Endpoints.Admin.Authorization.Permissions.GetPermissions;
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

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Admin.Authorization.Permissions.GetPermissions;

/// <summary>
/// Contains unit tests for the <see cref="GetPermissionsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPermissionsEndpointTests
{
    private readonly IQueryHandler<GetPermissionsQuery, Result<IEnumerable<PermissionResponse>>> _mockHandler;
    private readonly GetPermissionsEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPermissionsEndpointTests"/> class.
    /// </summary>
    public GetPermissionsEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetPermissionsQuery, Result<IEnumerable<PermissionResponse>>>>();
        _sut = Factory.Create<GetPermissionsEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithPermissions()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        IEnumerable<PermissionResponse> expectedResponse =
        [
            new(Guid.NewGuid(), AuthorizationPermission.CanViewUsers),
            new(Guid.NewGuid(), AuthorizationPermission.CanDeleteUsers),
            new(Guid.NewGuid(), AuthorizationPermission.CanRegisterUsers)
        ];
        _mockHandler.HandleAsync(Arg.Any<GetPermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        Ok<IEnumerable<PermissionResponse>> okResult = Assert.IsType<Ok<IEnumerable<PermissionResponse>>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Failure("Permissions.NotFound", "No permissions found.");
        _mockHandler.HandleAsync(Arg.Any<GetPermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemResult.ProblemDetails);

        Assert.Equal("Permissions.NotFound", problemResult.ProblemDetails.Title);
        Assert.Equal("No permissions found.", problemResult.ProblemDetails.Detail);
        Assert.Equal(StatusCodes.Status403Forbidden, problemResult.ProblemDetails.Status);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemResult.ProblemDetails.Type);
        Assert.NotNull(problemResult.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetPermissionsQueryToSender()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        IEnumerable<PermissionResponse> response = [];
        _mockHandler.HandleAsync(Arg.Any<GetPermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(response));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Any<GetPermissionsQuery>(),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetPermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(info => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                info.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(Enumerable.Empty<PermissionResponse>());
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
