#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using FluentAssertions;
using Lumina.Application.Core.UsersManagement.Authentication.Commands.ChangePassword;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Responses.Authentication;
using Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authentication.ChangePassword;
using Lumina.Presentation.Api.UnitTests.Core.Endpoints.UsersManagement.Authentication.ChangePassword.Fixtures;
using Mediator;
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
    private readonly ISender _mockSender;
    private readonly ChangePasswordEndpoint _sut;
    private readonly ChangePasswordRequestFixture _changePasswordRequestFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangePasswordEndpointTests"/> class.
    /// </summary>
    public ChangePasswordEndpointTests()
    {
        _mockSender = Substitute.For<ISender>();
        _sut = Factory.Create<ChangePasswordEndpoint>(_mockSender);
        _changePasswordRequestFixture = new ChangePasswordRequestFixture();
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithChangePasswordResponse()
    {
        // Arrange
        ChangePasswordRequest request = _changePasswordRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        ChangePasswordResponse expectedResponse = new(IsPasswordChanged: true);
        _mockSender.Send(Arg.Any<ChangePasswordCommand>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<ChangePasswordResponse> okResult = result.Should().BeOfType<Ok<ChangePasswordResponse>>().Subject;
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediatorReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        ChangePasswordRequest request = _changePasswordRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Validation("Password.Invalid", "The current password is incorrect.");
        _mockSender.Send(Arg.Any<ChangePasswordCommand>(), Arg.Any<CancellationToken>())
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
                Key = "Password.Invalid",
                Value = new[] { "The current password is incorrect." }
            });
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendChangePasswordCommandToSender()
    {
        // Arrange
        ChangePasswordRequest request = _changePasswordRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockSender.Send(Arg.Any<ChangePasswordCommand>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(new ChangePasswordResponse(true)));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockSender.Received(1).Send(
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

        _mockSender.Send(Arg.Any<ChangePasswordCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => new ValueTask<ErrorOr<ChangePasswordResponse>>(Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return ErrorOrFactory.From(new ChangePasswordResponse(true));
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
