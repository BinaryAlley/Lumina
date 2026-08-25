#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.UsersManagement.Authentication.Commands.ChangePassword;
using Lumina.Contracts.Fixtures.Core.Requests.Authentication;
using Lumina.Contracts.Fixtures.Core.Responses.Authentication;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Responses.Authentication;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authentication.ChangePassword;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.UsersManagement.Authentication.ChangePassword;

/// <summary>
/// Contains unit tests for the <see cref="ChangePasswordEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ChangePasswordEndpointTests
{
    private readonly ICommandHandler<ChangePasswordCommand, Result<ChangePasswordResponse>> _mockHandler;
    private readonly ChangePasswordEndpoint _sut;
    private readonly ChangePasswordRequestFixture _changePasswordRequestFixture = new();
    private readonly ChangePasswordResponseFixture _changePasswordResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangePasswordEndpointTests"/> class.
    /// </summary>
    public ChangePasswordEndpointTests()
    {
        _mockHandler = Substitute.For<ICommandHandler<ChangePasswordCommand, Result<ChangePasswordResponse>>>();
        _sut = FastEndpoints.Factory.Create<ChangePasswordEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithChangePasswordResponse()
    {
        // Arrange
        ChangePasswordRequest request = _changePasswordRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        ChangePasswordResponse expectedResponse = _changePasswordResponseFixture.Create(isPasswordChanged: true);
        _mockHandler.HandleAsync(Arg.Any<ChangePasswordCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<ChangePasswordResponse> okResult = Assert.IsType<Ok<ChangePasswordResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        ChangePasswordRequest request = _changePasswordRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Validation("Password.Invalid", "The current password is incorrect.");
        _mockHandler.HandleAsync(Arg.Any<ChangePasswordCommand>(), Arg.Any<CancellationToken>())
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
        Assert.Equal(new[] { "The current password is incorrect." }, validationProblemDetails.Errors["Password.Invalid"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendChangePasswordCommandToSender()
    {
        // Arrange
        ChangePasswordRequest request = _changePasswordRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<ChangePasswordCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_changePasswordResponseFixture.Create(isPasswordChanged: true)));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<ChangePasswordCommand>(cmd =>
                cmd.Username == request.Username &&
                cmd.CurrentPassword == request.CurrentPassword &&
                cmd.NewPassword == request.NewPassword &&
                cmd.NewPasswordConfirm == request.NewPasswordConfirm),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        ChangePasswordRequest request = _changePasswordRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<ChangePasswordCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_changePasswordResponseFixture.Create(isPasswordChanged: true));
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
