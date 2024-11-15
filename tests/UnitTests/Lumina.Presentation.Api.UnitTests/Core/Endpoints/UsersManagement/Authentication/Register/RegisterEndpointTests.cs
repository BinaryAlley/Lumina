#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using FluentAssertions;
using Lumina.Application.Core.UsersManagement.Authentication.Commands.RegisterUser;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Responses.Authentication;
using Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authentication.Register;
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

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.UsersManagement.Authentication.Register;

/// <summary>
/// Contains unit tests for the <see cref="RegisterEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RegisterEndpointTests
{
    private readonly ISender _mockSender;
    private readonly RegisterEndpoint _sut;
    private readonly RegistrationRequestFixture _registrationRequestFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterEndpointTests"/> class.
    /// </summary>
    public RegisterEndpointTests()
    {
        _mockSender = Substitute.For<ISender>();
        _sut = Factory.Create<RegisterEndpoint>(_mockSender);
        _registrationRequestFixture = new RegistrationRequestFixture();
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnCreatedResultWithRegistrationResponse()
    {
        // Arrange
        RegistrationRequest request = _registrationRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        RegistrationResponse expectedResponse = new(Guid.NewGuid(), "testUser", "TOTP123");
        _mockSender.Send(Arg.Any<RegisterUserCommand>(), Arg.Any<CancellationToken>())
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
        Error expectedError = Error.Validation("Registration.Failed", "Username is already taken.");
        _mockSender.Send(Arg.Any<RegisterUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut. ExecuteAsync(request, cancellationToken);

        // Assert
        result.Should().BeOfType<ProblemHttpResult>();
        ProblemHttpResult problemDetails = (ProblemHttpResult)result;
        problemDetails.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        problemDetails.ContentType.Should().Be("application/problem+json");
        problemDetails.ProblemDetails.Should().BeOfType<HttpValidationProblemDetails>();
        HttpValidationProblemDetails validationProblemDetails = (HttpValidationProblemDetails)problemDetails.ProblemDetails;
        validationProblemDetails.Status.Should().Be(StatusCodes.Status422UnprocessableEntity);
        validationProblemDetails.Title.Should().Be("General.Validation");
        validationProblemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc4918#section-11.2");
        validationProblemDetails.Errors.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new
            {
                Key = "Registration.Failed",
                Value = new[] { "Username is already taken." }
            });
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendRegisterUserCommandToSender()
    {
        // Arrange
        RegistrationRequest request = _registrationRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockSender.Send(Arg.Any<RegisterUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(new RegistrationResponse(Guid.NewGuid(), "testUser", "TOTP123")));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockSender.Received(1).Send(
            Arg.Is<RegisterUserCommand>(cmd =>
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

        _mockSender.Send(Arg.Any<RegisterUserCommand>(), Arg.Any<CancellationToken>())
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
