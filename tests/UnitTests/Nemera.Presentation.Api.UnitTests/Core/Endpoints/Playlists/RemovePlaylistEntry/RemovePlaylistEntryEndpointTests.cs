#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Playlists.Commands.RemovePlaylistEntry;
using Lumina.Contracts.Fixtures.Core.Responses.Playlists;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Playlists.RemovePlaylistEntry;
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

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Playlists.RemovePlaylistEntry;

/// <summary>
/// Contains unit tests for the <see cref="RemovePlaylistEntryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RemovePlaylistEntryEndpointTests
{
    private readonly ICommandHandler<RemovePlaylistEntryCommand, Result<PlaylistEntryRemovedResponse>> _mockHandler;
    private readonly RemovePlaylistEntryEndpoint _sut;
    private readonly PlaylistEntryRemovedResponseFixture _playlistEntryRemovedResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RemovePlaylistEntryEndpointTests"/> class.
    /// </summary>
    public RemovePlaylistEntryEndpointTests()
    {
        _mockHandler = Substitute.For<ICommandHandler<RemovePlaylistEntryCommand, Result<PlaylistEntryRemovedResponse>>>();
        _sut = FastEndpoints.Factory.Create<RemovePlaylistEntryEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithPlaylistEntryRemovedResponse()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        PlaylistEntryRemovedResponse expectedResponse = _playlistEntryRemovedResponseFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<RemovePlaylistEntryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));
        SetRouteValues(Guid.NewGuid(), Guid.NewGuid());

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        Ok<PlaylistEntryRemovedResponse> okResult = Assert.IsType<Ok<PlaylistEntryRemovedResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("Authorization", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsValidationErrors_ShouldReturnValidationProblemResultWithErrorsGroupedByCode()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        List<Error> expectedErrors = [DomainErrors.Playlists.PlaylistIdCannotBeEmpty, DomainErrors.Playlists.PlaylistEntryIdCannotBeEmpty];
        _mockHandler.HandleAsync(Arg.Any<RemovePlaylistEntryCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedErrors);
        SetRouteValues("not-a-guid", "not-an-entry-guid");

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

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
        Assert.Equal(new[] { "PlaylistIdCannotBeEmpty", "PlaylistEntryIdCannotBeEmpty" }, singleErrorGroup.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsPlaylistEntryNotFoundError_ShouldReturnNotFoundProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<RemovePlaylistEntryCommand>(), Arg.Any<CancellationToken>())
            .Returns(DomainErrors.Playlists.PlaylistEntryNotFound);
        SetRouteValues(Guid.NewGuid(), Guid.NewGuid());

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

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
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<RemovePlaylistEntryCommand>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Authorization.NotAuthorized);
        SetRouteValues(Guid.NewGuid(), Guid.NewGuid());

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

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
    public async Task ExecuteAsync_WhenCalled_ShouldSendRemovePlaylistEntryCommandWithTheRouteIds()
    {
        // Arrange
        Guid playlistId = Guid.NewGuid();
        Guid playlistEntryId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<RemovePlaylistEntryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playlistEntryRemovedResponseFixture.Create()));
        SetRouteValues(playlistId, playlistEntryId);

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<RemovePlaylistEntryCommand>(command =>
                command.PlaylistId == playlistId.ToString() &&
                command.PlaylistEntryId == playlistEntryId.ToString()),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreNotParseable_ShouldSendCommandWithTheRawRouteValues()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<RemovePlaylistEntryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playlistEntryRemovedResponseFixture.Create()));
        SetRouteValues("not-a-playlist-guid", "not-an-entry-guid");

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<RemovePlaylistEntryCommand>(command =>
                command.PlaylistId == "not-a-playlist-guid" &&
                command.PlaylistEntryId == "not-an-entry-guid"),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreMissing_ShouldSendCommandWithNullRouteValues()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<RemovePlaylistEntryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playlistEntryRemovedResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues.Remove("playlistId");
        _sut.HttpContext.Request.RouteValues.Remove("playlistEntryId");

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<RemovePlaylistEntryCommand>(command => command.PlaylistId == null && command.PlaylistEntryId == null),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        PlaylistEntryRemovedResponse expectedResponse = _playlistEntryRemovedResponseFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
        _mockHandler.HandleAsync(Arg.Any<RemovePlaylistEntryCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(expectedResponse);
            }, callInfo.Arg<CancellationToken>()));
        SetRouteValues(Guid.NewGuid(), Guid.NewGuid());

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cts.Token);
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
