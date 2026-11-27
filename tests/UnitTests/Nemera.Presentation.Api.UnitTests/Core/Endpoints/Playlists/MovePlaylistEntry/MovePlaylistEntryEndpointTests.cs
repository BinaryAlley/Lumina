#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Playlists.Commands.MovePlaylistEntry;
using Lumina.Contracts.Fixtures.Core.Requests.Playlists;
using Lumina.Contracts.Fixtures.Core.Responses.Playlists;
using Lumina.Contracts.Requests.Playlists;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Playlists.MovePlaylistEntry;
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

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Playlists.MovePlaylistEntry;

/// <summary>
/// Contains unit tests for the <see cref="MovePlaylistEntryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MovePlaylistEntryEndpointTests
{
    private readonly ICommandHandler<MovePlaylistEntryCommand, Result<PlaylistEntryMovedResponse>> _mockHandler;
    private readonly MovePlaylistEntryEndpoint _sut;
    private readonly MovePlaylistEntryRequestFixture _movePlaylistEntryRequestFixture = new();
    private readonly PlaylistEntryMovedResponseFixture _playlistEntryMovedResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MovePlaylistEntryEndpointTests"/> class.
    /// </summary>
    public MovePlaylistEntryEndpointTests()
    {
        _mockHandler = Substitute.For<ICommandHandler<MovePlaylistEntryCommand, Result<PlaylistEntryMovedResponse>>>();
        _sut = FastEndpoints.Factory.Create<MovePlaylistEntryEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithPlaylistEntryMovedResponse()
    {
        // Arrange
        MovePlaylistEntryRequest request = _movePlaylistEntryRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        PlaylistEntryMovedResponse expectedResponse = _playlistEntryMovedResponseFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<MovePlaylistEntryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));
        SetRouteValues(Guid.NewGuid(), Guid.NewGuid());

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<PlaylistEntryMovedResponse> okResult = Assert.IsType<Ok<PlaylistEntryMovedResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("Authorization", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsValidationErrors_ShouldReturnValidationProblemResultWithErrorsGroupedByCode()
    {
        // Arrange
        MovePlaylistEntryRequest request = _movePlaylistEntryRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        List<Error> expectedErrors = [DomainErrors.Playlists.PlaylistEntryIdCannotBeEmpty, DomainErrors.Playlists.PlaylistMoveTargetPositionIsInvalid];
        _mockHandler.HandleAsync(Arg.Any<MovePlaylistEntryCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedErrors);
        SetRouteValues("not-a-guid", "not-an-entry-guid");

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
        Assert.Equal(new[] { "PlaylistEntryIdCannotBeEmpty", "PlaylistMoveTargetPositionIsInvalid" }, singleErrorGroup.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsPlaylistEntryNotFoundError_ShouldReturnNotFoundProblemResult()
    {
        // Arrange
        MovePlaylistEntryRequest request = _movePlaylistEntryRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<MovePlaylistEntryCommand>(), Arg.Any<CancellationToken>())
            .Returns(DomainErrors.Playlists.PlaylistEntryNotFound);
        SetRouteValues(Guid.NewGuid(), Guid.NewGuid());

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        ProblemDetails problemDetails = Assert.IsType<ProblemDetails>(problemResult.ProblemDetails);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
        Assert.Equal("General.NotFound", problemDetails.Title);
        Assert.Equal("PlaylistEntryNotFound", problemDetails.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problemDetails.Type);
        Assert.NotNull(problemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotAuthorizedError_ShouldReturnForbiddenProblemResult()
    {
        // Arrange
        MovePlaylistEntryRequest request = _movePlaylistEntryRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<MovePlaylistEntryCommand>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Authorization.NotAuthorized);
        SetRouteValues(Guid.NewGuid(), Guid.NewGuid());

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
    public async Task ExecuteAsync_WhenCalled_ShouldSendMovePlaylistEntryCommandWithTheRouteIdsAndTargetPosition()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        Guid playlistEntryId = Guid.NewGuid();
        MovePlaylistEntryRequest request = _movePlaylistEntryRequestFixture.Create(targetPosition: 7);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<MovePlaylistEntryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playlistEntryMovedResponseFixture.Create()));
        SetRouteValues(playlistId, playlistEntryId);

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<MovePlaylistEntryCommand>(command =>
                command.PlaylistId == playlistId.ToString() &&
                command.PlaylistEntryId == playlistEntryId.ToString() &&
                command.TargetPosition == 7),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenTargetPositionIsMissing_ShouldSendCommandWithNullTargetPosition()
    {
        // Arrange
        MovePlaylistEntryRequest request = _movePlaylistEntryRequestFixture.Create(includeTargetPosition: false);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<MovePlaylistEntryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playlistEntryMovedResponseFixture.Create()));
        SetRouteValues(Guid.NewGuid(), Guid.NewGuid());

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<MovePlaylistEntryCommand>(command => command.TargetPosition == null),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreNotParseable_ShouldSendCommandWithTheRawRouteValues()
    {
        // Arrange
        MovePlaylistEntryRequest request = _movePlaylistEntryRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<MovePlaylistEntryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playlistEntryMovedResponseFixture.Create()));
        SetRouteValues("not-a-playlist-guid", "not-an-entry-guid");

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<MovePlaylistEntryCommand>(command =>
                command.PlaylistId == "not-a-playlist-guid" &&
                command.PlaylistEntryId == "not-an-entry-guid"),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreMissing_ShouldSendCommandWithNullRouteValues()
    {
        // Arrange
        MovePlaylistEntryRequest request = _movePlaylistEntryRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<MovePlaylistEntryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playlistEntryMovedResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues.Remove("playlistId");
        _sut.HttpContext.Request.RouteValues.Remove("playlistEntryId");

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<MovePlaylistEntryCommand>(command => command.PlaylistId == null && command.PlaylistEntryId == null),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        MovePlaylistEntryRequest request = _movePlaylistEntryRequestFixture.Create();
        PlaylistEntryMovedResponse expectedResponse = _playlistEntryMovedResponseFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
        _mockHandler.HandleAsync(Arg.Any<MovePlaylistEntryCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(expectedResponse);
            }, callInfo.Arg<CancellationToken>()));
        SetRouteValues(Guid.NewGuid(), Guid.NewGuid());

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(request, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }

    /// <summary>
    /// Sets the route values of the playlist and playlist entry identifier route segments.
    /// </summary>
    /// <param name="playlistId">The value of the playlist identifier route segment.</param>
    /// <param name="playlistEntryId">The value of the playlist entry identifier route segment.</param>
    private void SetRouteValues(object playlistId, object playlistEntryId)
    {
        _sut.HttpContext.Request.RouteValues["playlistId"] = playlistId;
        _sut.HttpContext.Request.RouteValues["playlistEntryId"] = playlistEntryId;
    }
}
