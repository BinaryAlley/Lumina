#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBookCover;
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
    private readonly Application.Common.CQRS.ICommandHandler<UpdateBookCoverCommand, Result<string>> _mockHandler;
    private readonly UpdateBookCoverEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBookCoverEndpointTests"/> class.
    /// </summary>
    public UpdateBookCoverEndpointTests()
    {
        _mockHandler = Substitute.For<Application.Common.CQRS.ICommandHandler<UpdateBookCoverCommand, Result<string>>>();
        _sut = Factory.Create<UpdateBookCoverEndpoint>(_mockHandler);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ShouldReturnOkResultWithCoverPath()
    {
        // Arrange
        Guid bookId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        string expectedPath = "/media/books/cover.jpg";
        _mockHandler.HandleAsync(Arg.Any<UpdateBookCoverCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From(expectedPath));
        ConfigureRequest(bookId, [1, 2, 3], "cover.jpg");

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        Ok<string> okResult = Assert.IsType<Ok<string>>(result);
        Assert.Equal(expectedPath, okResult.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithValidRouteIdAndFile_ShouldSendUpdateBookCoverCommand()
    {
        // Arrange
        Guid bookId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<UpdateBookCoverCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From("/media/books/cover.jpg"));
        ConfigureRequest(bookId, [1, 2, 3], "cover.jpg");

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<UpdateBookCoverCommand>(command =>
                command.BookId == bookId &&
                command.Cover != null &&
                command.FileName == "cover.jpg"),
            Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteIdIsNotParseable_ShouldSendCommandWithEmptyBookId()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<UpdateBookCoverCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From("/media/books/cover.jpg"));
        ConfigureRequest("not-a-guid", [1, 2, 3], "cover.jpg");

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<UpdateBookCoverCommand>(command => command.BookId == Guid.Empty && command.Cover != null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenFormHasNoFiles_ShouldSendCommandWithNullCover()
    {
        // Arrange
        Guid bookId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        _mockHandler.HandleAsync(Arg.Any<UpdateBookCoverCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.From("/media/books/cover.jpg"));
        ConfigureRequestWithoutFiles(bookId);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

        // Assert
        Assert.IsType<Ok<string>>(result);
        await _mockHandler.Received(1).HandleAsync(
            Arg.Is<UpdateBookCoverCommand>(command => command.BookId == bookId && command.Cover == null && command.FileName == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        Guid bookId = Guid.NewGuid();
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.NotFound("Book.NotFound", "BookNotFound");
        _mockHandler.HandleAsync(Arg.Any<UpdateBookCoverCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);
        ConfigureRequest(bookId, [1, 2, 3], "cover.jpg");

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, cancellationToken);

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

    private void ConfigureRequest(Guid bookId, byte[] content, string fileName)
    {
        ConfigureRequest(bookId.ToString(), content, fileName);
    }

    private void ConfigureRequest(string bookId, byte[] content, string fileName)
    {
        _sut.HttpContext.Request.RouteValues["id"] = bookId;
        MemoryStream coverStream = new(content);
        IFormFile formFile = new FormFile(coverStream, 0, content.Length, "cover", fileName);
        FormFileCollection files = [formFile];
        IFormCollection form = new FormCollection([], files);
        _sut.HttpContext.Request.ContentType = "multipart/form-data; boundary=----test";
        _sut.HttpContext.Features.Set<IFormFeature>(new FormFeature(form));
    }

    private void ConfigureRequestWithoutFiles(Guid bookId)
    {
        _sut.HttpContext.Request.RouteValues["id"] = bookId.ToString();
        IFormCollection form = new FormCollection([]);
        _sut.HttpContext.Request.ContentType = "multipart/form-data; boundary=----test";
        _sut.HttpContext.Features.Set<IFormFeature>(new FormFeature(form));
    }
}
