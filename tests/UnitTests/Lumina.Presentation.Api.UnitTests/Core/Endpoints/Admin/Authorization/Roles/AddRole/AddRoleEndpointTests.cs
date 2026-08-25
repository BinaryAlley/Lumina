#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Admin.Authorization.Roles.Commands.AddRole;
using Lumina.Contracts.Fixtures.Core.DTO.Authentication;
using Lumina.Contracts.Fixtures.Core.Requests.Authorization;
using Lumina.Contracts.Fixtures.Core.Responses.Authorization;
using Lumina.Contracts.Requests.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Admin.Authorization.Roles.AddRole;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Admin.Authorization.Roles.AddRole;

/// <summary>
/// Contains unit tests for the <see cref="AddRoleEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddRoleEndpointTests
{
    private readonly ICommandHandler<AddRoleCommand, Result<RolePermissionsResponse>> _mockHandler;
    private readonly AddRoleEndpoint _sut;
    private readonly RolePermissionsResponseFixture _rolePermissionsResponseFixture = new();
    private readonly RoleDtoFixture _roleDtoFixture = new();
    private readonly AddRoleRequestFixture _addRoleRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AddRoleEndpointTests"/> class.
    /// </summary>
    public AddRoleEndpointTests()
    {
        _mockHandler = Substitute.For<ICommandHandler<AddRoleCommand, Result<RolePermissionsResponse>>>();
        _sut = FastEndpoints.Factory.Create<AddRoleEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithRoleResponse()
    {
        // Arrange
        AddRoleRequest request = _addRoleRequestFixture.Create(roleName: "Admin", permissions: [Guid.NewGuid()]);
        CancellationToken cancellationToken = CancellationToken.None;
        RolePermissionsResponse expectedResponse = _rolePermissionsResponseFixture.Create(
            role: _roleDtoFixture.Create(roleName: "Admin"),
            permissions: []);
        _mockHandler.HandleAsync(Arg.Any<AddRoleCommand>(), Arg.Any<CancellationToken>())
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
        AddRoleRequest request = _addRoleRequestFixture.Create(roleName: "Admin", permissions: [Guid.NewGuid()]);
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Failure("Role.Creation.Failed", "Failed to create role.");
        _mockHandler.HandleAsync(Arg.Any<AddRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemResult.ProblemDetails);

        Assert.Equal("Role.Creation.Failed", problemResult.ProblemDetails.Title);
        Assert.Equal("Failed to create role.", problemResult.ProblemDetails.Detail);
        Assert.Equal(StatusCodes.Status403Forbidden, problemResult.ProblemDetails.Status);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemResult.ProblemDetails.Type);
        Assert.NotNull(problemResult.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendAddRoleCommandToSender()
    {
        // Arrange
        AddRoleRequest request = _addRoleRequestFixture.Create(roleName: "Admin", permissions: [Guid.NewGuid()]);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<AddRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_rolePermissionsResponseFixture.Create(
                role: _roleDtoFixture.Create(roleName: "Admin"),
                permissions: [])));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<AddRoleCommand>(cmd =>
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

        _mockHandler.HandleAsync(Arg.Any<AddRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(info => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                info.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_rolePermissionsResponseFixture.Create(
                    role: _roleDtoFixture.Create(roleName: "Admin"),
                    permissions: []));
            }, info.Arg<CancellationToken>()));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(_addRoleRequestFixture.Create(roleName: "Admin", permissions: []), cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
