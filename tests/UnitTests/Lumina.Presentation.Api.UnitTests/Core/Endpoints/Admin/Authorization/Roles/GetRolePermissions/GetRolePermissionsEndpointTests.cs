#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Admin.Authorization.Roles.Queries.GetRolePermissions;
using Lumina.Contracts.Fixtures.Core.DTO.Authentication;
using Lumina.Contracts.Fixtures.Core.Requests.Authorization;
using Lumina.Contracts.Fixtures.Core.Responses.Authorization;
using Lumina.Contracts.Requests.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Authorization;
using Lumina.Presentation.Api.Core.Endpoints.Admin.Authorization.Roles.GetRolePermissions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Admin.Authorization.Roles.GetRolePermissions;

/// <summary>
/// Contains unit tests for the <see cref="GetRolePermissionsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRolePermissionsEndpointTests
{
    private readonly IQueryHandler<GetRolePermissionsQuery, Result<RolePermissionsResponse>> _mockHandler;
    private readonly GetRolePermissionsEndpoint _sut;
    private readonly RolePermissionsResponseFixture _rolePermissionsResponseFixture = new();
    private readonly RoleDtoFixture _roleDtoFixture = new();
    private readonly PermissionDtoFixture _permissionDtoFixture = new();
    private readonly GetRolePermissionsRequestFixture _getRolePermissionsRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRolePermissionsEndpointTests"/> class.
    /// </summary>
    public GetRolePermissionsEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetRolePermissionsQuery, Result<RolePermissionsResponse>>>();
        _sut = FastEndpoints.Factory.Create<GetRolePermissionsEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithPermissions()
    {
        // Arrange
        GetRolePermissionsRequest request = _getRolePermissionsRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        RolePermissionsResponse expectedResponse = _rolePermissionsResponseFixture.Create(
            role: _roleDtoFixture.Create(roleName: "Admin"),
            permissions: [_permissionDtoFixture.Create(permissionName: AuthorizationPermission.CanViewUsers)]
        );
        _mockHandler.HandleAsync(Arg.Any<GetRolePermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

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
        GetRolePermissionsRequest request = _getRolePermissionsRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Failure("Role.Permissions.NotFound", "Role permissions not found.");
        _mockHandler.HandleAsync(Arg.Any<GetRolePermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemResult.ProblemDetails);

        Assert.Equal("Role.Permissions.NotFound", problemResult.ProblemDetails.Title);
        Assert.Equal("Role permissions not found.", problemResult.ProblemDetails.Detail);
        Assert.Equal(StatusCodes.Status403Forbidden, problemResult.ProblemDetails.Status);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemResult.ProblemDetails.Type);
        Assert.NotNull(problemResult.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetRolePermissionsQueryToSender()
    {
        // Arrange
        Guid roleId = Guid.NewGuid();
        GetRolePermissionsRequest request = _getRolePermissionsRequestFixture.Create(roleId: roleId);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetRolePermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_rolePermissionsResponseFixture.Create(
                role: _roleDtoFixture.Create(roleName: "Admin"),
                permissions: []
            )));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetRolePermissionsQuery>(cmd => cmd.RoleId == request.RoleId),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetRolePermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(info => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                info.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_rolePermissionsResponseFixture.Create(
                    role: _roleDtoFixture.Create(roleName: "Admin"),
                    permissions: []
                ));
            }, info.Arg<CancellationToken>()));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(_getRolePermissionsRequestFixture.Create(), cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
