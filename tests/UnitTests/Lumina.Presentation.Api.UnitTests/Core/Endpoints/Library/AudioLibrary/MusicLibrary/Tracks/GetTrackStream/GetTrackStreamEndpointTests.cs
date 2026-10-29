#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrackStream;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetTrackStream;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetTrackStream;

/// <summary>
/// Contains unit tests for the <see cref="GetTrackStreamEndpoint"/> class.
/// </summary>
/// <remarks>
/// The endpoint returns a <see cref="PhysicalFileHttpResult"/>, so these tests verify everything the endpoint resolves itself: the status, the
/// media type, the entity tag, the last modification time, the range processing flag and the response headers. The actual bytes of the file and
/// the range and conditional request handling are performed by the framework when the result is executed, and are covered by the API
/// integration tests.
/// </remarks>
[ExcludeFromCodeCoverage]
public class GetTrackStreamEndpointTests
{
    private readonly IQueryHandler<GetTrackStreamQuery, Result<TrackStreamDataDto>> _mockHandler;
    private readonly GetTrackStreamEndpoint _sut;
    private readonly TrackStreamDataDtoFixture _trackStreamDataDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTrackStreamEndpointTests"/> class.
    /// </summary>
    public GetTrackStreamEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetTrackStreamQuery, Result<TrackStreamDataDto>>>();
        _sut = Factory.Create<GetTrackStreamEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnPhysicalFileResultWithTheStreamData()
    {
        // Arrange
        DateTimeOffset lastModified = new(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);
        TrackStreamDataDto streamData = _trackStreamDataDtoFixture.Create(
            path: @"C:\music\track.flac",
            format: AudioFileFormat.FLAC,
            length: 1234,
            lastModified: lastModified);
        _mockHandler.HandleAsync(Arg.Any<GetTrackStreamQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(streamData));
        SetRouteValues();

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        PhysicalFileHttpResult fileResult = Assert.IsType<PhysicalFileHttpResult>(result);
        Assert.Equal(streamData.Path, fileResult.FileName);
        Assert.Equal("audio/flac", fileResult.ContentType);
        Assert.Null(fileResult.FileDownloadName);
        Assert.True(fileResult.EnableRangeProcessing);
        Assert.Equal(lastModified, fileResult.LastModified);
        Assert.NotNull(fileResult.EntityTag);
        Assert.Equal($"\"{lastModified.ToUnixTimeSeconds()}-{streamData.Length}\"", fileResult.EntityTag.Tag.ToString());
    }

