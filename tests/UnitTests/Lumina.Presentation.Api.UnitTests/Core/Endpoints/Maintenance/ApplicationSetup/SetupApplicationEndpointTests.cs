#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Maintenance.ApplicationSetup.Commands.SetupApplication;
using Lumina.Contracts.Fixtures.Core.Requests.Authentication;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Responses.Authentication;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Maintenance.ApplicationSetup;
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
    private readonly ICommandHandler<SetupApplicationCommand, Result<RegistrationResponse>> _mockHandler;
    private readonly SetupApplicationEndpoint _sut;
    private readonly RegistrationRequestFixture _registrationRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SetupApplicationEndpointTests"/> class.
    /// </summary>
    public SetupApplicationEndpointTests()
    {
        _mockHandler = Substitute.For<ICommandHandler<SetupApplicationCommand, Result<RegistrationResponse>>>();
        _sut = FastEndpoints.Factory.Create<SetupApplicationEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnCreatedResultWithRegistrationResponse()
    {
        // Arrange
        RegistrationRequest request = _registrationRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        RegistrationResponse expectedResponse = new(Guid.NewGuid(), "testUser", "TOTP123");
        _mockHandler.HandleAsync(Arg.Any<SetupApplicationCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Created<RegistrationResponse> createdResult = Assert.IsType<Created<RegistrationResponse>>(result);
        Assert.Equal(expectedResponse, createdResult.Value);
        Assert.EndsWith(expectedResponse.Id.ToString(), createdResult.Location);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        RegistrationRequest request = _registrationRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Conflict("Registration.Failed", "The application is already initialized.");
        _mockHandler.HandleAsync(Arg.Any<SetupApplicationCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status409Conflict, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemDetails.ProblemDetails);

        Assert.Equal("Registration.Failed", problemDetails.ProblemDetails.Title);
        Assert.Equal("The application is already initialized.", problemDetails.ProblemDetails.Detail);
        Assert.Equal(StatusCodes.Status409Conflict, problemDetails.ProblemDetails.Status);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.10", problemDetails.ProblemDetails.Type);
        Assert.NotNull(problemDetails.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendSetupApplicationCommandToSender()
    {
        // Arrange
        RegistrationRequest request = _registrationRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<SetupApplicationCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(new RegistrationResponse(Guid.NewGuid(), "testUser", "TOTP123")));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
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

        _mockHandler.HandleAsync(Arg.Any<SetupApplicationCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(new RegistrationResponse(Guid.NewGuid(), "testUser", "TOTP123"));
            }, callInfo.Arg<CancellationToken>()));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(request, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
