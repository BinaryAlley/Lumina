#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Admin.Authorization.Roles.Commands.DeleteRole;
using Lumina.Contracts.Requests.Authorization;
using Lumina.Presentation.Api.Core.Endpoints.Admin.Authorization.Roles.DeleteRole;
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
    private readonly ICommandHandler<DeleteRoleCommand, ErrorOr<Deleted>> _mockHandler;
    private readonly DeleteRoleEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteRoleEndpointTests"/> class.
    /// </summary>
    public DeleteRoleEndpointTests()
    {
        _mockHandler = Substitute.For<ICommandHandler<DeleteRoleCommand, ErrorOr<Deleted>>>();
        _sut = FastEndpoints.Factory.Create<DeleteRoleEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnNoContent()
    {
        // Arrange
        DeleteRoleRequest request = new(Guid.NewGuid());
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<DeleteRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(Result.Deleted));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Assert.IsType<NoContent>(result);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        DeleteRoleRequest request = new(Guid.NewGuid());
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Failure("Role.Deletion.Failed", "Failed to delete role.");
        _mockHandler.HandleAsync(Arg.Any<DeleteRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemResult.ProblemDetails);

        Assert.Equal("Role.Deletion.Failed", problemResult.ProblemDetails.Title);
        Assert.Equal("Failed to delete role.", problemResult.ProblemDetails.Detail);
        Assert.Equal(StatusCodes.Status403Forbidden, problemResult.ProblemDetails.Status);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemResult.ProblemDetails.Type);
        Assert.NotNull(problemResult.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendDeleteRoleCommandToSender()
    {
        // Arrange
        Guid roleId = Guid.NewGuid();
        DeleteRoleRequest request = new(roleId);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<DeleteRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(Result.Deleted));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
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

        _mockHandler.HandleAsync(Arg.Any<DeleteRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(info => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                info.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return ErrorOrFactory.From(Result.Deleted);
            }, info.Arg<CancellationToken>()));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(new DeleteRoleRequest(Guid.NewGuid()), cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
