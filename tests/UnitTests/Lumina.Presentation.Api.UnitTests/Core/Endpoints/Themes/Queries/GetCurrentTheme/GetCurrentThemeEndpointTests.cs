#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Themes.Management.Queries.GetCurrentTheme;
using Lumina.Contracts.Fixtures.Core.Responses.Themes;
using Lumina.Contracts.Responses.Themes;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Themes.Queries.GetCurrentTheme;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Themes.Queries.GetCurrentTheme;

/// <summary>
/// Contains unit tests for the <see cref="GetCurrentThemeEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetCurrentThemeEndpointTests
{
    private readonly IQueryHandler<GetCurrentThemeQuery, Result<ThemeResponse>> _mockHandler;
    private readonly GetCurrentThemeEndpoint _sut;
    private readonly ThemeResponseFixture _themeResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCurrentThemeEndpointTests"/> class.
    /// </summary>
    public GetCurrentThemeEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetCurrentThemeQuery, Result<ThemeResponse>>>();
        _sut = Factory.Create<GetCurrentThemeEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithCurrentTheme()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        ThemeResponse expectedResponse = _themeResponseFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<GetCurrentThemeQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        Ok<ThemeResponse> okResult = Assert.IsType<Ok<ThemeResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotFoundError_ShouldReturnProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = DomainErrors.Themes.ThemeNotFound;
        _mockHandler.HandleAsync(Arg.Any<GetCurrentThemeQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        Microsoft.AspNetCore.Mvc.ProblemDetails problemDetailsBody = Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemDetails.ProblemDetails);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetailsBody.Status);
        Assert.Equal("General.NotFound", problemDetailsBody.Title);
        Assert.Equal("ThemeNotFound", problemDetailsBody.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problemDetailsBody.Type);
        Assert.NotNull(problemDetailsBody.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetCurrentThemeQueryToHandler()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetCurrentThemeQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themeResponseFixture.Create()));

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetCurrentThemeQuery>(query => query != null),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetCurrentThemeQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_themeResponseFixture.Create());
            }, callInfo.Arg<CancellationToken>()));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(EmptyRequest.Instance, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }
}
