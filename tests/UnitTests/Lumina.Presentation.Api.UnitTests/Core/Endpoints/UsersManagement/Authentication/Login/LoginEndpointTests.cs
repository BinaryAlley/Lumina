#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using FluentAssertions;
using Lumina.Application.Core.UsersManagement.Authentication.Queries.LoginUser;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Responses.Authentication;
using Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authentication.Login;
using Lumina.Presentation.Api.UnitTests.Core.Endpoints.UsersManagement.Authentication.Login.Fixtures;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.UsersManagement.Authentication.Login;

/// <summary>
/// Contains unit tests for the <see cref="LoginEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LoginEndpointTests
{
    private readonly ISender _mockSender;
    private readonly LoginEndpoint _sut;
    private readonly LoginRequestFixture _loginRequestFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginEndpointTests"/> class.
    /// </summary>
    public LoginEndpointTests()
    {
        _mockSender = Substitute.For<ISender>();
        _sut = Factory.Create<LoginEndpoint>(_mockSender);
        _loginRequestFixture = new LoginRequestFixture();
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithLoginResponse()
    {
        // Arrange
        LoginRequest request = _loginRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        LoginResponse expectedResponse = new(Guid.NewGuid(), "testUser", "jwt_token", true);
        _mockSender.Send(Arg.Any<LoginUserQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<LoginResponse> okResult = result.Should().BeOfType<Ok<LoginResponse>>().Subject;
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediatorReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        LoginRequest request = _loginRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Validation("Login.Failed", "Invalid username or password.");
        _mockSender.Send(Arg.Any<LoginUserQuery>(), Arg.Any<CancellationToken>())
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
                Key = "Login.Failed",
                Value = new[] { "Invalid username or password." }
            });
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendLoginUserQueryToSender()
    {
        // Arrange
        LoginRequest request = _loginRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockSender.Send(Arg.Any<LoginUserQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(new LoginResponse(Guid.NewGuid(), "testUser", "jwt_token", true)));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockSender.Received(1).Send(
            Arg.Is<LoginUserQuery>(q =>
                q.Username == request.Username &&
                q.Password == request.Password &&
                q.TotpCode == request.TotpCode),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        LoginRequest request = _loginRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockSender.Send(Arg.Any<LoginUserQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => new ValueTask<ErrorOr<LoginResponse>>(Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return ErrorOrFactory.From(new LoginResponse(Guid.NewGuid(), "testUser", "jwt_token", true));
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
