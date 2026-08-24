#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.UsersManagement.Authentication.Commands.RecoverPassword;
using Lumina.Contracts.Fixtures.Core.Requests.Authentication;
using Lumina.Contracts.Fixtures.Core.Responses.Authentication;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Responses.Authentication;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authentication.RecoverPassword;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.UsersManagement.Authentication.RecoverPassword;

/// <summary>
/// Contains unit tests for the <see cref="RecoverPasswordEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RecoverPasswordEndpointTests
{
    private readonly ICommandHandler<RecoverPasswordCommand, Result<RecoverPasswordResponse>> _mockHandler;
    private readonly RecoverPasswordEndpoint _sut;
    private readonly RecoverPasswordRequestFixture _recoverPasswordRequestFixture = new();
    private readonly RecoverPasswordResponseFixture _recoverPasswordResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RecoverPasswordEndpointTests"/> class.
    /// </summary>
    public RecoverPasswordEndpointTests()
    {
        _mockHandler = Substitute.For<ICommandHandler<RecoverPasswordCommand, Result<RecoverPasswordResponse>>>();
        _sut = FastEndpoints.Factory.Create<RecoverPasswordEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithRecoverPasswordResponse()
    {
        // Arrange
        RecoverPasswordRequest request = _recoverPasswordRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        RecoverPasswordResponse expectedResponse = _recoverPasswordResponseFixture.Create(isPasswordReset: true);
        _mockHandler.HandleAsync(Arg.Any<RecoverPasswordCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<RecoverPasswordResponse> okResult = Assert.IsType<Ok<RecoverPasswordResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        RecoverPasswordRequest request = _recoverPasswordRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Validation("Recovery.Failed", "Invalid username or TOTP code.");
        _mockHandler.HandleAsync(Arg.Any<RecoverPasswordCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        HttpValidationProblemDetails validationProblemDetails = Assert.IsType<HttpValidationProblemDetails>(problemDetails.ProblemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, validationProblemDetails.Status);
        Assert.Equal("General.Validation", validationProblemDetails.Title);
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", validationProblemDetails.Type);
        Assert.Single(validationProblemDetails.Errors);
        Assert.Equal(new[] { "Invalid username or TOTP code." }, validationProblemDetails.Errors["Recovery.Failed"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendRecoverPasswordCommandToSender()
    {
        // Arrange
        RecoverPasswordRequest request = _recoverPasswordRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<RecoverPasswordCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_recoverPasswordResponseFixture.Create(isPasswordReset: true)));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<RecoverPasswordCommand>(cmd =>
                cmd.Username == request.Username &&
                cmd.TotpCode == request.TotpCode),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        RecoverPasswordRequest request = _recoverPasswordRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<RecoverPasswordCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_recoverPasswordResponseFixture.Create(isPasswordReset: true));
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
