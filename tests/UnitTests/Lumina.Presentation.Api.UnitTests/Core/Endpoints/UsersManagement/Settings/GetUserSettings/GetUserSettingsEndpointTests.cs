#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.UsersManagement.Settings.Queries.GetUserSettings;
using Lumina.Contracts.Responses.UsersManagement.Settings;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Settings.GetUserSettings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.UsersManagement.Settings.GetUserSettings;

/// <summary>
/// Contains unit tests for the <see cref="GetUserSettingsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetUserSettingsEndpointTests
{
    private readonly IQueryHandler<GetUserSettingsQuery, Result<UserSettingsResponse>> _mockHandler;
    private readonly GetUserSettingsEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserSettingsEndpointTests"/> class.
    /// </summary>
    public GetUserSettingsEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetUserSettingsQuery, Result<UserSettingsResponse>>>();
        _sut = Factory.Create<GetUserSettingsEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithUserSettingsResponse()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        UserSettingsResponse expectedResponse = new(Guid.NewGuid(), true, 48, false);
        _mockHandler.HandleAsync(Arg.Any<GetUserSettingsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(new EmptyRequest(), cancellationToken);

        // Assert
        Ok<UserSettingsResponse> okResult = Assert.IsType<Ok<UserSettingsResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Failure("General.Failure", "An error has occurred.");
        _mockHandler.HandleAsync(Arg.Any<GetUserSettingsQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(new EmptyRequest(), cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemResult.ProblemDetails);
        Assert.Equal("General.Failure", problemResult.ProblemDetails.Title);
        Assert.Equal("An error has occurred.", problemResult.ProblemDetails.Detail);
        Assert.NotNull(problemResult.ProblemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetUserSettingsQueryToHandler()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetUserSettingsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(new UserSettingsResponse(Guid.NewGuid(), true, 48, false)));

        // Act
        await _sut.ExecuteAsync(new EmptyRequest(), cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Any<GetUserSettingsQuery>(),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetUserSettingsQuery>(), Arg.Any<CancellationToken>())
            .Returns(info => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                info.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(new UserSettingsResponse(Guid.NewGuid(), true, 48, false));
            }, info.Arg<CancellationToken>()));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(new EmptyRequest(), cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