    [Theory]
    [InlineData(AudioFileFormat.MP3, "audio/mpeg")] // mp3 audio
    [InlineData(AudioFileFormat.FLAC, "audio/flac")] // flac audio
    [InlineData(AudioFileFormat.OGG, "audio/ogg")] // ogg audio
    [InlineData(AudioFileFormat.None, "application/octet-stream")] // an unrecognized format stays opaque binary
    public async Task ExecuteAsync_WhenSuccessful_ShouldServeTheMediaTypeOfTheFormat(AudioFileFormat format, string expectedContentType)
    {
        // Arrange
        TrackStreamDataDto streamData = _trackStreamDataDtoFixture.Create(format: format);
        _mockHandler.HandleAsync(Arg.Any<GetTrackStreamQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(streamData));
        SetRouteValues();

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        PhysicalFileHttpResult fileResult = Assert.IsType<PhysicalFileHttpResult>(result);
        Assert.Equal(expectedContentType, fileResult.ContentType);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldDisableContentSniffingAndKeepTheStreamPrivate()
    {
        // Arrange
        _mockHandler.HandleAsync(Arg.Any<GetTrackStreamQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_trackStreamDataDtoFixture.Create()));
        SetRouteValues();

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        Assert.Equal("nosniff", _sut.HttpContext.Response.Headers["X-Content-Type-Options"].ToString());
        Assert.Equal("private", _sut.HttpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("Authorization", _sut.HttpContext.Response.Headers["Vary"].ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTrackIsNotFound_ShouldReturnNotFoundProblemResult()
    {
        // Arrange
        _mockHandler.HandleAsync(Arg.Any<GetTrackStreamQuery>(), Arg.Any<CancellationToken>())
            .Returns(DomainErrors.Music.TrackNotFound);
        SetRouteValues();

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemResult.StatusCode);
        Assert.Equal(DomainErrors.Music.TrackNotFound.Code, problemResult.ProblemDetails.Title);
        Assert.Equal(DomainErrors.Music.TrackNotFound.Description, problemResult.ProblemDetails.Detail);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheFileIsOutsideEveryContentLocation_ShouldReturnNotFoundProblemResultWithoutLeakingThePath()
    {
        // Arrange
        _mockHandler.HandleAsync(Arg.Any<GetTrackStreamQuery>(), Arg.Any<CancellationToken>())
            .Returns(DomainErrors.Music.TrackFileNotFound);
        SetRouteValues();

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemResult.StatusCode);
        Assert.Equal("TrackFileNotFound", problemResult.ProblemDetails.Detail);
        Assert.DoesNotContain(@"C:\", problemResult.ProblemDetails.Detail, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheUserIsNotAuthorized_ShouldReturnForbiddenProblemResult()
    {
        // Arrange
        _mockHandler.HandleAsync(Arg.Any<GetTrackStreamQuery>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationErrors.Authorization.NotAuthorized);
        SetRouteValues();

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemResult.StatusCode);
        Assert.Equal("General.Unauthorized", problemResult.ProblemDetails.Title);
        Assert.Equal("NotAuthorized", problemResult.ProblemDetails.Detail);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsValidationErrors_ShouldReturnValidationProblemResult()
    {
        // Arrange
        List<Error> errors = [DomainErrors.Library.LibraryIdCannotBeEmpty, DomainErrors.Music.TrackIdCannotBeEmpty];
        _mockHandler.HandleAsync(Arg.Any<GetTrackStreamQuery>(), Arg.Any<CancellationToken>())
            .Returns(errors);
        SetRouteValues();

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        HttpValidationProblemDetails validationProblemDetails = Assert.IsType<HttpValidationProblemDetails>(problemResult.ProblemDetails);
        Assert.Contains("LibraryIdCannotBeEmpty", validationProblemDetails.Errors["General.Validation"]);
        Assert.Contains("TrackIdCannotBeEmpty", validationProblemDetails.Errors["General.Validation"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendTheExactRouteValuesToTheHandler()
    {
        // Arrange
        string libraryId = "11111111-1111-1111-1111-111111111111";
        string artistId = "22222222-2222-2222-2222-222222222222";
        string albumId = "33333333-3333-3333-3333-333333333333";
        string trackId = "44444444-4444-4444-4444-444444444444";
        _mockHandler.HandleAsync(Arg.Any<GetTrackStreamQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_trackStreamDataDtoFixture.Create()));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId;
        _sut.HttpContext.Request.RouteValues["artistId"] = artistId;
        _sut.HttpContext.Request.RouteValues["albumId"] = albumId;
        _sut.HttpContext.Request.RouteValues["trackId"] = trackId;
        CancellationToken cancellationToken = CancellationToken.None;

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetTrackStreamQuery>(query =>
                query.LibraryId == libraryId &&
                query.ArtistId == artistId &&
                query.AlbumId == albumId &&
                query.TrackId == trackId),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreMissing_ShouldSendNullRouteValuesToTheHandler()
    {
        // Arrange
        _mockHandler.HandleAsync(Arg.Any<GetTrackStreamQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_trackStreamDataDtoFixture.Create()));
        _sut.HttpContext.Request.RouteValues.Clear();

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetTrackStreamQuery>(query =>
                query.LibraryId == null &&
                query.ArtistId == null &&
                query.AlbumId == null &&
                query.TrackId == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cancellationTokenSource = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetTrackStreamQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_trackStreamDataDtoFixture.Create());
            }, callInfo.Arg<CancellationToken>()));
        SetRouteValues();

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(EmptyRequest.Instance, cancellationTokenSource.Token);
        await operationStarted.Task;
        cancellationTokenSource.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }

    /// <summary>
    /// Sets the four route values the endpoint reads from the route.
    /// </summary>
    private void SetRouteValues()
    {
        _sut.HttpContext.Request.RouteValues["libraryId"] = Guid.NewGuid().ToString();
        _sut.HttpContext.Request.RouteValues["artistId"] = Guid.NewGuid().ToString();
        _sut.HttpContext.Request.RouteValues["albumId"] = Guid.NewGuid().ToString();
        _sut.HttpContext.Request.RouteValues["trackId"] = Guid.NewGuid().ToString();
    }
}
