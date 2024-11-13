#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using FluentAssertions;
using Lumina.Application.Core.Maintenance.ApplicationSetup.Commands.SetupApplication;
using Lumina.Application.Core.Maintenance.ApplicationSetup.Queries.CheckInitialization;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Responses.Authentication;
using Lumina.Contracts.Responses.UsersManagement;
using Lumina.Presentation.Api.Core.Endpoints.Maintenance.ApplicationSetup;
using Lumina.Presentation.Api.UnitTests.Core.Endpoints.Maintenance.ApplicationSetup.Fixtures;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Maintenance.ApplicationSetup;

/// <summary>
/// Contains unit tests for the <see cref="SetupApplicationEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetupApplicationEndpointTests
{
    private readonly ISender _mockSender;
    private readonly SetupApplicationEndpoint _sut;
    private readonly RegistrationRequestFixture _registrationRequestFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetupApplicationEndpointTests"/> class.
    /// </summary>
    public SetupApplicationEndpointTests()
    {
        _mockSender = Substitute.For<ISender>();
        _sut = Factory.Create<SetupApplicationEndpoint>(_mockSender);
        _registrationRequestFixture = new RegistrationRequestFixture();
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnCreatedResultWithRegistrationResponse()
    {
        // Arrange
        RegistrationRequest request = _registrationRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        RegistrationResponse expectedResponse = new(Guid.NewGuid(), "testUser", "TOTP123");
        _mockSender.Send(Arg.Any<SetupApplicationCommand>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Created<RegistrationResponse> createdResult = result.Should().BeOfType<Created<RegistrationResponse>>().Subject;
        createdResult.Value.Should().BeEquivalentTo(expectedResponse);
        createdResult.Location.Should().EndWith(expectedResponse.Id.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediatorReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        RegistrationRequest request = _registrationRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Conflict("Registration.Failed", "The application is already initialized.");
        _mockSender.Send(Arg.Any<SetupApplicationCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        result.Should().BeOfType<ProblemHttpResult>();
        ProblemHttpResult problemDetails = (ProblemHttpResult)result;
        problemDetails.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        problemDetails.ContentType.Should().Be("application/problem+json");
        problemDetails.ProblemDetails.Should().BeOfType<Microsoft.AspNetCore.Mvc.ProblemDetails>();

        problemDetails.ProblemDetails.Title.Should().Be("Registration.Failed");
        problemDetails.ProblemDetails.Detail.Should().Be("The application is already initialized.");
        problemDetails.ProblemDetails.Status.Should().Be(StatusCodes.Status409Conflict);
        problemDetails.ProblemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.10");
        problemDetails.ProblemDetails.Extensions["traceId"].Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendSetupApplicationCommandToSender()
    {
        // Arrange
        RegistrationRequest request = _registrationRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockSender.Send(Arg.Any<SetupApplicationCommand>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(new RegistrationResponse(Guid.NewGuid(), "testUser", "TOTP123")));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockSender.Received(1).Send(
            Arg.Is<SetupApplicationCommand>(cmd =>
                cmd.Username == request.Username &&
                cmd.Password == request.Password &&
                cmd.PasswordConfirm == request.PasswordConfirm &&
                cmd.Use2fa == request.Use2fa),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        RegistrationRequest request = _registrationRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockSender.Send(Arg.Any<SetupApplicationCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => new ValueTask<ErrorOr<RegistrationResponse>>(Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return ErrorOrFactory.From(new RegistrationResponse(Guid.NewGuid(), "testUser", "TOTP123"));
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
