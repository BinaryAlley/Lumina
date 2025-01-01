#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using FluentAssertions;
using Lumina.Application.Core.UsersManagement.Authorization.Queries.GetUserRole;
using Lumina.Contracts.Requests.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authorization.GetUserRole;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.UsersManagement.Authorization.GetUserRole;

/// <summary>
/// Contains unit tests for the <see cref="GetUserRoleEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserRoleEndpointTests
{
    private readonly ISender _mockSender;
    private readonly GetUserRoleEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserRoleEndpointTests"/> class.
    /// </summary>
    public GetUserRoleEndpointTests()
    {
        _mockSender = Substitute.For<ISender>();
        _sut = Factory.Create<GetUserRoleEndpoint>(_mockSender);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithRole()
    {
        // Arrange
        GetUserRoleRequest request = new(Guid.NewGuid());
        CancellationToken cancellationToken = CancellationToken.None;
        RoleResponse expectedResponse = new(Guid.NewGuid(), "Admin");
        _mockSender.Send(Arg.Any<GetUserRoleQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<RoleResponse> okResult = result.Should().BeOfType<Ok<RoleResponse>>().Subject;
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediatorReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        GetUserRoleRequest request = new(Guid.NewGuid());
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Failure("User.Role.NotFound", "User role not found.");
        _mockSender.Send(Arg.Any<GetUserRoleQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        problemResult.ContentType.Should().Be("application/problem+json");
        problemResult.ProblemDetails.Should().BeOfType<Microsoft.AspNetCore.Mvc.ProblemDetails>();

        problemResult.ProblemDetails.Title.Should().Be("User.Role.NotFound");
        problemResult.ProblemDetails.Detail.Should().Be("User role not found.");
        problemResult.ProblemDetails.Status.Should().Be(StatusCodes.Status403Forbidden);
        problemResult.ProblemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.4");
        problemResult.ProblemDetails.Extensions["traceId"].Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetUserRoleQueryToSender()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        GetUserRoleRequest request = new(userId);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockSender.Send(Arg.Any<GetUserRoleQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(new RoleResponse(Guid.NewGuid(), "Admin")));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockSender.Received(1).Send(
            Arg.Is<GetUserRoleQuery>(cmd => cmd.UserId == request.UserId),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockSender.Send(Arg.Any<GetUserRoleQuery>(), Arg.Any<CancellationToken>())
            .Returns(info => new ValueTask<ErrorOr<RoleResponse?>>(
                Task.Run(async () =>
                {
                    operationStarted.SetResult(true);
                    await cancellationRequested.Task;
                    info.Arg<CancellationToken>().ThrowIfCancellationRequested();
                    return ErrorOrFactory.From(new RoleResponse(Guid.NewGuid(), "Admin"));
                }, info.Arg<CancellationToken>())
            ));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(new GetUserRoleRequest(Guid.NewGuid()), cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
