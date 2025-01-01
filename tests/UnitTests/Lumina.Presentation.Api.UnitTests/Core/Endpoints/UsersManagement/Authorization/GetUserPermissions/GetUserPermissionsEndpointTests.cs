#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using FluentAssertions;
using Lumina.Application.Core.UsersManagement.Authorization.Queries.GetUserPermissions;
using Lumina.Contracts.Requests.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.Common.Enums.Authorization;
using Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authorization.GetUserPermissions;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.UsersManagement.Authorization.GetUserPermissions;

/// <summary>
/// Contains unit tests for the <see cref="GetUserPermissionsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserPermissionsEndpointTests
{
    private readonly ISender _mockSender;
    private readonly GetUserPermissionsEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserPermissionsEndpointTests"/> class.
    /// </summary>
    public GetUserPermissionsEndpointTests()
    {
        _mockSender = Substitute.For<ISender>();
        _sut = Factory.Create<GetUserPermissionsEndpoint>(_mockSender);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithPermissions()
    {
        // Arrange
        GetUserPermissionsRequest request = new(Guid.NewGuid());
        CancellationToken cancellationToken = CancellationToken.None;
        IEnumerable<PermissionResponse> expectedResponse =
        [
            new PermissionResponse(Guid.NewGuid(), AuthorizationPermission.CanViewUsers),
            new PermissionResponse(Guid.NewGuid(), AuthorizationPermission.CanDeleteUsers)
        ];
        _mockSender.Send(Arg.Any<GetUserPermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<IEnumerable<PermissionResponse>> okResult = result.Should().BeOfType<Ok<IEnumerable<PermissionResponse>>>().Subject;
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediatorReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        GetUserPermissionsRequest request = new(Guid.NewGuid());
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Failure("User.Permissions.NotFound", "User permissions not found.");
        _mockSender.Send(Arg.Any<GetUserPermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        problemResult.ContentType.Should().Be("application/problem+json");
        problemResult.ProblemDetails.Should().BeOfType<Microsoft.AspNetCore.Mvc.ProblemDetails>();

        problemResult.ProblemDetails.Title.Should().Be("User.Permissions.NotFound");
        problemResult.ProblemDetails.Detail.Should().Be("User permissions not found.");
        problemResult.ProblemDetails.Status.Should().Be(StatusCodes.Status403Forbidden);
        problemResult.ProblemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.4");
        problemResult.ProblemDetails.Extensions["traceId"].Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetUserPermissionsQueryToSender()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        GetUserPermissionsRequest request = new(userId);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockSender.Send(Arg.Any<GetUserPermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<ErrorOr<IEnumerable<PermissionResponse>>>(
                ErrorOrFactory.From(Array.Empty<PermissionResponse>() as IEnumerable<PermissionResponse>)
            ));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockSender.Received(1).Send(
            Arg.Is<GetUserPermissionsQuery>(cmd => cmd.UserId == request.UserId),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockSender.Send(Arg.Any<GetUserPermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(info => new ValueTask<ErrorOr<IEnumerable<PermissionResponse>>>(
                Task.Run(async () =>
                {
                    operationStarted.SetResult(true);
                    await cancellationRequested.Task;
                    info.Arg<CancellationToken>().ThrowIfCancellationRequested();
                    return ErrorOrFactory.From(Array.Empty<PermissionResponse>() as IEnumerable<PermissionResponse>);
                }, info.Arg<CancellationToken>())
            ));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(new GetUserPermissionsRequest(Guid.NewGuid()), cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
