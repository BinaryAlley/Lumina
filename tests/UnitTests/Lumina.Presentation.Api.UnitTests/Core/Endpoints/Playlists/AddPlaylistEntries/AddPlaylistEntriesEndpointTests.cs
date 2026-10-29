#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Playlists.Commands.AddPlaylistEntries;
using Lumina.Contracts.Fixtures.Core.Requests.Playlists;
using Lumina.Contracts.Fixtures.Core.Responses.Playlists;
using Lumina.Contracts.Requests.Playlists;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Playlists.AddPlaylistEntries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Playlists.AddPlaylistEntries;

/// <summary>
/// Contains unit tests for the <see cref="AddPlaylistEntriesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistEntriesEndpointTests
{
    private readonly ICommandHandler<AddPlaylistEntriesCommand, Result<PlaylistEntriesAddedResponse>> _mockHandler;
    private readonly AddPlaylistEntriesEndpoint _sut;
    private readonly AddPlaylistEntriesRequestFixture _addPlaylistEntriesRequestFixture = new();
    private readonly PlaylistEntriesAddedResponseFixture _playlistEntriesAddedResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistEntriesEndpointTests"/> class.
    /// </summary>
    public AddPlaylistEntriesEndpointTests()
    {
        _mockHandler = Substitute.For<ICommandHandler<AddPlaylistEntriesCommand, Result<PlaylistEntriesAddedResponse>>>();
        _sut = FastEndpoints.Factory.Create<AddPlaylistEntriesEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithPlaylistEntriesAddedResponse()
    {
        // Arrange
        AddPlaylistEntriesRequest request = _addPlaylistEntriesRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        PlaylistEntriesAddedResponse expectedResponse = _playlistEntriesAddedResponseFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<AddPlaylistEntriesCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));
        SetRoutePlaylistId(Guid.NewGuid());

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<PlaylistEntriesAddedResponse> okResult = Assert.IsType<Ok<PlaylistEntriesAddedResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("Authorization", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsValidationErrors_ShouldReturnValidationProblemResultWithErrorsGroupedByCode()
    {
        // Arrange
        AddPlaylistEntriesRequest request = _addPlaylistEntriesRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        List<Error> expectedErrors = [DomainErrors.Playlists.PlaylistIdCannotBeEmpty, DomainErrors.Playlists.PlaylistEntryPositionsAreInvalid];
        _mockHandler.HandleAsync(Arg.Any<AddPlaylistEntriesCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedErrors);
        SetRoutePlaylistId("not-a-guid");

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
        Assert.Equal(new[] { "PlaylistIdCannotBeEmpty", "PlaylistEntryPositionsAreInvalid" }, singleErrorGroup.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsPlaylistNotFoundError_ShouldReturnNotFoundProblemResult()
    {
        // Arrange
        AddPlaylistEntriesRequest request = _addPlaylistEntriesRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<AddPlaylistEntriesCommand>(), Arg.Any<CancellationToken>())
            .Returns(DomainErrors.Playlists.PlaylistNotFound);
        SetRoutePlaylistId(Guid.NewGuid());

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        ProblemDetails problemDetails = Assert.IsType<ProblemDetails>(problemResult.ProblemDetails);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
        Assert.Equal("General.NotFound", problemDetails.Title);
        Assert.Equal("PlaylistNotFound", problemDetails.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problemDetails.Type);
        Assert.NotNull(problemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotAuthorizedError_ShouldReturnForbiddenProblemResult()
    {
        // Arrange
        AddPlaylistEntriesRequest request = _addPlaylistEntriesRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<AddPlaylistEntriesCommand>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Authorization.NotAuthorized);
        SetRoutePlaylistId(Guid.NewGuid());

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
    public async Task ExecuteAsync_WhenCalled_ShouldSendAddPlaylistEntriesCommandWithTheRouteIdAndTrackIds()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        List<Guid> trackIds = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        AddPlaylistEntriesRequest request = _addPlaylistEntriesRequestFixture.Create(trackIds: trackIds);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<AddPlaylistEntriesCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playlistEntriesAddedResponseFixture.Create()));
        SetRoutePlaylistId(playlistId);

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<AddPlaylistEntriesCommand>(command =>
                command.PlaylistId == playlistId.ToString() &&
                command.TrackIds != null &&
                command.TrackIds.SequenceEqual(trackIds)),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenTrackIdsAreNull_ShouldSendCommandWithNullTrackIds()
    {
        // Arrange
        AddPlaylistEntriesRequest request = _addPlaylistEntriesRequestFixture.Create(includeTrackIds: false);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<AddPlaylistEntriesCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playlistEntriesAddedResponseFixture.Create()));
        SetRoutePlaylistId(Guid.NewGuid());

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<AddPlaylistEntriesCommand>(command => command.TrackIds == null),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValueIsNotParseable_ShouldSendCommandWithTheRawRouteValue()
    {
        // Arrange
        AddPlaylistEntriesRequest request = _addPlaylistEntriesRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<AddPlaylistEntriesCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playlistEntriesAddedResponseFixture.Create()));
        SetRoutePlaylistId("not-a-playlist-guid");

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<AddPlaylistEntriesCommand>(command => command.PlaylistId == "not-a-playlist-guid"),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValueIsMissing_ShouldSendCommandWithNullPlaylistId()
    {
        // Arrange
        AddPlaylistEntriesRequest request = _addPlaylistEntriesRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<AddPlaylistEntriesCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playlistEntriesAddedResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues.Remove("playlistId");

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<AddPlaylistEntriesCommand>(command => command.PlaylistId == null),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        AddPlaylistEntriesRequest request = _addPlaylistEntriesRequestFixture.Create();
        PlaylistEntriesAddedResponse expectedResponse = _playlistEntriesAddedResponseFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
        _mockHandler.HandleAsync(Arg.Any<AddPlaylistEntriesCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(expectedResponse);
            }, callInfo.Arg<CancellationToken>()));
        SetRoutePlaylistId(Guid.NewGuid());

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(request, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }

    /// <summary>
    /// Sets the route value of the playlist identifier route segment.
    /// </summary>
    /// <param name="playlistId">The value of the playlist identifier route segment.</param>
    private void SetRoutePlaylistId(object playlistId)
    {
        _sut.HttpContext.Request.RouteValues["playlistId"] = playlistId;
    }
}
