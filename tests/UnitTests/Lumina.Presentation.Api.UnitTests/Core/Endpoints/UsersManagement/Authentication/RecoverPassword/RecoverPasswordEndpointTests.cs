#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using FluentAssertions;
using Lumina.Application.Core.UsersManagement.Authentication.Commands.RecoverPassword;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Responses.Authentication;
using Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authentication.RecoverPassword;
using Lumina.Presentation.Api.UnitTests.Core.Endpoints.UsersManagement.Authentication.RecoverPassword.Fixtures;
using Mediator;
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
    private readonly ISender _mockSender;
    private readonly RecoverPasswordEndpoint _sut;
    private readonly RecoverPasswordRequestFixture _recoverPasswordRequestFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecoverPasswordEndpointTests"/> class.
    /// </summary>
    public RecoverPasswordEndpointTests()
    {
        _mockSender = Substitute.For<ISender>();
        _sut = Factory.Create<RecoverPasswordEndpoint>(_mockSender);
        _recoverPasswordRequestFixture = new RecoverPasswordRequestFixture();
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithRecoverPasswordResponse()
    {
        // Arrange
        RecoverPasswordRequest request = _recoverPasswordRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        RecoverPasswordResponse expectedResponse = new(IsPasswordReset: true);
        _mockSender.Send(Arg.Any<RecoverPasswordCommand>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<RecoverPasswordResponse> okResult = result.Should().BeOfType<Ok<RecoverPasswordResponse>>().Subject;
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediatorReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        RecoverPasswordRequest request = _recoverPasswordRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Validation("Recovery.Failed", "Invalid username or TOTP code.");
        _mockSender.Send(Arg.Any<RecoverPasswordCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

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
                Key = "Recovery.Failed",
                Value = new[] { "Invalid username or TOTP code." }
            });
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendRecoverPasswordCommandToSender()
    {
        // Arrange
        RecoverPasswordRequest request = _recoverPasswordRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockSender.Send(Arg.Any<RecoverPasswordCommand>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(new RecoverPasswordResponse(true)));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockSender.Received(1).Send(
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

        _mockSender.Send(Arg.Any<RecoverPasswordCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => new ValueTask<ErrorOr<RecoverPasswordResponse>>(Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return ErrorOrFactory.From(new RecoverPasswordResponse(true));
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
