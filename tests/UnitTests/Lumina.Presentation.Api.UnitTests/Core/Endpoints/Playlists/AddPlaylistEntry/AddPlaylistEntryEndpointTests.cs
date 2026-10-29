#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Playlists.Commands.AddPlaylistEntry;
using Lumina.Contracts.Fixtures.Core.Requests.Playlists;
using Lumina.Contracts.Fixtures.Core.Responses.Playlists;
using Lumina.Contracts.Requests.Playlists;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Playlists.AddPlaylistEntry;
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

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Playlists.AddPlaylistEntry;

/// <summary>
/// Contains unit tests for the <see cref="AddPlaylistEntryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistEntryEndpointTests
{
    private readonly ICommandHandler<AddPlaylistEntryCommand, Result<PlaylistEntryAddedResponse>> _mockHandler;
    private readonly AddPlaylistEntryEndpoint _sut;
    private readonly AddPlaylistEntryRequestFixture _addPlaylistEntryRequestFixture = new();
    private readonly PlaylistEntryAddedResponseFixture _playlistEntryAddedResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistEntryEndpointTests"/> class.
    /// </summary>
    public AddPlaylistEntryEndpointTests()
    {
        _mockHandler = Substitute.For<ICommandHandler<AddPlaylistEntryCommand, Result<PlaylistEntryAddedResponse>>>();
        _sut = FastEndpoints.Factory.Create<AddPlaylistEntryEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithPlaylistEntryAddedResponse()
    {
        // Arrange
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        PlaylistEntryAddedResponse expectedResponse = _playlistEntryAddedResponseFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<AddPlaylistEntryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));
        SetRoutePlaylistId(Guid.NewGuid());

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<PlaylistEntryAddedResponse> okResult = Assert.IsType<Ok<PlaylistEntryAddedResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("Authorization", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsValidationErrors_ShouldReturnValidationProblemResultWithErrorsGroupedByCode()
    {
        // Arrange
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        List<Error> expectedErrors = [DomainErrors.Playlists.PlaylistIdCannotBeEmpty, DomainErrors.Playlists.PlaylistEntryPositionsAreInvalid];
        _mockHandler.HandleAsync(Arg.Any<AddPlaylistEntryCommand>(), Arg.Any<CancellationToken>())
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
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<AddPlaylistEntryCommand>(), Arg.Any<CancellationToken>())
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
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<AddPlaylistEntryCommand>(), Arg.Any<CancellationToken>())
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
    public async Task ExecuteAsync_WhenCalled_ShouldSendAddPlaylistEntryCommandWithTheRouteIdAndTrackId()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create(trackId: trackId);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<AddPlaylistEntryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playlistEntryAddedResponseFixture.Create()));
        SetRoutePlaylistId(playlistId);

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<AddPlaylistEntryCommand>(command =>
                command.PlaylistId == playlistId.ToString() &&
                command.TrackId == trackId),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenTrackIdIsNull_ShouldSendCommandWithNullTrackId()
    {
        // Arrange
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create(includeTrackId: false);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<AddPlaylistEntryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playlistEntryAddedResponseFixture.Create()));
        SetRoutePlaylistId(Guid.NewGuid());

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<AddPlaylistEntryCommand>(command => command.TrackId == null),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValueIsNotParseable_ShouldSendCommandWithTheRawRouteValue()
    {
        // Arrange
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<AddPlaylistEntryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playlistEntryAddedResponseFixture.Create()));
        SetRoutePlaylistId("not-a-playlist-guid");

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<AddPlaylistEntryCommand>(command => command.PlaylistId == "not-a-playlist-guid"),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValueIsMissing_ShouldSendCommandWithNullPlaylistId()
    {
        // Arrange
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<AddPlaylistEntryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playlistEntryAddedResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues.Remove("playlistId");

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<AddPlaylistEntryCommand>(command => command.PlaylistId == null),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create();
        PlaylistEntryAddedResponse expectedResponse = _playlistEntryAddedResponseFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
        _mockHandler.HandleAsync(Arg.Any<AddPlaylistEntryCommand>(), Arg.Any<CancellationToken>())
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
