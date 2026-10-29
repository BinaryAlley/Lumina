#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Favorites.Commands.AddFavorite;
using Lumina.Contracts.Fixtures.Core.Requests.Favorites;
using Lumina.Contracts.Fixtures.Core.Responses.Favorites;
using Lumina.Contracts.Requests.Favorites;
using Lumina.Contracts.Responses.Favorites;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using Lumina.Presentation.Api.Core.Endpoints.Favorites.AddFavorite;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Favorites.AddFavorite;

/// <summary>
/// Contains unit tests for the <see cref="AddFavoriteEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddFavoriteEndpointTests
{
    private readonly ICommandHandler<AddFavoriteCommand, Result<FavoriteResponse>> _mockHandler;
    private readonly AddFavoriteEndpoint _sut;
    private readonly AddFavoriteRequestFixture _addFavoriteRequestFixture = new();
    private readonly FavoriteResponseFixture _favoriteResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AddFavoriteEndpointTests"/> class.
    /// </summary>
    public AddFavoriteEndpointTests()
    {
        _mockHandler = Substitute.For<ICommandHandler<AddFavoriteCommand, Result<FavoriteResponse>>>();
        _sut = FastEndpoints.Factory.Create<AddFavoriteEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithFavoriteResponse()
    {
        // Arrange
        AddFavoriteRequest request = _addFavoriteRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        FavoriteResponse expectedResponse = _favoriteResponseFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<AddFavoriteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<FavoriteResponse> okResult = Assert.IsType<Ok<FavoriteResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldSetPrivateNoStoreCacheHeaders()
    {
        // Arrange
        AddFavoriteRequest request = _addFavoriteRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<AddFavoriteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_favoriteResponseFixture.Create()));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("Authorization", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsValidationErrors_ShouldReturnValidationProblemResultWithErrorsGroupedByCode()
    {
        // Arrange
        AddFavoriteRequest request = _addFavoriteRequestFixture.Create(includeMediaKind: false, includeMediaItemId: false);
        CancellationToken cancellationToken = CancellationToken.None;
        List<Error> expectedErrors = [DomainErrors.Favorites.FavoriteMediaKindIsInvalid, DomainErrors.Favorites.FavoriteMediaItemIdCannotBeEmpty];
        _mockHandler.HandleAsync(Arg.Any<AddFavoriteCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedErrors);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        HttpValidationProblemDetails validationProblemDetails = Assert.IsType<HttpValidationProblemDetails>(problemResult.ProblemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, validationProblemDetails.Status);
        Assert.Equal("General.Validation", validationProblemDetails.Title);
        Assert.Equal("OneOrMoreValidationErrorsOccurred", validationProblemDetails.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", validationProblemDetails.Type);
        Assert.NotNull(validationProblemDetails.Extensions["traceId"]);
        KeyValuePair<string, string[]> singleErrorGroup = Assert.Single(validationProblemDetails.Errors);
        Assert.Equal("General.Validation", singleErrorGroup.Key);
        Assert.Equal(new[] { "FavoriteMediaKindIsInvalid", "FavoriteMediaItemIdCannotBeEmpty" }, singleErrorGroup.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldStillSetPrivateNoStoreCacheHeaders()
    {
        // Arrange
        AddFavoriteRequest request = _addFavoriteRequestFixture.Create();
        Error expectedError = DomainErrors.Music.TrackNotFound;
        _mockHandler.HandleAsync(Arg.Any<AddFavoriteCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("Authorization", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotAuthorizedError_ShouldReturnForbiddenProblemResult()
    {
        // Arrange
        AddFavoriteRequest request = _addFavoriteRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = ApplicationErrors.Authorization.NotAuthorized;
        _mockHandler.HandleAsync(Arg.Any<AddFavoriteCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        ProblemDetails problemDetails = Assert.IsType<ProblemDetails>(problemResult.ProblemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails.Status);
        Assert.Equal("General.Unauthorized", problemDetails.Title);
        Assert.Equal("NotAuthorized", problemDetails.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetails.Type);
        Assert.NotNull(problemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsUnsupportedMediaKindError_ShouldReturnValidationProblemResult()
    {
        // Arrange
        AddFavoriteRequest request = _addFavoriteRequestFixture.Create(mediaKind: FavoriteMediaKind.Video);
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = DomainErrors.Favorites.FavoriteMediaKindIsNotSupported;
        _mockHandler.HandleAsync(Arg.Any<AddFavoriteCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemResult.StatusCode);
        HttpValidationProblemDetails validationProblemDetails = Assert.IsType<HttpValidationProblemDetails>(problemResult.ProblemDetails);
        Assert.Equal("General.Validation", validationProblemDetails.Title);
        Assert.Equal("OneOrMoreValidationErrorsOccurred", validationProblemDetails.Detail);
        Assert.Equal(new[] { "FavoriteMediaKindIsNotSupported" }, validationProblemDetails.Errors["General.Validation"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsTrackNotFoundError_ShouldReturnNotFoundProblemResult()
    {
        // Arrange
        AddFavoriteRequest request = _addFavoriteRequestFixture.Create(mediaKind: FavoriteMediaKind.MusicTrack);
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = DomainErrors.Music.TrackNotFound;
        _mockHandler.HandleAsync(Arg.Any<AddFavoriteCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        ProblemDetails problemDetails = Assert.IsType<ProblemDetails>(problemResult.ProblemDetails);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
        Assert.Equal("General.NotFound", problemDetails.Title);
        Assert.Equal("TrackNotFound", problemDetails.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problemDetails.Type);
        Assert.NotNull(problemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsBookNotFoundError_ShouldReturnNotFoundProblemResultWithTheBookDetail()
    {
        // Arrange
        AddFavoriteRequest request = _addFavoriteRequestFixture.Create(mediaKind: FavoriteMediaKind.Book);
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = DomainErrors.WrittenContent.BookNotFound;
        _mockHandler.HandleAsync(Arg.Any<AddFavoriteCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemResult.StatusCode);
        ProblemDetails problemDetails = Assert.IsType<ProblemDetails>(problemResult.ProblemDetails);
        Assert.Equal("General.NotFound", problemDetails.Title);
        Assert.Equal("BookNotFound", problemDetails.Detail);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsConcurrencyConflictError_ShouldReturnConflictProblemResult()
    {
        // Arrange
        AddFavoriteRequest request = _addFavoriteRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = ApplicationErrors.Persistence.ConcurrencyConflict;
        _mockHandler.HandleAsync(Arg.Any<AddFavoriteCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status409Conflict, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        ProblemDetails problemDetails = Assert.IsType<ProblemDetails>(problemResult.ProblemDetails);
        Assert.Equal(StatusCodes.Status409Conflict, problemDetails.Status);
        Assert.Equal("General.Conflict", problemDetails.Title);
        Assert.Equal("ConcurrencyConflict", problemDetails.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.10", problemDetails.Type);
        Assert.NotNull(problemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendAddFavoriteCommandToSender()
    {
        // Arrange
        Guid mediaItemId = Guid.NewGuid();
        AddFavoriteRequest request = _addFavoriteRequestFixture.Create(mediaKind: FavoriteMediaKind.MusicAlbum, mediaItemId: mediaItemId);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<AddFavoriteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_favoriteResponseFixture.Create()));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<AddFavoriteCommand>(command => command.MediaKind == FavoriteMediaKind.MusicAlbum && command.MediaItemId == mediaItemId),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRequestValuesAreNull_ShouldSendCommandWithNullValues()
    {
        // Arrange
        AddFavoriteRequest request = _addFavoriteRequestFixture.Create(includeMediaKind: false, includeMediaItemId: false);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<AddFavoriteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_favoriteResponseFixture.Create()));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<AddFavoriteCommand>(command => command.MediaKind == null && command.MediaItemId == null),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        AddFavoriteRequest request = _addFavoriteRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<AddFavoriteCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_favoriteResponseFixture.Create());
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
