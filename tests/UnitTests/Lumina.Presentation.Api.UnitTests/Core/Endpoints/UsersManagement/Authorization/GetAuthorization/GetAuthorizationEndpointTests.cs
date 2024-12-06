#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using FluentAssertions;
using Lumina.Application.Core.UsersManagement.Authentication.Queries.LoginUser;
using Lumina.Application.Core.UsersManagement.Authorization.Queries.GetAuthorization;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Requests.Authorization;
using Lumina.Contracts.Responses.Authentication;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Domain.Common.Enums.Authorization;
using Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authentication.Login;
using Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authorization.GetAuthorization;
using Lumina.Presentation.Api.UnitTests.Core.Endpoints.UsersManagement.Authentication.Login.Fixtures;
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

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.UsersManagement.Authorization.GetAuthorization;

/// <summary>
/// Contains unit tests for the <see cref="GetAuthorizationEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAuthorizationEndpointTests
{
    private readonly ISender _mockSender;
    private readonly GetAuthorizationEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAuthorizationEndpointTests"/> class.
    /// </summary>
    public GetAuthorizationEndpointTests()
    {
        _mockSender = Substitute.For<ISender>();
        _sut = Factory.Create<GetAuthorizationEndpoint>(_mockSender);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithAuthorizationResponse()
    {
        // Arrange
        GetAuthorizationRequest request = GetAuthorizationRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        GetAuthorizationResponse expectedResponse = new(
            request.UserId!.Value,
            new HashSet<AuthorizationRole> { AuthorizationRole.Admin },
            new HashSet<AuthorizationPermission> { AuthorizationPermission.CanViewUsers });

        _mockSender.Send(Arg.Any<GetAuthorizationQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        GetAuthorizationResponse actualResponse = result.Should().BeOfType<Ok<GetAuthorizationResponse>>().Subject.Value!;
        actualResponse.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediatorReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        GetAuthorizationRequest request = GetAuthorizationRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Unauthorized("Authorization.Failed", "User is not authorized.");

        _mockSender.Send(Arg.Any<GetAuthorizationQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problemDetails.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        problemDetails.ContentType.Should().Be("application/problem+json");
        problemDetails.ProblemDetails.Should().BeOfType<Microsoft.AspNetCore.Mvc.ProblemDetails>();

        problemDetails.ProblemDetails.Title.Should().Be("Authorization.Failed");
        problemDetails.ProblemDetails.Detail.Should().Be("User is not authorized.");
        problemDetails.ProblemDetails.Status.Should().Be(StatusCodes.Status403Forbidden);
        problemDetails.ProblemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.4");
        problemDetails.ProblemDetails.Extensions["traceId"].Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetAuthorizationQueryToSender()
    {
        // Arrange
        GetAuthorizationRequest request = GetAuthorizationRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;

        _mockSender.Send(Arg.Any<GetAuthorizationQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(new GetAuthorizationResponse(
                request.UserId!.Value,
                new HashSet<AuthorizationRole>().ToHashSet(),
                new HashSet<AuthorizationPermission>().ToHashSet()
            )));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockSender.Received(1).Send(
            Arg.Is<GetAuthorizationQuery>(query =>
                query.UserId == request.UserId),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetAuthorizationRequest request = GetAuthorizationRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockSender.Send(Arg.Any<GetAuthorizationQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => new ValueTask<ErrorOr<GetAuthorizationResponse>>(Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return ErrorOrFactory.From(new GetAuthorizationResponse(
                    request.UserId!.Value,
                    new HashSet<AuthorizationRole>().ToHashSet(),
                    new HashSet<AuthorizationPermission>().ToHashSet()
                ));
            }, callInfo.Arg<CancellationToken>())));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(request, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
