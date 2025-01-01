#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using FluentAssertions;
using Lumina.Application.Core.UsersManagement.Authentication.Queries.GetUsers;
using Lumina.Contracts.Responses.UsersManagement.Users;
using Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authentication.GetUsers;
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

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.UsersManagement.Authentication.GetUsers;

/// <summary>
/// Contains unit tests for the <see cref="GetUsersEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUsersEndpointTests
{
    private readonly ISender _mockSender;
    private readonly GetUsersEndpoint _sut;
    /// <summary>
    /// Initializes a new instance of the <see cref="GetUsersEndpointTests"/> class.
    /// </summary>
    public GetUsersEndpointTests()
    {
        _mockSender = Substitute.For<ISender>();
        _sut = Factory.Create<GetUsersEndpoint>(_mockSender);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithUsers()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        IEnumerable<UserResponse> expectedResponse =
        [
        new UserResponse(Guid.NewGuid(), "testUser", DateTime.UtcNow, null)
    ];
        _mockSender.Send(Arg.Any<GetUsersQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(new EmptyRequest(), cancellationToken);

        // Assert
        Ok<IEnumerable<UserResponse>> okResult = result.Should().BeOfType<Ok<IEnumerable<UserResponse>>>().Subject;
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediatorReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Failure("Users.NotFound", "No users found.");
        _mockSender.Send(Arg.Any<GetUsersQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(new EmptyRequest(), cancellationToken);

        // Assert
        ProblemHttpResult problemResult = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        problemResult.ContentType.Should().Be("application/problem+json");
        problemResult.ProblemDetails.Should().BeOfType<Microsoft.AspNetCore.Mvc.ProblemDetails>();

        problemResult.ProblemDetails.Title.Should().Be("Users.NotFound");
        problemResult.ProblemDetails.Detail.Should().Be("No users found.");
        problemResult.ProblemDetails.Status.Should().Be(StatusCodes.Status403Forbidden);
        problemResult.ProblemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.4");
        problemResult.ProblemDetails.Extensions["traceId"].Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetUsersQueryToSender()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockSender.Send(Arg.Any<GetUsersQuery>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<ErrorOr<IEnumerable<UserResponse>>>(
                ErrorOrFactory.From(Array.Empty<UserResponse>() as IEnumerable<UserResponse>)
            ));

        // Act
        await _sut.ExecuteAsync(new EmptyRequest(), cancellationToken);

        // Assert
        await _mockSender.Received(1).Send(
            Arg.Any<GetUsersQuery>(),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockSender.Send(Arg.Any<GetUsersQuery>(), Arg.Any<CancellationToken>())
            .Returns(info => new ValueTask<ErrorOr<IEnumerable<UserResponse>>>(
                Task.Run(async () =>
                {
                    operationStarted.SetResult(true);
                    await cancellationRequested.Task;
                    info.Arg<CancellationToken>().ThrowIfCancellationRequested();
                    return ErrorOrFactory.From(Array.Empty<UserResponse>() as IEnumerable<UserResponse>);
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
