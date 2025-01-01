#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using FluentAssertions;
using Lumina.Application.Core.Admin.Authorization.Roles.Commands.DeleteRole;
using Lumina.Contracts.Requests.Authorization;
using Lumina.Presentation.Api.Core.Endpoints.Admin.Authorization.Roles.DeleteRole;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Admin.Authorization.Roles.DeleteRole;

/// <summary>
/// Contains unit tests for the <see cref="DeleteRoleEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteRoleEndpointTests
{
    private readonly ISender _mockSender;
    private readonly DeleteRoleEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteRoleEndpointTests"/> class.
    /// </summary>
    public DeleteRoleEndpointTests()
    {
        _mockSender = Substitute.For<ISender>();
        _sut = Factory.Create<DeleteRoleEndpoint>(_mockSender);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnNoContent()
    {
        // Arrange
        DeleteRoleRequest request = new(Guid.NewGuid());
        CancellationToken cancellationToken = CancellationToken.None;
        _mockSender.Send(Arg.Any<DeleteRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(Result.Deleted));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediatorReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        DeleteRoleRequest request = new(Guid.NewGuid());
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Failure("Role.Deletion.Failed", "Failed to delete role.");
        _mockSender.Send(Arg.Any<DeleteRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        problemResult.ContentType.Should().Be("application/problem+json");
        problemResult.ProblemDetails.Should().BeOfType<Microsoft.AspNetCore.Mvc.ProblemDetails>();

        problemResult.ProblemDetails.Title.Should().Be("Role.Deletion.Failed");
        problemResult.ProblemDetails.Detail.Should().Be("Failed to delete role.");
        problemResult.ProblemDetails.Status.Should().Be(StatusCodes.Status403Forbidden);
        problemResult.ProblemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.4");
        problemResult.ProblemDetails.Extensions["traceId"].Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendDeleteRoleCommandToSender()
    {
        // Arrange
        Guid roleId = Guid.NewGuid();
        DeleteRoleRequest request = new(roleId);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockSender.Send(Arg.Any<DeleteRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(Result.Deleted));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockSender.Received(1).Send(
            Arg.Is<DeleteRoleCommand>(cmd => cmd.RoleId == request.RoleId),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockSender.Send(Arg.Any<DeleteRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(info => new ValueTask<ErrorOr<Deleted>>(
                Task.Run(async () =>
                {
                    operationStarted.SetResult(true);
                    await cancellationRequested.Task;
                    info.Arg<CancellationToken>().ThrowIfCancellationRequested();
                    return ErrorOrFactory.From(Result.Deleted);
                }, info.Arg<CancellationToken>())
            ));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(new DeleteRoleRequest(Guid.NewGuid()), cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
