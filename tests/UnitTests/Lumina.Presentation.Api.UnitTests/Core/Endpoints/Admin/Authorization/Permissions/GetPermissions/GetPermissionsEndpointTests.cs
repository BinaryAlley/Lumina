#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using FluentAssertions;
using Lumina.Application.Core.Admin.Authorization.Permissions.Queries.GetPermissions;
using Lumina.Application.Core.UsersManagement.Authorization.Queries.GetAuthorization;
using Lumina.Contracts.Requests.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.Common.Enums.Authorization;
using Lumina.Presentation.Api.Core.Endpoints.Admin.Authorization.Permissions.GetPermissions;
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

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Admin.Authorization.Permissions.GetPermissions;

/// <summary>
/// Contains unit tests for the <see cref="GetPermissionsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPermissionsEndpointTests
{
    private readonly ISender _mockSender;
    private readonly GetPermissionsEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPermissionsEndpointTests"/> class.
    /// </summary>
    public GetPermissionsEndpointTests()
    {
        _mockSender = Substitute.For<ISender>();
        _sut = Factory.Create<GetPermissionsEndpoint>(_mockSender);
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
        _mockSender.Send(Arg.Any<GetPermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(new EmptyRequest(), cancellationToken);

        // Assert
        Ok<IEnumerable<PermissionResponse>> okResult = result.Should().BeOfType<Ok<IEnumerable<PermissionResponse>>>().Subject;
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediatorReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Failure("Permissions.NotFound", "No permissions found.");
        _mockSender.Send(Arg.Any<GetPermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(new EmptyRequest(), cancellationToken);

        // Assert
        ProblemHttpResult problemResult = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        problemResult.ContentType.Should().Be("application/problem+json");
        problemResult.ProblemDetails.Should().BeOfType<Microsoft.AspNetCore.Mvc.ProblemDetails>();

        problemResult.ProblemDetails.Title.Should().Be("Permissions.NotFound");
        problemResult.ProblemDetails.Detail.Should().Be("No permissions found.");
        problemResult.ProblemDetails.Status.Should().Be(StatusCodes.Status403Forbidden);
        problemResult.ProblemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.4");
        problemResult.ProblemDetails.Extensions["traceId"].Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetPermissionsQueryToSender()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        IEnumerable<PermissionResponse> response = [];
        _mockSender.Send(Arg.Any<GetPermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<ErrorOr<IEnumerable<PermissionResponse>>>(
                ErrorOrFactory.From(response)
            ));

        // Act
        await _sut.ExecuteAsync(new EmptyRequest(), cancellationToken);

        // Assert
        await _mockSender.Received(1).Send(
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

        _mockSender.Send(Arg.Any<GetPermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(info => new ValueTask<ErrorOr<IEnumerable<PermissionResponse>>>(
                Task.Run(async () =>
                {
                    operationStarted.SetResult(true);
                    await cancellationRequested.Task;
                    info.Arg<CancellationToken>().ThrowIfCancellationRequested();
                    return ErrorOrFactory.From(Enumerable.Empty<PermissionResponse>());
                }, info.Arg<CancellationToken>())
            ));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(new EmptyRequest(), cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
