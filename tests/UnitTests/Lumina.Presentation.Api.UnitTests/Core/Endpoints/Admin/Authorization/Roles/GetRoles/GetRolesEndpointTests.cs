#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using FluentAssertions;
using Lumina.Application.Core.Admin.Authorization.Roles.Queries.GetRoles;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Presentation.Api.Core.Endpoints.Admin.Authorization.Roles.GetRoles;
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

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Admin.Authorization.Roles.GetRoles;

/// <summary>
/// Contains unit tests for the <see cref="GetRolesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRolesEndpointTests
{
    private readonly ISender _mockSender;
    private readonly GetRolesEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRolesEndpointTests"/> class.
    /// </summary>
    public GetRolesEndpointTests()
    {
        _mockSender = Substitute.For<ISender>();
        _sut = Factory.Create<GetRolesEndpoint>(_mockSender);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithRoles()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        IEnumerable<RoleResponse> expectedResponse =
        [
        new RoleResponse(Guid.NewGuid(), "Admin")
    ];
        _mockSender.Send(Arg.Any<GetRolesQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(new EmptyRequest(), cancellationToken);

        // Assert
        Ok<IEnumerable<RoleResponse>> okResult = result.Should().BeOfType<Ok<IEnumerable<RoleResponse>>>().Subject;
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediatorReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Failure("Roles.NotFound", "No roles found.");
        _mockSender.Send(Arg.Any<GetRolesQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(new EmptyRequest(), cancellationToken);

        // Assert
        ProblemHttpResult problemResult = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        problemResult.ContentType.Should().Be("application/problem+json");
        problemResult.ProblemDetails.Should().BeOfType<Microsoft.AspNetCore.Mvc.ProblemDetails>();

        problemResult.ProblemDetails.Title.Should().Be("Roles.NotFound");
        problemResult.ProblemDetails.Detail.Should().Be("No roles found.");
        problemResult.ProblemDetails.Status.Should().Be(StatusCodes.Status403Forbidden);
        problemResult.ProblemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.4");
        problemResult.ProblemDetails.Extensions["traceId"].Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetRolesQueryToSender()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockSender.Send(Arg.Any<GetRolesQuery>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<ErrorOr<IEnumerable<RoleResponse>>>(
                ErrorOrFactory.From(new List<RoleResponse>
                {
                    new(Guid.NewGuid(), "Admin")
                } as IEnumerable<RoleResponse>)
            ));

        // Act
        await _sut.ExecuteAsync(new EmptyRequest(), cancellationToken);

        // Assert
        await _mockSender.Received(1).Send(
            Arg.Any<GetRolesQuery>(),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockSender.Send(Arg.Any<GetRolesQuery>(), Arg.Any<CancellationToken>())
            .Returns(info => new ValueTask<ErrorOr<IEnumerable<RoleResponse>>>(
                Task.Run(async () =>
                {
                    operationStarted.SetResult(true);
                    await cancellationRequested.Task;
                    info.Arg<CancellationToken>().ThrowIfCancellationRequested();
                    return ErrorOrFactory.From(new List<RoleResponse>() as IEnumerable<RoleResponse>);
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
