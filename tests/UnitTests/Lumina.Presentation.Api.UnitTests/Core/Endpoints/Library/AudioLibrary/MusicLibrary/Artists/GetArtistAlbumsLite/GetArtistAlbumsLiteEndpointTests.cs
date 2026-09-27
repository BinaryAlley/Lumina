#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistAlbumsLite;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Contracts.Fixtures.Responses.Common;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.GetArtistAlbumsLite;
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
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.GetArtistAlbumsLite;

/// <summary>
/// Contains unit tests for the <see cref="GetArtistAlbumsLiteEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetArtistAlbumsLiteEndpointTests
{
    private readonly IQueryHandler<GetArtistAlbumsLiteQuery, Result<PaginatedResponse<AlbumLiteResponse>>> _mockHandler;
    private readonly GetArtistAlbumsLiteEndpoint _sut;
    private readonly GetArtistAlbumsLiteRequestFixture _getArtistAlbumsLiteRequestFixture = new();
    private readonly AlbumLiteResponseFixture _albumLiteResponseFixture = new();
    private readonly PaginatedResponseFixture<AlbumLiteResponse> _paginatedResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetArtistAlbumsLiteEndpointTests"/> class.
    /// </summary>
    public GetArtistAlbumsLiteEndpointTests()
    {
        _mockHandler = Substitute.For<IQueryHandler<GetArtistAlbumsLiteQuery, Result<PaginatedResponse<AlbumLiteResponse>>>>();
        _sut = FastEndpoints.Factory.Create<GetArtistAlbumsLiteEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithPaginatedAlbumLiteResponses()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        GetArtistAlbumsLiteRequest request = _getArtistAlbumsLiteRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        PaginatedResponse<AlbumLiteResponse> expectedResponse = CreatePaginatedResponse(albumCount: 2);
        _mockHandler.HandleAsync(Arg.Any<GetArtistAlbumsLiteQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedResponse));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["artistId"] = artistId.ToString();

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        Ok<PaginatedResponse<AlbumLiteResponse>> okResult = Assert.IsType<Ok<PaginatedResponse<AlbumLiteResponse>>>(result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotAuthorizedError_ShouldReturnProblemResult()
    {
        // Arrange
        GetArtistAlbumsLiteRequest request = _getArtistAlbumsLiteRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = ApplicationErrors.Authorization.NotAuthorized;
        _mockHandler.HandleAsync(Arg.Any<GetArtistAlbumsLiteQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        // Act
        IResult result = await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        ProblemDetails problemDetailsBody = Assert.IsType<ProblemDetails>(problemDetails.ProblemDetails);
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetailsBody.Status);
        Assert.Equal("General.Unauthorized", problemDetailsBody.Title);
        Assert.Equal("NotAuthorized", problemDetailsBody.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", problemDetailsBody.Type);
        Assert.NotNull(problemDetailsBody.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsValidationErrors_ShouldReturnValidationProblemResult()
    {
        // Arrange
        GetArtistAlbumsLiteRequest request = _getArtistAlbumsLiteRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Errors.Music.ArtistIdCannotBeEmpty;
        _mockHandler.HandleAsync(Arg.Any<GetArtistAlbumsLiteQuery>(), Arg.Any<CancellationToken>())
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
        Assert.Equal(new[] { "ArtistIdCannotBeEmpty" }, validationProblemDetails.Errors["General.Validation"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldSendGetArtistAlbumsLiteQueryToSender()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        GetArtistAlbumsLiteRequest request = _getArtistAlbumsLiteRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetArtistAlbumsLiteQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(CreatePaginatedResponse(albumCount: 0)));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["artistId"] = artistId.ToString();

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetArtistAlbumsLiteQuery>(query =>
                query.LibraryId == libraryId.ToString() &&
                query.ArtistId == artistId.ToString() &&
                query.PaginationData != null &&
                query.PaginationData.CurrentPage == request.CurrentPage &&
                query.PaginationData.PerPage == request.PerPage),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenPaginationIsMissing_ShouldSendQueryWithNullPaginationData()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        GetArtistAlbumsLiteRequest request = _getArtistAlbumsLiteRequestFixture.Create(includeCurrentPage: false, includePerPage: false);
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetArtistAlbumsLiteQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(CreatePaginatedResponse(albumCount: 0)));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["artistId"] = artistId.ToString();

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetArtistAlbumsLiteQuery>(query =>
                query.LibraryId == libraryId.ToString() &&
                query.ArtistId == artistId.ToString() &&
                query.PaginationData == null),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreNotParseable_ShouldSendQueryWithRawRouteValues()
    {
        // Arrange
        GetArtistAlbumsLiteRequest request = _getArtistAlbumsLiteRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetArtistAlbumsLiteQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(CreatePaginatedResponse(albumCount: 0)));
        _sut.HttpContext.Request.RouteValues["libraryId"] = "not-a-library-guid";
        _sut.HttpContext.Request.RouteValues["artistId"] = "not-an-artist-guid";

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetArtistAlbumsLiteQuery>(query =>
                query.LibraryId == "not-a-library-guid" &&
                query.ArtistId == "not-an-artist-guid"),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreMissing_ShouldSendQueryWithNullIds()
    {
        // Arrange
        GetArtistAlbumsLiteRequest request = _getArtistAlbumsLiteRequestFixture.Create();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<GetArtistAlbumsLiteQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(CreatePaginatedResponse(albumCount: 0)));
        _sut.HttpContext.Request.RouteValues.Remove("libraryId");
        _sut.HttpContext.Request.RouteValues.Remove("artistId");

        // Act
        await _sut.ExecuteAsync(request, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<GetArtistAlbumsLiteQuery>(query =>
                query.LibraryId == null &&
                query.ArtistId == null),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        GetArtistAlbumsLiteRequest request = _getArtistAlbumsLiteRequestFixture.Create();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<GetArtistAlbumsLiteQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(CreatePaginatedResponse(albumCount: 0));
            }, callInfo.Arg<CancellationToken>()));

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(request, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }

    /// <summary>
    /// Creates a <see cref="PaginatedResponse{AlbumLiteResponse}"/> with the specified number of albums.
    /// </summary>
    /// <param name="albumCount">The number of album responses to include in the data collection.</param>
    /// <returns>A configured paginated response instance.</returns>
    private PaginatedResponse<AlbumLiteResponse> CreatePaginatedResponse(int albumCount)
    {
        List<AlbumLiteResponse> albums = [];
        for (int index = 0; index < albumCount; index++)
            albums.Add(_albumLiteResponseFixture.Create());

        return _paginatedResponseFixture.Create(data: albums, currentPage: 1, perPage: 10, count: albumCount, numberOfPages: albumCount > 0 ? 1 : 0);
    }
}
