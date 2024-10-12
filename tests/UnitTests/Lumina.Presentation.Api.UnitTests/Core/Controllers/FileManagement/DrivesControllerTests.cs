#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Core.FileManagement.Drives.Queries.GetDrives;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Presentation.Api.Common.Http;
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
/// Contains unit tests for the <see cref="DrivesController"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DrivesControllerTests
{
    private readonly ISender _mockMediator;
    private readonly DrivesController _sut;
    private readonly FileSystemTreeNodeResponseFixture _fileSystemTreeNodeResponseFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="DrivesControllerTests"/> class.
    /// </summary>
    public DrivesControllerTests()
    {
        _mockMediator = Substitute.For<ISender>();
        _sut = new DrivesController(_mockMediator);
        _fileSystemTreeNodeResponseFixture = new FileSystemTreeNodeResponseFixture();
    }

    [Fact]
    public async Task GetDrives_WhenCalled_ShouldReturnOkResultWithFileSystemTreeNodeResponses()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        List<FileSystemTreeNodeResponse> expectedResponses = _fileSystemTreeNodeResponseFixture.CreateMany(3, 1, 1);
        _mockMediator.Send(Arg.Any<GetDrivesQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponses.AsEnumerable()));

        // Act
        IActionResult result = await _sut.GetDrives(cancellationToken);

        // Assert
        OkObjectResult okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        IEnumerable<FileSystemTreeNodeResponse> actualResponses = okResult.Value.Should().BeAssignableTo<IEnumerable<FileSystemTreeNodeResponse>>().Subject;
        actualResponses.Should().BeEquivalentTo(expectedResponses);
    }

    [Fact]
    public async Task GetDrives_WhenMediatorReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.NotFound("Drive.NotFound", "The requested drive was not found.");
        _mockMediator.Send(Arg.Any<GetDrivesQuery>(), Arg.Any<CancellationToken>())
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
                Detail = "The requested drive was not found."
            });
        _sut.ProblemDetailsFactory = mockProblemDetailsFactory;

        // Act
        IActionResult result = await _sut.GetDrives(cancellationToken);

        // Assert
        ObjectResult objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        ProblemDetails problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Title.Should().Be(ErrorType.NotFound.ToString());
        problemDetails.Detail.Should().Be("The requested drive was not found.");
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        mockItems.Should().ContainKey(HttpContextItemKeys.ERRORS);
        mockItems[HttpContextItemKeys.ERRORS].Should().BeEquivalentTo(new List<Error> { expectedError });
    }

    [Fact]
    public async Task GetDrives_WhenCalled_ShouldSendGetDrivesQueryToMediator()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockMediator.Send(Arg.Any<GetDrivesQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(Enumerable.Empty<FileSystemTreeNodeResponse>()));

        // Act
        await _sut.GetDrives(cancellationToken);

        // Assert
        await _mockMediator.Received(1).Send(Arg.Any<GetDrivesQuery>(), Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task GetDrives_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();

        _mockMediator.Send(Arg.Any<GetDrivesQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => new ValueTask<ErrorOr<IEnumerable<FileSystemTreeNodeResponse>>>(Task.Run(async () =>
            {
                await Task.Delay(100, callInfo.Arg<CancellationToken>());
                return ErrorOrFactory.From(_fileSystemTreeNodeResponseFixture.CreateMany(3, 1, 1).AsEnumerable());
            }, callInfo.Arg<CancellationToken>())));

        // Act & Assert
        cts.CancelAfter(50);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _sut.GetDrives(cts.Token));
    }
}
