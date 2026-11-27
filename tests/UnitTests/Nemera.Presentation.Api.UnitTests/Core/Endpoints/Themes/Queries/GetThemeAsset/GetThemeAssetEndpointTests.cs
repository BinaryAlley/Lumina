#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Application.Common.CQRS;
using Nemera.Application.Core.Themes.Management.Queries.GetThemeAsset;
using Nemera.Contracts.Fixtures.Core.Requests.Themes;
using Nemera.Contracts.Fixtures.Core.Responses.Themes;
using Nemera.Contracts.Requests.Themes;
using Nemera.Contracts.Responses.Themes;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Core.Endpoints.Themes.Queries.GetThemeAsset;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DomainErrors = Nemera.Domain.Common.Errors.Errors;
#endregion

namespace Nemera.Presentation.Api.UnitTests.Core.Endpoints.Themes.Queries.GetThemeAsset;

/// <summary>
/// Contains unit tests for the <see cref="GetThemeAssetEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemeAssetEndpointTests
{
    private readonly IQueryHandler<GetThemeAssetQuery, Result<ThemeAssetResponse>> _mockHandler;
    private readonly GetThemeAssetEndpoint _sut;
    private readonly GetThemeAssetRequestFixture _getThemeAssetRequestFixture = new();
    private readonly ThemeAssetResponseFixture _themeAssetResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemeAssetEndpointTests"/> class.
    /// </summary>
    public GetThemeAssetEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetThemeAssetQuery, Result<ThemeAssetResponse>>>();
        _sut = Factory.Create<GetThemeAssetEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnFileResultWithThemeAsset()
    {
        // Arrange
        GetThemeAssetRequest request = _getThemeAssetRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        ThemeAssetResponse expectedResponse = _themeAssetResponseFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<GetThemeAssetQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        FileContentHttpResult fileResult = Assert.IsType<FileContentHttpResult>(result);
        Assert.Equal(expectedResponse.Bytes, fileResult.FileContents.ToArray());
        Assert.Equal(expectedResponse.ContentType, fileResult.ContentType);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotFoundError_ShouldReturnProblemResult()
    {
        // Arrange
        GetThemeAssetRequest request = _getThemeAssetRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = DomainErrors.Themes.ThemeNotFound;
        _mockHandler.HandleAsync(Arg.Any<GetThemeAssetQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

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
    public async Task ExecuteAsync_WhenHandlerReturnsValidationErrors_ShouldReturnValidationProblemResult()
    {
        // Arrange
        GetThemeAssetRequest request = _getThemeAssetRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = DomainErrors.Themes.ThemeAssetPathCannotBeEmpty;
        _mockHandler.HandleAsync(Arg.Any<GetThemeAssetQuery>(), Arg.Any<CancellationToken>())
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
        Assert.Equal("OneOrMoreValidationErrorsOccurred", validationProblemDetails.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", validationProblemDetails.Type);
        Assert.Single(validationProblemDetails.Errors);
        Assert.Equal(new[] { "ThemeAssetPathCannotBeEmpty" }, validationProblemDetails.Errors["General.Validation"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetThemeAssetQueryToSender()
    {
        // Arrange
        GetThemeAssetRequest request = _getThemeAssetRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetThemeAssetQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themeAssetResponseFixture.Create()));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetThemeAssetQuery>(query =>
                query.ThemeId == request.ThemeId &&
                query.AssetPath == request.AssetPath),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetThemeAssetRequest request = _getThemeAssetRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetThemeAssetQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_themeAssetResponseFixture.Create());
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
