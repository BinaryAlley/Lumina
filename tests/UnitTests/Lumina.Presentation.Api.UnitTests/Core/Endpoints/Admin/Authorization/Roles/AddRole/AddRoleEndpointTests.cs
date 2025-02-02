#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using Lumina.Application.Core.Admin.Authorization.Permissions.Queries.GetPermissions;
using Lumina.Application.Core.Admin.Authorization.Roles.Commands.AddRole;
using Lumina.Application.Core.UsersManagement.Authorization.Queries.GetAuthorization;
using Lumina.Contracts.DTO.Authentication;
using Lumina.Contracts.Requests.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.Common.Enums.Authorization;
using Lumina.Presentation.Api.Core.Endpoints.Admin.Authorization.Permissions.GetPermissions;
using Lumina.Presentation.Api.Core.Endpoints.Admin.Authorization.Roles.AddRole;
using Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authorization.GetAuthorization;
using Lumina.Presentation.Api.UnitTests.Core.Endpoints.UsersManagement.Authorization.GetAuthorization.Fixtures;
using Mediator;
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

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Admin.Authorization.Roles.AddRole;

/// <summary>
/// Contains unit tests for the <see cref="AddRoleEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddRoleEndpointTests
{
    private readonly ISender _mockSender;
    private readonly AddRoleEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddRoleEndpointTests"/> class.
    /// </summary>
    public AddRoleEndpointTests()
    {
        _mockSender = Substitute.For<ISender>();
        _sut = Factory.Create<AddRoleEndpoint>(_mockSender);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithRoleResponse()
    {
        // Arrange
        AddRoleRequest request = new("Admin", [Guid.NewGuid()]);
        CancellationToken cancellationToken = CancellationToken.None;
        RolePermissionsResponse expectedResponse = new(new RoleDto(Guid.NewGuid(), "Admin"), []);
        _mockSender.Send(Arg.Any<AddRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<RolePermissionsResponse> okResult = Assert.IsType<Ok<RolePermissionsResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediatorReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        AddRoleRequest request = new("Admin", [Guid.NewGuid()]);
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Failure("Role.Creation.Failed", "Failed to create role.");
        _mockSender.Send(Arg.Any<AddRoleCommand>(), Arg.Any<CancellationToken>())
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
        AddRoleRequest request = new("Admin", [Guid.NewGuid()]);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockSender.Send(Arg.Any<AddRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(new RolePermissionsResponse(new RoleDto(Guid.NewGuid(), "Admin"), [])));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockSender.Received(1).Send(
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

        _mockSender.Send(Arg.Any<AddRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(info => new ValueTask<ErrorOr<RolePermissionsResponse>>(
                Task.Run(async () =>
                {
                    operationStarted.SetResult(true);
                    await cancellationRequested.Task;
                    info.Arg<CancellationToken>().ThrowIfCancellationRequested();
                    return ErrorOrFactory.From(new RolePermissionsResponse(new RoleDto(Guid.NewGuid(), "Admin"), []));
                }, info.Arg<CancellationToken>())
            ));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(new AddRoleRequest("Admin", []), cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
