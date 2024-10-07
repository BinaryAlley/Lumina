#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Core.FileManagement.Drives.Queries.GetDrives;
using Lumina.Application.Core.FileManagement.Thumbnails.Queries.GetThumbnail;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Presentation.Api.Common.Http;
using Lumina.Presentation.Api.Common.Utilities;
using Lumina.Presentation.Api.Core.Controllers.FileManagement;
using Lumina.Presentation.Api.UnitTests.Core.Controllers.FileManagement.Fixtures;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Controllers.FileManagement;

/// <summary>
/// Contains unit tests for the <see cref="ThumbnailsController"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThumbnailsControllerTests
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly ISender _mockMediator;
    private readonly ThumbnailsController _sut;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="ThumbnailsControllerTests"/> class.
    /// </summary>
    public ThumbnailsControllerTests()
    {
        _mockMediator = Substitute.For<ISender>();
        _sut = new ThumbnailsController(_mockMediator);
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    [Fact]
    public async Task GetThumbnail_WhenCalled_ShouldReturnFileResultWithThumbnailResponse()
    {
        // Arrange
        string path = "/path/to/file.jpg";
        int quality = 80;
        CancellationToken cancellationToken = CancellationToken.None;
        ThumbnailResponse expectedResponse = ThumbnailResponseFixture.CreateThumbnailResponse();
        _mockMediator.Send(Arg.Any<GetThumbnailQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponse));

        // Act
        IActionResult result = await _sut.GetThumbnail(path, quality, cancellationToken);

        // Assert
        FileContentResult fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.FileContents.Should().BeEquivalentTo(expectedResponse.Bytes);
        fileResult.ContentType.Should().Be(MimeTypes.GetMimeType(expectedResponse.Type));
    }

    [Fact]
    public async Task GetThumbnail_WhenMediatorReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        string path = "/path/to/nonexistent/file.jpg";
        int quality = 80;
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.NotFound("Thumbnail.NotFound", "The requested thumbnail was not found.");
        _mockMediator.Send(Arg.Any<GetThumbnailQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);
        HttpContext mockHttpContext = Substitute.For<HttpContext>();
        Dictionary<object, object> mockItems = [];
        mockHttpContext.Items.Returns(mockItems!);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = mockHttpContext
        };
        ProblemDetailsFactory mockProblemDetailsFactory = Substitute.For<ProblemDetailsFactory>();
        mockProblemDetailsFactory.CreateProblemDetails(
            Arg.Any<HttpContext>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = ErrorType.NotFound.ToString(),
                Detail = "The requested thumbnail was not found."
            });
        _sut.ProblemDetailsFactory = mockProblemDetailsFactory;

        // Act
        IActionResult result = await _sut.GetThumbnail(path, quality, cancellationToken);

        // Assert
        ObjectResult objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        ProblemDetails problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Title.Should().Be(ErrorType.NotFound.ToString());
        problemDetails.Detail.Should().Be("The requested thumbnail was not found.");
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        mockItems.Should().ContainKey(HttpContextItemKeys.ERRORS);
        mockItems[HttpContextItemKeys.ERRORS].Should().BeEquivalentTo(new List<Error> { expectedError });
    }

    [Fact]
    public async Task GetThumbnail_WhenCalled_ShouldSendGetThumbnailQueryToMediator()
    {
        // Arrange
        string path = "/path/to/file.jpg";
        int quality = 80;
        CancellationToken cancellationToken = CancellationToken.None;
        _mockMediator.Send(Arg.Any<GetThumbnailQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(ThumbnailResponseFixture.CreateThumbnailResponse()));

        // Act
        await _sut.GetThumbnail(path, quality, cancellationToken);

        // Assert
        await _mockMediator.Received(1).Send(Arg.Is<GetThumbnailQuery>(q => q.Path == path && q.Quality == quality), Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task GetThumbnail_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        string path = "/path/to/file.jpg";
        int quality = 80;
        CancellationTokenSource cts = new();

        _mockMediator.Send(Arg.Any<GetThumbnailQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => new ValueTask<ErrorOr<ThumbnailResponse>>(Task.Run(async () =>
            {
                await Task.Delay(100, callInfo.Arg<CancellationToken>());
                return ErrorOrFactory.From(ThumbnailResponseFixture.CreateThumbnailResponse());
            }, callInfo.Arg<CancellationToken>())));

        // Act & Assert
        cts.CancelAfter(50);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _sut.GetThumbnail(path, quality, cts.Token));
    }
    #endregion
}
