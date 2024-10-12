#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Core.FileManagement.Paths.Commands.CombinePath;
using Lumina.Application.Core.FileManagement.Paths.Commands.SplitPath;
using Lumina.Application.Core.FileManagement.Paths.Queries.CheckPathExists;
using Lumina.Application.Core.FileManagement.Paths.Queries.GetPathParent;
using Lumina.Application.Core.FileManagement.Paths.Queries.GetPathRoot;
using Lumina.Application.Core.FileManagement.Paths.Queries.GetPathSeparator;
using Lumina.Application.Core.FileManagement.Paths.Queries.ValidatePath;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Presentation.Api.Core.Controllers.FileManagement;
using Lumina.Presentation.Api.UnitTests.Core.Controllers.FileManagement.Fixtures;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
/// Contains unit tests for the <see cref="PathController"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PathControllerTests
{
    private readonly ISender _mockMediator;
    private readonly PathController _sut;
    private readonly PathSegmentResponseFixture _pathSegmentResponseFixture;
    private readonly PathSeparatorResponseFixture _pathSeparatorResponseFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="PathControllerTests"/> class.
    /// </summary>
    public PathControllerTests()
    {
        _mockMediator = Substitute.For<ISender>();
        _sut = new PathController(_mockMediator);
        _pathSegmentResponseFixture = new PathSegmentResponseFixture();
        _pathSeparatorResponseFixture = new PathSeparatorResponseFixture();
    }

    [Fact]
    public async Task GetPathRoot_WhenCalled_ShouldReturnOkResultWithPathSegmentResponse()
    {
        // Arrange
        string path = @"C:\Users\TestUser";
        CancellationToken cancellationToken = CancellationToken.None;
        PathSegmentResponse expectedResponse = _pathSegmentResponseFixture.Create();
        _mockMediator.Send(Arg.Any<GetPathRootQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponse));

        // Act
        IActionResult result = await _sut.GetPathRoot(path, cancellationToken);

        // Assert
        OkObjectResult okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        PathSegmentResponse actualResponse = okResult.Value.Should().BeAssignableTo<PathSegmentResponse>().Subject;
        actualResponse.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetPathRoot_WhenMediatorReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        string path = @"InvalidPath";
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Validation("Path.Invalid", "The provided path is invalid.");
        _mockMediator.Send(Arg.Any<GetPathRootQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        _sut.ControllerContext = new ControllerContext();

        ProblemDetailsFactory mockProblemDetailsFactory = Substitute.For<ProblemDetailsFactory>();
        mockProblemDetailsFactory.CreateProblemDetails(
            Arg.Any<HttpContext>(),
            Arg.Any<int?>(),
            Arg.Is<string>(ErrorType.Validation.ToString()),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = ErrorType.Validation.ToString(),
                Detail = "The provided path is invalid."
            });
        _sut.ProblemDetailsFactory = mockProblemDetailsFactory;
        ValidationProblemDetails expectedValidationProblemDetails = new()
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
            Errors =
            {
                { "Path.Invalid", new[] { "The provided path is invalid." } }
            }
        };

        mockProblemDetailsFactory.CreateValidationProblemDetails(
            Arg.Any<HttpContext>(),
            Arg.Any<ModelStateDictionary>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(expectedValidationProblemDetails);

        // Act
        IActionResult result = await _sut.GetPathRoot(path, cancellationToken);

        // Assert
        BadRequestObjectResult badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        ValidationProblemDetails actualValidationProblemDetails = badRequestResult.Value.Should().BeOfType<ValidationProblemDetails>().Subject;
        actualValidationProblemDetails.Should().BeEquivalentTo(expectedValidationProblemDetails);
    }

    [Fact]
    public async Task GetPathRoot_WhenCalled_ShouldSendGetPathRootQueryToMediator()
    {
        // Arrange
        string path = @"C:\Users\TestUser";
        CancellationToken cancellationToken = CancellationToken.None;
        _mockMediator.Send(Arg.Any<GetPathRootQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(_pathSegmentResponseFixture.Create()));

        // Act
        await _sut.GetPathRoot(path, cancellationToken);

        // Assert
        await _mockMediator.Received(1).Send(Arg.Is<GetPathRootQuery>(q => q.Path == path), Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task GetPathRoot_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        string path = @"C:\Users\TestUser";
        CancellationTokenSource cts = new();

        _mockMediator.Send(Arg.Any<GetPathRootQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => new ValueTask<ErrorOr<PathSegmentResponse>>(Task.Run(async () =>
            {
                await Task.Delay(100, callInfo.Arg<CancellationToken>());
                return ErrorOrFactory.From(_pathSegmentResponseFixture.Create());
            }, callInfo.Arg<CancellationToken>())));

        // Act & Assert
        cts.CancelAfter(50);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _sut.GetPathRoot(path, cts.Token));
    }

    [Fact]
    public async Task GetPathSeparator_WhenCalled_ShouldReturnOkResultWithPathSeparatorResponse()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        PathSeparatorResponse expectedResponse = _pathSeparatorResponseFixture.Create();
        _mockMediator.Send(Arg.Any<GetPathSeparatorQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        IActionResult result = await _sut.GetPathSeparator(cancellationToken);

        // Assert
        OkObjectResult okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        PathSeparatorResponse actualResponse = okResult.Value.Should().BeAssignableTo<PathSeparatorResponse>().Subject;
        actualResponse.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetPathSeparator_WhenCalled_ShouldSendGetPathSeparatorQueryToMediator()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        _mockMediator.Send(Arg.Any<GetPathSeparatorQuery>(), Arg.Any<CancellationToken>())
            .Returns(_pathSeparatorResponseFixture.Create());

        // Act
        await _sut.GetPathSeparator(cancellationToken);

        // Assert
        await _mockMediator.Received(1).Send(Arg.Any<GetPathSeparatorQuery>(), Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task GetPathSeparator_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        CancellationTokenSource cts = new();

        _mockMediator.Send(Arg.Any<GetPathSeparatorQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => new ValueTask<PathSeparatorResponse>(Task.Run(async () =>
            {
                await Task.Delay(100, callInfo.Arg<CancellationToken>());
                return _pathSeparatorResponseFixture.Create();
            }, callInfo.Arg<CancellationToken>())));

        // Act & Assert
        cts.CancelAfter(50);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _sut.GetPathSeparator(cts.Token));
    }

    [Fact]
    public async Task GetPathParent_WhenCalled_ShouldReturnOkResultWithPathSegmentResponses()
    {
        // Arrange
        string path = @"C:\Users\TestUser\Documents";
        CancellationToken cancellationToken = CancellationToken.None;
        IEnumerable<PathSegmentResponse> expectedResponse =
        [
            _pathSegmentResponseFixture.Create(),
            _pathSegmentResponseFixture.Create()
        ];
        _mockMediator.Send(Arg.Any<GetPathParentQuery>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponse));

        // Act
        IActionResult result = await _sut.GetPathParent(path, cancellationToken);

        // Assert
        OkObjectResult okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        IEnumerable<PathSegmentResponse> actualResponse = okResult.Value.Should().BeAssignableTo<IEnumerable<PathSegmentResponse>>().Subject;
        actualResponse.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetPathParent_WhenMediatorReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        string path = @"InvalidPath";
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Validation("Path.Invalid", "The provided path is invalid.");
        _mockMediator.Send(Arg.Any<GetPathParentQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        _sut.ControllerContext = new ControllerContext();

        ProblemDetailsFactory mockProblemDetailsFactory = Substitute.For<ProblemDetailsFactory>();
        mockProblemDetailsFactory.CreateProblemDetails(
            Arg.Any<HttpContext>(),
            Arg.Any<int?>(),
            Arg.Is<string>(ErrorType.Validation.ToString()),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = ErrorType.Validation.ToString(),
                Detail = "The provided path is invalid."
            });
        _sut.ProblemDetailsFactory = mockProblemDetailsFactory;
        ValidationProblemDetails expectedValidationProblemDetails = new()
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
            Errors =
            {
                { "Path.Invalid", new[] { "The provided path is invalid." } }
            }
        };
        mockProblemDetailsFactory.CreateValidationProblemDetails(
            Arg.Any<HttpContext>(),
            Arg.Any<ModelStateDictionary>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(expectedValidationProblemDetails);

        // Act
        IActionResult result = await _sut.GetPathParent(path, cancellationToken);

        // Assert
        BadRequestObjectResult badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        ValidationProblemDetails actualValidationProblemDetails = badRequestResult.Value.Should().BeOfType<ValidationProblemDetails>().Subject;
        actualValidationProblemDetails.Should().BeEquivalentTo(expectedValidationProblemDetails);
    }

    [Fact]
    public async Task GetPathParent_WhenCalled_ShouldSendGetPathParentQueryToMediator()
    {
        // Arrange
        string path = @"C:\Users\TestUser\Documents";
        CancellationToken cancellationToken = CancellationToken.None;
        _mockMediator.Send(Arg.Any<GetPathParentQuery>(), Arg.Any<CancellationToken>())
            .Returns(new List<PathSegmentResponse> { _pathSegmentResponseFixture.Create() });

        // Act
        await _sut.GetPathParent(path, cancellationToken);

        // Assert
        await _mockMediator.Received(1).Send(Arg.Is<GetPathParentQuery>(q => q.Path == path), Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task GetPathParent_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        string path = @"C:\Users\TestUser\Documents";
        CancellationTokenSource cts = new();

        _mockMediator.Send(Arg.Any<GetPathParentQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => new ValueTask<ErrorOr<IEnumerable<PathSegmentResponse>>>(Task.Run(async () =>
            {
                await Task.Delay(100, callInfo.Arg<CancellationToken>());
                return ErrorOrFactory.From(new List<PathSegmentResponse>
                {
                _pathSegmentResponseFixture.Create()
                }.AsEnumerable());
            }, callInfo.Arg<CancellationToken>())));

        // Act & Assert
        cts.CancelAfter(50);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _sut.GetPathParent(path, cts.Token));
    }

    [Fact]
    public async Task CombinePath_WhenCalled_ShouldReturnOkResultWithPathSegmentResponse()
    {
        // Arrange
        string originalPath = @"C:\Users";
        string newPath = "TestUser";
        CancellationToken cancellationToken = CancellationToken.None;
        PathSegmentResponse expectedResponse = _pathSegmentResponseFixture.Create();
        _mockMediator.Send(Arg.Any<CombinePathCommand>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponse));

        // Act
        IActionResult result = await _sut.CombinePath(originalPath, newPath, cancellationToken);

        // Assert
        OkObjectResult okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        PathSegmentResponse actualResponse = okResult.Value.Should().BeAssignableTo<PathSegmentResponse>().Subject;
        actualResponse.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task CombinePath_WhenMediatorReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        string originalPath = @"InvalidPath";
        string newPath = "TestUser";
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Validation("Path.Invalid", "The provided path is invalid.");
        _mockMediator.Send(Arg.Any<CombinePathCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        _sut.ControllerContext = new ControllerContext();

        ProblemDetailsFactory mockProblemDetailsFactory = Substitute.For<ProblemDetailsFactory>();
        mockProblemDetailsFactory.CreateProblemDetails(
            Arg.Any<HttpContext>(),
            Arg.Any<int?>(),
            Arg.Is<string>(ErrorType.Validation.ToString()),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = ErrorType.Validation.ToString(),
                Detail = "The provided path is invalid."
            });
        _sut.ProblemDetailsFactory = mockProblemDetailsFactory;
        ValidationProblemDetails expectedValidationProblemDetails = new()
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
            Errors =
            {
                { "Path.Invalid", new[] { "The provided path is invalid." } }
            }
        };
        mockProblemDetailsFactory.CreateValidationProblemDetails(
            Arg.Any<HttpContext>(),
            Arg.Any<ModelStateDictionary>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(expectedValidationProblemDetails);

        // Act
        IActionResult result = await _sut.CombinePath(originalPath, newPath, cancellationToken);

        // Assert
        BadRequestObjectResult badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        ValidationProblemDetails actualValidationProblemDetails = badRequestResult.Value.Should().BeOfType<ValidationProblemDetails>().Subject;
        actualValidationProblemDetails.Should().BeEquivalentTo(expectedValidationProblemDetails);
    }

    [Fact]
    public async Task CombinePath_WhenCalled_ShouldSendCombinePathCommandToMediator()
    {
        // Arrange
        string originalPath = @"C:\Users";
        string newPath = "TestUser";
        CancellationToken cancellationToken = CancellationToken.None;
        _mockMediator.Send(Arg.Any<CombinePathCommand>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(_pathSegmentResponseFixture.Create()));

        // Act
        await _sut.CombinePath(originalPath, newPath, cancellationToken);

        // Assert
        await _mockMediator.Received(1).Send(Arg.Is<CombinePathCommand>(c => c.OriginalPath == originalPath && c.NewPath == newPath), Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task CombinePath_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        string originalPath = @"C:\Users";
        string newPath = "TestUser";
        CancellationTokenSource cts = new();

        _mockMediator.Send(Arg.Any<CombinePathCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => new ValueTask<ErrorOr<PathSegmentResponse>>(Task.Run(async () =>
            {
                await Task.Delay(100, callInfo.Arg<CancellationToken>());
                return ErrorOrFactory.From(_pathSegmentResponseFixture.Create());
            }, callInfo.Arg<CancellationToken>())));

        // Act & Assert
        cts.CancelAfter(50);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _sut.CombinePath(originalPath, newPath, cts.Token));
    }

    [Fact]
    public async Task SplitPath_WhenCalled_ShouldReturnOkResultWithPathSegmentResponses()
    {
        // Arrange
        string path = @"C:\Users\TestUser\Documents";
        CancellationToken cancellationToken = CancellationToken.None;
        IEnumerable<PathSegmentResponse> expectedResponse =
        [
            _pathSegmentResponseFixture.Create(),
            _pathSegmentResponseFixture.Create(),
            _pathSegmentResponseFixture.Create()
        ];
        _mockMediator.Send(Arg.Any<SplitPathCommand>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOrFactory.From(expectedResponse));

        // Act
        IActionResult result = await _sut.SplitPath(path, cancellationToken);

        // Assert
        OkObjectResult okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        IEnumerable<PathSegmentResponse> actualResponse = okResult.Value.Should().BeAssignableTo<IEnumerable<PathSegmentResponse>>().Subject;
        actualResponse.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task SplitPath_WhenMediatorReturnsError_ShouldReturnProblemResult()
    {
        // Arrange
        string path = @"InvalidPath";
        CancellationToken cancellationToken = CancellationToken.None;
        Error expectedError = Error.Validation("Path.Invalid", "The provided path is invalid.");
        _mockMediator.Send(Arg.Any<SplitPathCommand>(), Arg.Any<CancellationToken>())
            .Returns(expectedError);

        _sut.ControllerContext = new ControllerContext();

        ProblemDetailsFactory mockProblemDetailsFactory = Substitute.For<ProblemDetailsFactory>();
        mockProblemDetailsFactory.CreateProblemDetails(
            Arg.Any<HttpContext>(),
            Arg.Any<int?>(),
            Arg.Is<string>(ErrorType.Validation.ToString()),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = ErrorType.Validation.ToString(),
                Detail = "The provided path is invalid."
            });
        _sut.ProblemDetailsFactory = mockProblemDetailsFactory;
        ValidationProblemDetails expectedValidationProblemDetails = new()
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
            Errors =
        {
            { "Path.Invalid", new[] { "The provided path is invalid." } }
        }
        };
        mockProblemDetailsFactory.CreateValidationProblemDetails(
            Arg.Any<HttpContext>(),
            Arg.Any<ModelStateDictionary>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(expectedValidationProblemDetails);

        // Act
        IActionResult result = await _sut.SplitPath(path, cancellationToken);

        // Assert
        BadRequestObjectResult badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        ValidationProblemDetails actualValidationProblemDetails = badRequestResult.Value.Should().BeOfType<ValidationProblemDetails>().Subject;
        actualValidationProblemDetails.Should().BeEquivalentTo(expectedValidationProblemDetails);
    }

    [Fact]
    public async Task SplitPath_WhenCalled_ShouldSendSplitPathCommandToMediator()
    {
        // Arrange
        string path = @"C:\Users\TestUser\Documents";
        CancellationToken cancellationToken = CancellationToken.None;
        _mockMediator.Send(Arg.Any<SplitPathCommand>(), Arg.Any<CancellationToken>())
            .Returns(new List<PathSegmentResponse> { _pathSegmentResponseFixture.Create() });

        // Act
        await _sut.SplitPath(path, cancellationToken);

        // Assert
        await _mockMediator.Received(1).Send(Arg.Is<SplitPathCommand>(c => c.Path == path), Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task SplitPath_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        string path = @"C:\Users\TestUser\Documents";
        CancellationTokenSource cts = new();

        _mockMediator.Send(Arg.Any<SplitPathCommand>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => new ValueTask<ErrorOr<IEnumerable<PathSegmentResponse>>>(Task.Run(async () =>
            {
                await Task.Delay(100, callInfo.Arg<CancellationToken>());
                return ErrorOrFactory.From(new List<PathSegmentResponse>
                {
                _pathSegmentResponseFixture.Create()
                }.AsEnumerable());
            }, callInfo.Arg<CancellationToken>())));

        // Act & Assert
        cts.CancelAfter(50);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _sut.SplitPath(path, cts.Token));
    }

    [Fact]
    public async Task ValidatePath_WhenCalled_ShouldReturnOkResultWithPathValidResponse()
    {
        // Arrange
        string path = @"C:\Users\TestUser\Documents";
        CancellationToken cancellationToken = CancellationToken.None;
        PathValidResponse expectedResponse = new(true);
        _mockMediator.Send(Arg.Any<ValidatePathQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        IActionResult result = await _sut.ValidatePath(path, cancellationToken);

        // Assert
        OkObjectResult okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        PathValidResponse actualResponse = okResult.Value.Should().BeAssignableTo<PathValidResponse>().Subject;
        actualResponse.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task ValidatePath_WhenCalled_ShouldSendValidatePathQueryToMediator()
    {
        // Arrange
        string path = @"C:\Users\TestUser\Documents";
        CancellationToken cancellationToken = CancellationToken.None;
        _mockMediator.Send(Arg.Any<ValidatePathQuery>(), Arg.Any<CancellationToken>())
            .Returns(new PathValidResponse(true));

        // Act
        await _sut.ValidatePath(path, cancellationToken);

        // Assert
        await _mockMediator.Received(1).Send(Arg.Is<ValidatePathQuery>(q => q.Path == path), Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task ValidatePath_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        string path = @"C:\Users\TestUser\Documents";
        CancellationTokenSource cts = new();

        _mockMediator.Send(Arg.Any<ValidatePathQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => new ValueTask<PathValidResponse>(Task.Run(async () =>
            {
                await Task.Delay(100, callInfo.Arg<CancellationToken>());
                return new PathValidResponse(true);
            }, callInfo.Arg<CancellationToken>())));

        // Act & Assert
        cts.CancelAfter(50);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _sut.ValidatePath(path, cts.Token));
    }

    [Fact]
    public async Task CheckPathExists_WhenCalled_ShouldReturnOkResultWithPathExistsResponse()
    {
        // Arrange
        string path = @"C:\Users\TestUser\Documents";
        CancellationToken cancellationToken = CancellationToken.None;
        PathExistsResponse expectedResponse = new(true);
        _mockMediator.Send(Arg.Any<CheckPathExistsQuery>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        IActionResult result = await _sut.CheckPathExists(path, true, cancellationToken);

        // Assert
        OkObjectResult okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        PathExistsResponse actualResponse = okResult.Value.Should().BeAssignableTo<PathExistsResponse>().Subject;
        actualResponse.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task CheckPathExists_WhenCalled_ShouldSendCheckPathExistsQueryToMediator()
    {
        // Arrange
        string path = @"C:\Users\TestUser\Documents";
        CancellationToken cancellationToken = CancellationToken.None;
        _mockMediator.Send(Arg.Any<CheckPathExistsQuery>(), Arg.Any<CancellationToken>())
            .Returns(new PathExistsResponse(true));

        // Act
        await _sut.CheckPathExists(path, true, cancellationToken);

        // Assert
        await _mockMediator.Received(1).Send(Arg.Is<CheckPathExistsQuery>(q => q.Path == path), Arg.Is(cancellationToken));
    }

    [Fact]
    public async Task CheckPathExists_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        string path = @"C:\Users\TestUser\Documents";
        CancellationTokenSource cts = new();

        _mockMediator.Send(Arg.Any<CheckPathExistsQuery>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => new ValueTask<PathExistsResponse>(Task.Run(async () =>
            {
                await Task.Delay(100, callInfo.Arg<CancellationToken>());
                return new PathExistsResponse(true);
            }, callInfo.Arg<CancellationToken>())));

        // Act & Assert
        cts.CancelAfter(50);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _sut.CheckPathExists(path, true, cts.Token));
    }
}
