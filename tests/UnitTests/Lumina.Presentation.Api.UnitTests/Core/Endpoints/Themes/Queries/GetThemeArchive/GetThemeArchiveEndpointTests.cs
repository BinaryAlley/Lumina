#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Themes.Management.Queries.GetThemeArchive;
using Lumina.Contracts.Fixtures.Core.Requests.Themes;
using Lumina.Contracts.Fixtures.Core.Responses.Themes;
using Lumina.Contracts.Requests.Themes;
using Lumina.Contracts.Responses.Themes;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Themes.Queries.GetThemeArchive;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Themes.Queries.GetThemeArchive;

/// <summary>
/// Contains unit tests for the <see cref="GetThemeArchiveEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemeArchiveEndpointTests
{
    private readonly IQueryHandler<GetThemeArchiveQuery, Result<ThemeArchiveResponse>> _mockHandler;
    private readonly GetThemeArchiveEndpoint _sut;
    private readonly GetThemeArchiveRequestFixture _getThemeArchiveRequestFixture = new();
    private readonly ThemeArchiveResponseFixture _themeArchiveResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemeArchiveEndpointTests"/> class.
    /// </summary>
    public GetThemeArchiveEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetThemeArchiveQuery, Result<ThemeArchiveResponse>>>();
        _sut = Factory.Create<GetThemeArchiveEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnFileResultWithThemeArchive()
    {
        // Arrange
        GetThemeArchiveRequest request = _getThemeArchiveRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        ThemeArchiveResponse expectedResponse = _themeArchiveResponseFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<GetThemeArchiveQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        FileContentHttpResult fileResult = Assert.IsType<FileContentHttpResult>(result);
        Assert.Equal(expectedResponse.Bytes, fileResult.FileContents.ToArray());
        Assert.Equal(expectedResponse.ContentType, fileResult.ContentType);
        Assert.Equal(expectedResponse.FileName, fileResult.FileDownloadName);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotFoundError_ShouldReturnProblemResult()
    {
        // Arrange
        GetThemeArchiveRequest request = _getThemeArchiveRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = DomainErrors.Themes.ThemeNotFound;
        _mockHandler.HandleAsync(Arg.Any<GetThemeArchiveQuery>(), Arg.Any<CancellationToken>())
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
    public async Task ExecuteAsync_WhenHandlerReturnsValidationError_ShouldReturnValidationProblemResult()
    {
        // Arrange
        GetThemeArchiveRequest request = _getThemeArchiveRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = DomainErrors.Themes.ThemeIdCannotBeEmpty;
        _mockHandler.HandleAsync(Arg.Any<GetThemeArchiveQuery>(), Arg.Any<CancellationToken>())
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
        Assert.Equal(new[] { "ThemeIdCannotBeEmpty" }, validationProblemDetails.Errors["General.Validation"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetThemeArchiveQueryToHandler()
    {
        // Arrange
        GetThemeArchiveRequest request = _getThemeArchiveRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetThemeArchiveQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_themeArchiveResponseFixture.Create()));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetThemeArchiveQuery>(query => query.ThemeId == request.ThemeId),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetThemeArchiveRequest request = _getThemeArchiveRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetThemeArchiveQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_themeArchiveResponseFixture.Create());
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
