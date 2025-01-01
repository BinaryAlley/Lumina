#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using FluentAssertions;
using Lumina.Application.Core.Admin.Authorization.Roles.Commands.UpdateRole;
using Lumina.Contracts.DTO.Authentication;
using Lumina.Contracts.Requests.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.Common.Enums.Authorization;
using Lumina.Presentation.Api.Core.Endpoints.Admin.Authorization.Roles.UpdateRole;
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

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Admin.Authorization.Roles.UpdateRole;

/// <summary>
/// Contains unit tests for the <see cref="UpdateRoleEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateRoleEndpointTests
{
    private readonly ISender _mockSender;
    private readonly UpdateRoleEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateRoleEndpointTests"/> class.
    /// </summary>
    public UpdateRoleEndpointTests()
    {
        _mockSender = Substitute.For<ISender>();
        _sut = Factory.Create<UpdateRoleEndpoint>(_mockSender);
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
        _mockSender.Send(Arg.Any<UpdateRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<RolePermissionsResponse> okResult = result.Should().BeOfType<Ok<RolePermissionsResponse>>().Subject;
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediatorReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        UpdateRoleRequest request = new(Guid.NewGuid(), "UpdatedAdmin", [Guid.NewGuid()]);
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Failure("Role.Update.Failed", "Failed to update role.");
        _mockSender.Send(Arg.Any<UpdateRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        problemResult.ContentType.Should().Be("application/problem+json");
        problemResult.ProblemDetails.Should().BeOfType<Microsoft.AspNetCore.Mvc.ProblemDetails>();

        problemResult.ProblemDetails.Title.Should().Be("Role.Update.Failed");
        problemResult.ProblemDetails.Detail.Should().Be("Failed to update role.");
        problemResult.ProblemDetails.Status.Should().Be(StatusCodes.Status403Forbidden);
        problemResult.ProblemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.4");
        problemResult.ProblemDetails.Extensions["traceId"].Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendUpdateRoleCommandToSender()
    {
        // Arrange
        UpdateRoleRequest request = new(Guid.NewGuid(), "UpdatedAdmin", [Guid.NewGuid()]);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockSender.Send(Arg.Any<UpdateRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(new RolePermissionsResponse(
                new RoleDto(Guid.NewGuid(), "UpdatedAdmin"),
                []
            )));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockSender.Received(1).Send(
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

        _mockSender.Send(Arg.Any<UpdateRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(info => new ValueTask<ErrorOr<RolePermissionsResponse>>(
                Task.Run(async () =>
                {
                    operationStarted.SetResult(true);
                    await cancellationRequested.Task;
                    info.Arg<CancellationToken>().ThrowIfCancellationRequested();
                    return ErrorOrFactory.From(new RolePermissionsResponse(
                        new RoleDto(Guid.NewGuid(), "UpdatedAdmin"),
                        []
                    ));
                }, info.Arg<CancellationToken>())
            ));

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
