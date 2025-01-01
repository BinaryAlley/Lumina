#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using FluentAssertions;
using Lumina.Application.Core.Admin.Authorization.Roles.Queries.GetRolePermissions;
using Lumina.Contracts.DTO.Authentication;
using Lumina.Contracts.Requests.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.Common.Enums.Authorization;
using Lumina.Presentation.Api.Core.Endpoints.Admin.Authorization.Roles.GetRolePermissions;
using Mediator;
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
    private readonly ISender _mockSender;
    private readonly GetRolePermissionsEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRolePermissionsEndpointTests"/> class.
    /// </summary>
    public GetRolePermissionsEndpointTests()
    {
        _mockSender = Substitute.For<ISender>();
        _sut = Factory.Create<GetRolePermissionsEndpoint>(_mockSender);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithPermissions()
    {
        // Arrange
        GetRolePermissionsRequest request = new(Guid.NewGuid());
        CancellationToken cancellationToken = CancellationToken.None;
        RolePermissionsResponse expectedResponse = new(
            new RoleDto(Guid.NewGuid(), "Admin"),
            [new PermissionDto(Guid.NewGuid(), AuthorizationPermission.CanViewUsers)]
        );
        _mockSender.Send(Arg.Any<GetRolePermissionsQuery>(), Arg.Any<CancellationToken>())
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
        GetRolePermissionsRequest request = new(Guid.NewGuid());
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Failure("Role.Permissions.NotFound", "Role permissions not found.");
        _mockSender.Send(Arg.Any<GetRolePermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        problemResult.ContentType.Should().Be("application/problem+json");
        problemResult.ProblemDetails.Should().BeOfType<Microsoft.AspNetCore.Mvc.ProblemDetails>();

        problemResult.ProblemDetails.Title.Should().Be("Role.Permissions.NotFound");
        problemResult.ProblemDetails.Detail.Should().Be("Role permissions not found.");
        problemResult.ProblemDetails.Status.Should().Be(StatusCodes.Status403Forbidden);
        problemResult.ProblemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.4");
        problemResult.ProblemDetails.Extensions["traceId"].Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetRolePermissionsQueryToSender()
    {
        // Arrange
        Guid roleId = Guid.NewGuid();
        GetRolePermissionsRequest request = new(roleId);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockSender.Send(Arg.Any<GetRolePermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(new RolePermissionsResponse(
                new RoleDto(Guid.NewGuid(), "Admin"),
                []
            )));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockSender.Received(1).Send(
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

        _mockSender.Send(Arg.Any<GetRolePermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(info => new ValueTask<ErrorOr<RolePermissionsResponse>>(
                Task.Run(async () =>
                {
                    operationStarted.SetResult(true);
                    await cancellationRequested.Task;
                    info.Arg<CancellationToken>().ThrowIfCancellationRequested();
                    return ErrorOrFactory.From(new RolePermissionsResponse(
                        new RoleDto(Guid.NewGuid(), "Admin"),
                        []
                    ));
                }, info.Arg<CancellationToken>())
            ));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(new GetRolePermissionsRequest(Guid.NewGuid()), cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
