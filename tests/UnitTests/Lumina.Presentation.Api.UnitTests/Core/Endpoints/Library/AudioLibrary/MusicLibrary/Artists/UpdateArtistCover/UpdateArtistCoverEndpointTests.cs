#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Commands.UpdateArtistCover;
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.UpdateArtistCover;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.UpdateArtistCover;

/// <summary>
/// Contains unit tests for the <see cref="UpdateArtistCoverEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateArtistCoverEndpointTests
{
    private readonly ICommandHandler<UpdateArtistCoverCommand, Result<UpdateArtistCoverResponse>> _mockHandler;
    private readonly UpdateArtistCoverEndpoint _sut;
    private readonly UpdateArtistCoverResponseFixture _updateArtistCoverResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateArtistCoverEndpointTests"/> class.
    /// </summary>
    public UpdateArtistCoverEndpointTests()
    {
        _mockHandler = Substitute.For<ICommandHandler<UpdateArtistCoverCommand, Result<UpdateArtistCoverResponse>>>();
        _sut = FastEndpoints.Factory.Create<UpdateArtistCoverEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithCoverPath()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        string expectedPath = "/media/music/artist-cover.jpg";
        _mockHandler.HandleAsync(Arg.Any<UpdateArtistCoverCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_updateArtistCoverResponseFixture.Create(coverPath: expectedPath)));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["artistId"] = artistId.ToString();
        ConfigureForm([1, 2, 3], "cover.jpg");

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        Ok<UpdateArtistCoverResponse> okResult = Assert.IsType<Ok<UpdateArtistCoverResponse>>(result);
        Assert.Equal(expectedPath, okResult?.Value?.CoverPath);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithValidRouteIdsAndFile_ShouldSendUpdateArtistCoverCommandToSender()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<UpdateArtistCoverCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_updateArtistCoverResponseFixture.Create(coverPath: "/media/music/artist-cover.jpg")));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["artistId"] = artistId.ToString();
        ConfigureForm([1, 2, 3], "cover.jpg");

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<UpdateArtistCoverCommand>(command =>
                command.LibraryId == libraryId.ToString() &&
                command.ArtistId == artistId.ToString() &&
                command.Cover != null &&
                command.FileName == "cover.jpg"),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreNotParseable_ShouldSendCommandWithRawRouteValues()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<UpdateArtistCoverCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_updateArtistCoverResponseFixture.Create(coverPath: "/media/music/artist-cover.jpg")));
        _sut.HttpContext.Request.RouteValues["libraryId"] = "not-a-library-guid";
        _sut.HttpContext.Request.RouteValues["artistId"] = "not-an-artist-guid";
        ConfigureForm([1, 2, 3], "cover.jpg");

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<UpdateArtistCoverCommand>(command =>
                command.LibraryId == "not-a-library-guid" &&
                command.ArtistId == "not-an-artist-guid"),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreMissing_ShouldSendCommandWithNullIds()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<UpdateArtistCoverCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_updateArtistCoverResponseFixture.Create(coverPath: "/media/music/artist-cover.jpg")));
        _sut.HttpContext.Request.RouteValues.Remove("libraryId");
        _sut.HttpContext.Request.RouteValues.Remove("artistId");
        ConfigureForm([1, 2, 3], "cover.jpg");

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<UpdateArtistCoverCommand>(command =>
                command.LibraryId == null &&
                command.ArtistId == null),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenFormHasNoFiles_ShouldSendCommandWithNullCover()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<UpdateArtistCoverCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_updateArtistCoverResponseFixture.Create(coverPath: "/media/music/artist-cover.jpg")));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["artistId"] = artistId.ToString();
        IFormCollection form = new FormCollection([]);
        _sut.HttpContext.Request.ContentType = "multipart/form-data; boundary=----test";
        _sut.HttpContext.Features.Set<IFormFeature>(new FormFeature(form));

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        Assert.IsType<Ok<UpdateArtistCoverResponse>>(result);
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<UpdateArtistCoverCommand>(command =>
                command.ArtistId == artistId.ToString() &&
                command.Cover == null &&
                command.FileName == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRequestIsNotMultipart_ShouldSendCommandWithNullCover()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<UpdateArtistCoverCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_updateArtistCoverResponseFixture.Create(coverPath: "/media/music/artist-cover.jpg")));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["artistId"] = artistId.ToString();
        _sut.HttpContext.Request.ContentType = "application/json";

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        Assert.IsType<Ok<UpdateArtistCoverResponse>>(result);
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<UpdateArtistCoverCommand>(command => command.Cover == null && command.FileName == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheFormIsMalformed_ShouldSendCommandWithNullCover()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<UpdateArtistCoverCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_updateArtistCoverResponseFixture.Create(coverPath: "/media/music/artist-cover.jpg")));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["artistId"] = artistId.ToString();
        ConfigureMalformedForm();

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        Assert.IsType<Ok<UpdateArtistCoverResponse>>(result);
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<UpdateArtistCoverCommand>(command => command.Cover == null && command.FileName == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsValidationErrors_ShouldReturnValidationProblemResult()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        Error validationError = Errors.Music.CoverCannotBeNull;
        _mockHandler.HandleAsync(Arg.Any<UpdateArtistCoverCommand>(), Arg.Any<CancellationToken>())
            .Returns(validationError);
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["artistId"] = artistId.ToString();
        ConfigureForm([1, 2, 3], "cover.jpg");

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

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
        Assert.Equal(new[] { "CoverCannotBeNull" }, validationProblemDetails.Errors["General.Validation"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotFoundError_ShouldReturnProblemResult()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Errors.Music.ArtistNotFound;
        _mockHandler.HandleAsync(Arg.Any<UpdateArtistCoverCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["artistId"] = artistId.ToString();
        ConfigureForm([1, 2, 3], "cover.jpg");

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        ProblemDetails problemDetailsBody = Assert.IsType<ProblemDetails>(problemDetails.ProblemDetails);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetailsBody.Status);
        Assert.Equal("General.NotFound", problemDetailsBody.Title);
        Assert.Equal("ArtistNotFound", problemDetailsBody.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problemDetailsBody.Type);
        Assert.NotNull(problemDetailsBody.Extensions["traceId"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        CancellationTokenSource cts = new();
        TaskCompletionSource<bool> operationStarted = new();
        TaskCompletionSource<bool> cancellationRequested = new();

        _mockHandler.HandleAsync(Arg.Any<UpdateArtistCoverCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.Run(async () =>
            {
                operationStarted.SetResult(true);
                await cancellationRequested.Task;
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Result.From(_updateArtistCoverResponseFixture.Create(coverPath: "/media/music/artist-cover.jpg"));
            }, callInfo.Arg<CancellationToken>()));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["artistId"] = artistId.ToString();
        ConfigureForm([1, 2, 3], "cover.jpg");

        // Act
        Task<IResult> operationTask = _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cts.Token);
        await operationStarted.Task;
        cts.Cancel();
        cancellationRequested.SetResult(true);

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operationTask);
    }

    private void ConfigureForm(byte[] content, string fileName)
    {
        MemoryStream coverStream = new(content);
        IFormFile formFile = new FormFile(coverStream, 0, content.Length, "cover", fileName);
        FormFileCollection files = [formFile];
        IFormCollection form = new FormCollection([], files);
        _sut.HttpContext.Request.ContentType = "multipart/form-data; boundary=----test";
        _sut.HttpContext.Features.Set<IFormFeature>(new FormFeature(form));
    }

    /// <summary>
    /// Configures a multipart form request whose body cannot be parsed.
    /// </summary>
    private void ConfigureMalformedForm()
    {
        _sut.HttpContext.Request.ContentType = "multipart/form-data; boundary=----test";
        IFormCollection malformedForm = Substitute.For<IFormCollection>();
        malformedForm.Files.Returns(_ => throw new InvalidDataException("Malformed multipart body."));
        _sut.HttpContext.Features.Set<IFormFeature>(new FormFeature(malformedForm));
    }
}
