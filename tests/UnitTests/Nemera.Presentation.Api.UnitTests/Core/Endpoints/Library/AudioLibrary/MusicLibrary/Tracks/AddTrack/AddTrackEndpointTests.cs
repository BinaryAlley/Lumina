#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.AddTrack;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.AddTrack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.AddTrack;

/// <summary>
/// Contains unit tests for the <see cref="AddTrackEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddTrackEndpointTests
{
    private readonly Lumina.Application.Common.CQRS.ICommandHandler<AddTrackCommand, Result<TrackResponse>> _mockHandler;
    private readonly AddTrackEndpoint _sut;
    private readonly AddTrackRequestFixture _addTrackRequestFixture = new();
    private readonly TrackResponseFixture _trackResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AddTrackEndpointTests"/> class.
    /// </summary>
    public AddTrackEndpointTests()
    {
        _mockHandler = Substitute.For<Lumina.Application.Common.CQRS.ICommandHandler<AddTrackCommand, Result<TrackResponse>>>();
        _sut = FastEndpoints.Factory.Create<AddTrackEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnCreatedResultWithLocationAndTrackResponse()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        AddTrackRequest request = _addTrackRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        TrackResponse expectedResponse = _trackResponseFixture.Create();
        _mockHandler.HandleAsync(Arg.Any<AddTrackCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["artistId"] = artistId.ToString();
        _sut.HttpContext.Request.RouteValues["albumId"] = albumId.ToString();

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Created<TrackResponse> createdResult = Assert.IsType<Created<TrackResponse>>(result);
        Assert.Equal(expectedResponse, createdResult.Value);
        Assert.NotNull(createdResult.Location);
        Assert.EndsWith($"/api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{expectedResponse.Id}", createdResult.Location);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotFoundError_ShouldReturnProblemResult()
    {
        // Arrange
        AddTrackRequest request = _addTrackRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.NotFound("Music.ArtistNotFound", "ArtistNotFound");
        _mockHandler.HandleAsync(Arg.Any<AddTrackCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        Microsoft.AspNetCore.Mvc.ProblemDetails problemDetailsBody = Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemDetails.ProblemDetails);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetailsBody.Status);
        Assert.Equal("Music.ArtistNotFound", problemDetailsBody.Title);
        Assert.Equal("ArtistNotFound", problemDetailsBody.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problemDetailsBody.Type);
        Assert.NotNull(problemDetailsBody.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsValidationErrors_ShouldReturnValidationProblemResult()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        AddTrackRequest request = _addTrackRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error validationError = Errors.Library.LibraryIdCannotBeEmpty;
        _mockHandler.HandleAsync(Arg.Any<AddTrackCommand>(), Arg.Any<CancellationToken>())
            .Returns(validationError);
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();

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
        Assert.Equal(new[] { "LibraryIdCannotBeEmpty" }, validationProblemDetails.Errors["General.Validation"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendAddTrackCommandToSender()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        AddTrackRequest request = _addTrackRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<AddTrackCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_trackResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["artistId"] = artistId.ToString();
        _sut.HttpContext.Request.RouteValues["albumId"] = albumId.ToString();

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<AddTrackCommand>(command =>
                command.LibraryId == libraryId.ToString() &&
                command.ArtistId == artistId.ToString() &&
                command.AlbumId == albumId.ToString() &&
                command.Path == request.Path &&
                command.Metadata == request.Metadata &&
                command.TrackNumber == request.TrackNumber &&
                command.DiscNumber == request.DiscNumber &&
                command.Script == request.Script &&
                command.Key == request.Key &&
                command.Bpm == request.Bpm &&
                command.Work == request.Work &&
                command.MusicBrainzRecordingId == request.MusicBrainzRecordingId &&
                command.MusicBrainzTrackId == request.MusicBrainzTrackId &&
                command.Moods == request.Moods &&
                command.Isrcs == request.Isrcs &&
                command.Contributors == request.Contributors &&
                command.Ratings == request.Ratings),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreNotParseable_ShouldSendCommandWithRawRouteValues()
    {
        // Arrange
        AddTrackRequest request = _addTrackRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<AddTrackCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_trackResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues["libraryId"] = "not-a-library-guid";
        _sut.HttpContext.Request.RouteValues["artistId"] = "not-an-artist-guid";
        _sut.HttpContext.Request.RouteValues["albumId"] = "not-an-album-guid";

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<AddTrackCommand>(command =>
                command.LibraryId == "not-a-library-guid" &&
                command.ArtistId == "not-an-artist-guid" &&
                command.AlbumId == "not-an-album-guid"),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreMissing_ShouldSendCommandWithNullRouteValues()
    {
        // Arrange
        AddTrackRequest request = _addTrackRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<AddTrackCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_trackResponseFixture.Create()));
        _sut.HttpContext.Request.RouteValues.Remove("libraryId");
        _sut.HttpContext.Request.RouteValues.Remove("artistId");
        _sut.HttpContext.Request.RouteValues.Remove("albumId");

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<AddTrackCommand>(command =>
                command.LibraryId == null &&
                command.ArtistId == null &&
                command.AlbumId == null),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        AddTrackRequest request = _addTrackRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<AddTrackCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_trackResponseFixture.Create());
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
