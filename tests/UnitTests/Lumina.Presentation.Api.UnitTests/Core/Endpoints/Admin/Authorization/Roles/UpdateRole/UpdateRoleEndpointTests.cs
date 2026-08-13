#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Admin.Authorization.Roles.Commands.UpdateRole;
using Lumina.Contracts.DTO.Authentication;
using Lumina.Contracts.Requests.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.SharedKernel.Common.Enums.Authorization;
using Lumina.Presentation.Api.Core.Endpoints.Admin.Authorization.Roles.UpdateRole;
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

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Admin.Authorization.Roles.UpdateRole;

/// <summary>
/// Contains unit tests for the <see cref="UpdateRoleEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateRoleEndpointTests
{
    private readonly ICommandHandler<UpdateRoleCommand, ErrorOr<RolePermissionsResponse>> _mockHandler;
    private readonly UpdateRoleEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateRoleEndpointTests"/> class.
    /// </summary>
    public UpdateRoleEndpointTests()
    {
        _mockHandler = Substitute.For<ICommandHandler<UpdateRoleCommand, ErrorOr<RolePermissionsResponse>>>();
        _sut = FastEndpoints.Factory.Create<UpdateRoleEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithUpdatedRole()
    {
        // Arrange
        UpdateRoleRequest request = new(Guid.NewGuid(), "UpdatedAdmin", [Guid.NewGuid()]);
        CancellationToken cancellationToken = CancellationToken.None;
        RolePermissionsResponse expectedResponse = new(
            new RoleDto(Guid.NewGuid(), "UpdatedAdmin"),
            [new PermissionDto(Guid.NewGuid(), AuthorizationPermission.CanViewUsers)]
        );
        _mockHandler.HandleAsync(Arg.Any<UpdateRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<RolePermissionsResponse> okResult = Assert.IsType<Ok<RolePermissionsResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        UpdateRoleRequest request = new(Guid.NewGuid(), "UpdatedAdmin", [Guid.NewGuid()]);
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Failure("Role.Update.Failed", "Failed to update role.");
        _mockHandler.HandleAsync(Arg.Any<UpdateRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemResult.ProblemDetails);

        Assert.Equal("Role.Update.Failed", problemResult.ProblemDetails.Title);
        Assert.Equal("Failed to update role.", problemResult.ProblemDetails.Detail);
        Assert.Equal(StatusCodes.Status403Forbidden, problemResult.ProblemDetails.Status);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemResult.ProblemDetails.Type);
        Assert.NotNull(problemResult.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendUpdateRoleCommandToSender()
    {
        // Arrange
        UpdateRoleRequest request = new(Guid.NewGuid(), "UpdatedAdmin", [Guid.NewGuid()]);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<UpdateRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(new RolePermissionsResponse(
                new RoleDto(Guid.NewGuid(), "UpdatedAdmin"),
                []
            )));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<UpdateRoleCommand>(cmd =>
                cmd.RoleId == request.RoleId &&
                cmd.RoleName == request.RoleName &&
                cmd.Permissions.SequenceEqual(request.Permissions!)),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<UpdateRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(info => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                info.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return ErrorOrFactory.From(new RolePermissionsResponse(
                    new RoleDto(Guid.NewGuid(), "UpdatedAdmin"),
                    []
                ));
            }, info.Arg<CancellationToken>()));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(
            new UpdateRoleRequest(Guid.NewGuid(), "UpdatedAdmin", [Guid.NewGuid()]),
            cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
