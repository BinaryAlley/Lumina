#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Playlists.Commands.CreatePlaylist;
using Lumina.Contracts.Fixtures.Core.Requests.Playlists;
using Lumina.Contracts.Fixtures.Core.Responses.Playlists;
using Lumina.Contracts.Requests.Playlists;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Playlists;
using Lumina.Presentation.Api.Core.Endpoints.Playlists.CreatePlaylist;
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

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Playlists.CreatePlaylist;

/// <summary>
/// Contains unit tests for the <see cref="CreatePlaylistEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CreatePlaylistEndpointTests
{
    private readonly ICommandHandler<CreatePlaylistCommand, Result<PlaylistResponse>> _mockHandler;
    private readonly CreatePlaylistEndpoint _sut;
    private readonly CreatePlaylistRequestFixture _createPlaylistRequestFixture = new();
    private readonly PlaylistResponseFixture _playlistResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePlaylistEndpointTests"/> class.
    /// </summary>
    public CreatePlaylistEndpointTests()
    {
        _mockHandler = Substitute.For<ICommandHandler<CreatePlaylistCommand, Result<PlaylistResponse>>>();
        _sut = FastEndpoints.Factory.Create<CreatePlaylistEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithPlaylistResponse()
    {
        // Arrange
        CreatePlaylistRequest request = _createPlaylistRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        PlaylistResponse expectedResponse = _playlistResponseFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<CreatePlaylistCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<PlaylistResponse> okResult = Assert.IsType<Ok<PlaylistResponse>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
        Assert.Equal("private, no-store", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("Authorization", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsValidationErrors_ShouldReturnValidationProblemResultWithErrorsGroupedByCode()
    {
        // Arrange
        CreatePlaylistRequest request = _createPlaylistRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        List<Error> expectedErrors = [DomainErrors.Playlists.PlaylistNameCannotBeEmpty, DomainErrors.Playlists.PlaylistNameMustBeMaximum255CharactersLong];
        _mockHandler.HandleAsync(Arg.Any<CreatePlaylistCommand>(), Arg.Any<CancellationToken>())
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
        Assert.Equal(new[] { "PlaylistNameCannotBeEmpty", "PlaylistNameMustBeMaximum255CharactersLong" }, singleErrorGroup.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsPlaylistAlreadyExistsError_ShouldReturnConflictProblemResult()
    {
        // Arrange
        CreatePlaylistRequest request = _createPlaylistRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<CreatePlaylistCommand>(), Arg.Any<CancellationToken>())
            .Returns(DomainErrors.Playlists.PlaylistAlreadyExists);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status409Conflict, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        ProblemDetails problemDetails = Assert.IsType<ProblemDetails>(problemResult.ProblemDetails);
        Assert.Equal(StatusCodes.Status409Conflict, problemDetails.Status);
        Assert.Equal("General.Conflict", problemDetails.Title);
        Assert.Equal("PlaylistAlreadyExists", problemDetails.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.10", problemDetails.Type);
        Assert.NotNull(problemDetails.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotAuthorizedError_ShouldReturnForbiddenProblemResult()
    {
        // Arrange
        CreatePlaylistRequest request = _createPlaylistRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<CreatePlaylistCommand>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Authorization.NotAuthorized);

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
    public async Task ExecuteAsync_WhenCalled_ShouldSendCreatePlaylistCommandWithTheMappedFields()
    {
        // Arrange
        CreatePlaylistRequest request = _createPlaylistRequestFixture.Create(
            name: "Road trip",
            description: "Songs for the highway.",
            type: PlaylistType.Music);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<CreatePlaylistCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playlistResponseFixture.Create()));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<CreatePlaylistCommand>(command =>
                command.Name == "Road trip" &&
                command.Description == "Songs for the highway." &&
                command.Type == PlaylistType.Music),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenOptionalFieldsAreMissing_ShouldSendCommandWithNullDescriptionAndType()
    {
        // Arrange
        CreatePlaylistRequest request = _createPlaylistRequestFixture.Create(includeDescription: false, includeType: false);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<CreatePlaylistCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_playlistResponseFixture.Create()));

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<CreatePlaylistCommand>(command =>
                command.Name == request.Name &&
                command.Description == null &&
                command.Type == null),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CreatePlaylistRequest request = _createPlaylistRequestFixture.Create();
        PlaylistResponse expectedResponse = _playlistResponseFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();
        _mockHandler.HandleAsync(Arg.Any<CreatePlaylistCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(expectedResponse);
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
