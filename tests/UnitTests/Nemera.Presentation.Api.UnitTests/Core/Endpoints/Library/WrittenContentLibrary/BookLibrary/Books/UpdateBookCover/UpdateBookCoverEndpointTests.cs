#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBookCover;
using Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.UpdateBookCover;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.UpdateBookCover;

/// <summary>
/// Contains unit tests for the <see cref="UpdateBookCoverEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateBookCoverEndpointTests
{
    private readonly ICommandHandler<UpdateBookCoverCommand, Result<UpdateBookCoverResponse>> _mockHandler;
    private readonly UpdateBookCoverEndpoint _sut;
    private readonly UpdateBookCoverResponseFixture _updateBookCoverResponseFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBookCoverEndpointTests"/> class.
    /// </summary>
    public UpdateBookCoverEndpointTests()
    {
        _mockHandler = Substitute.For<ICommandHandler<UpdateBookCoverCommand, Result<UpdateBookCoverResponse>>>();
        _sut = FastEndpoints.Factory.Create<UpdateBookCoverEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithCoverPath()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        string expectedPath = "/media/books/cover.jpg";
        _mockHandler.HandleAsync(Arg.Any<UpdateBookCoverCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_updateBookCoverResponseFixture.Create(coverPath: expectedPath)));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["bookId"] = bookId.ToString();
        ConfigureForm([1, 2, 3], "cover.jpg");

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        Ok<UpdateBookCoverResponse> okResult = Assert.IsType<Ok<UpdateBookCoverResponse>>(result);
        Assert.Equal(expectedPath, okResult.Value.CoverPath);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithValidRouteIdsAndFile_ShouldSendUpdateBookCoverCommandToSender()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<UpdateBookCoverCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_updateBookCoverResponseFixture.Create(coverPath: "/media/books/cover.jpg")));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["bookId"] = bookId.ToString();
        ConfigureForm([1, 2, 3], "cover.jpg");

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<UpdateBookCoverCommand>(command =>
                command.LibraryId == libraryId.ToString() &&
                command.BookId == bookId.ToString() &&
                command.Cover != null &&
                command.FileName == "cover.jpg"),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreNotParseable_ShouldSendCommandWithRawRouteValues()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<UpdateBookCoverCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_updateBookCoverResponseFixture.Create(coverPath: "/media/books/cover.jpg")));
        _sut.HttpContext.Request.RouteValues["libraryId"] = "not-a-library-guid";
        _sut.HttpContext.Request.RouteValues["bookId"] = "not-a-guid";
        ConfigureForm([1, 2, 3], "cover.jpg");

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<UpdateBookCoverCommand>(command => command.LibraryId == "not-a-library-guid" && command.BookId == "not-a-guid"),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteValuesAreMissing_ShouldSendCommandWithNullIds()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<UpdateBookCoverCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_updateBookCoverResponseFixture.Create(coverPath: "/media/books/cover.jpg")));
        _sut.HttpContext.Request.RouteValues.Remove("libraryId");
        _sut.HttpContext.Request.RouteValues.Remove("bookId");
        ConfigureForm([1, 2, 3], "cover.jpg");

        // Act
        await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<UpdateBookCoverCommand>(command => command.LibraryId == null && command.BookId == null),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenFormHasNoFiles_ShouldSendCommandWithNullCover()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<UpdateBookCoverCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(_updateBookCoverResponseFixture.Create(coverPath: "/media/books/cover.jpg")));
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["bookId"] = bookId.ToString();
        IFormCollection form = new FormCollection([]);
        _sut.HttpContext.Request.ContentType = "multipart/form-data; boundary=----test";
        _sut.HttpContext.Features.Set<IFormFeature>(new FormFeature(form));

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        Assert.IsType<Ok<UpdateBookCoverResponse>>(result);
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<UpdateBookCoverCommand>(command => command.BookId == bookId.ToString() && command.Cover == null && command.FileName == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsValidationErrors_ShouldReturnValidationProblemResult()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        Error validationError = Errors.WrittenContent.BookIdCannotBeEmpty;
        _mockHandler.HandleAsync(Arg.Any<UpdateBookCoverCommand>(), Arg.Any<CancellationToken>())
            .Returns(validationError);
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["bookId"] = bookId.ToString();
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
        Assert.Equal(new[] { "BookIdCannotBeEmpty" }, validationProblemDetails.Errors["General.Validation"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsNotFoundError_ShouldReturnProblemResult()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.NotFound("Book.NotFound", "BookNotFound");
        _mockHandler.HandleAsync(Arg.Any<UpdateBookCoverCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);
        _sut.HttpContext.Request.RouteValues["libraryId"] = libraryId.ToString();
        _sut.HttpContext.Request.RouteValues["bookId"] = bookId.ToString();
        ConfigureForm([1, 2, 3], "cover.jpg");

        // Act
        IResult result = await _sut.ExecuteAsync(FastEndpoints.EmptyRequest.Instance, cancellationToken);

        // Assert
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        Microsoft.AspNetCore.Mvc.ProblemDetails problemDetailsBody = Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(problemDetails.ProblemDetails);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetailsBody.Status);
        Assert.Equal("Book.NotFound", problemDetailsBody.Title);
        Assert.Equal("BookNotFound", problemDetailsBody.Detail);
        Assert.NotNull(problemDetailsBody.Extensions["traceId"]);
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
}
