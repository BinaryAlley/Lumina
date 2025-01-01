#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using FluentAssertions;
using Lumina.Application.Core.UsersManagement.Authorization.Commands.UpdateUserRoleAndPermissions;
using Lumina.Contracts.Requests.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.Common.Enums.Authorization;
using Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authorization.UpdateUserRoleAndPermissions;
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

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.UsersManagement.Authorization.UpdateUserRoleAndPermissions;

/// <summary>
/// Contains unit tests for the <see cref="UpdateUserRoleAndPermissionsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateUserRoleAndPermissionsEndpointTests
{
    private readonly ISender _mockSender;
    private readonly UpdateUserRoleAndPermissionsEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserRoleAndPermissionsEndpointTests"/> class.
    /// </summary>
    public UpdateUserRoleAndPermissionsEndpointTests()
    {
        _mockSender = Substitute.For<ISender>();
        _sut = Factory.Create<UpdateUserRoleAndPermissionsEndpoint>(_mockSender);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithAuthorization()
    {
        // Arrange
        UpdateUserRoleAndPermissionsRequest request = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            [Guid.NewGuid()]
        );
        CancellationToken cancellationToken = CancellationToken.None;
        AuthorizationResponse expectedResponse = new(
            Guid.NewGuid(),
            "Admin",
            new HashSet<AuthorizationPermission> { AuthorizationPermission.CanViewUsers }
        );
        _mockSender.Send(Arg.Any<UpdateUserRoleAndPermissionsCommand>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<AuthorizationResponse> okResult = result.Should().BeOfType<Ok<AuthorizationResponse>>().Subject;
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediatorReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        UpdateUserRoleAndPermissionsRequest request = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            [Guid.NewGuid()]
        );
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Failure("Authorization.Update.Failed", "Failed to update user role and permissions.");
        _mockSender.Send(Arg.Any<UpdateUserRoleAndPermissionsCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        problemResult.ContentType.Should().Be("application/problem+json");
        problemResult.ProblemDetails.Should().BeOfType<Microsoft.AspNetCore.Mvc.ProblemDetails>();

        problemResult.ProblemDetails.Title.Should().Be("Authorization.Update.Failed");
        problemResult.ProblemDetails.Detail.Should().Be("Failed to update user role and permissions.");
        problemResult.ProblemDetails.Status.Should().Be(StatusCodes.Status403Forbidden);
        problemResult.ProblemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.4");
        problemResult.ProblemDetails.Extensions["traceId"].Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendUpdateUserRoleAndPermissionsCommandToSender()
    {
        // Arrange
        UpdateUserRoleAndPermissionsRequest request = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            [Guid.NewGuid()]
        );
        CancellationToken cancellationToken = CancellationToken.None;
        _mockSender.Send(Arg.Any<UpdateUserRoleAndPermissionsCommand>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(new AuthorizationResponse(
                Guid.NewGuid(),
                "Admin",
                new HashSet<AuthorizationPermission> { AuthorizationPermission.CanViewUsers }
            )));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockSender.Received(1).Send(
            Arg.Is<UpdateUserRoleAndPermissionsCommand>(cmd =>
                cmd.UserId == request.UserId &&
                cmd.RoleId == request.RoleId &&
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

        _mockSender.Send(Arg.Any<UpdateUserRoleAndPermissionsCommand>(), Arg.Any<CancellationToken>())
            .Returns(info => new ValueTask<ErrorOr<AuthorizationResponse>>(
                Task.Run(async () =>
                {
                    operationStarted.SetResult(true);
                    await cancellationRequested.Task;
                    info.Arg<CancellationToken>().ThrowIfCancellationRequested();
                    return ErrorOrFactory.From(new AuthorizationResponse(
                        Guid.NewGuid(),
                        "Admin",
                        new HashSet<AuthorizationPermission> { AuthorizationPermission.CanViewUsers }
                    ));
                }, info.Arg<CancellationToken>())
            ));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(
            new UpdateUserRoleAndPermissionsRequest(Guid.NewGuid(), Guid.NewGuid(), [Guid.NewGuid()]),
            cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
