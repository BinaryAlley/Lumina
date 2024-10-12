#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Presentation.Api.Common.Http;
using Lumina.Presentation.Api.Core.Controllers.Common;
using Lumina.Presentation.Api.UnitTests.Core.Controllers.Common.Fixtures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NSubstitute;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Controllers.Common;

/// <summary>
/// Contains unit tests for the <see cref="ApiController"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ApiControllerTests
{
    private readonly ApiControllerFixture _sut;
    private readonly HttpContext _mockHttpContext;
    private readonly ProblemDetailsFactory _mockProblemDetailsFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiControllerTests"/> class.
    /// </summary>
    public ApiControllerTests()
    {
        _sut = new ApiControllerFixture();
        _mockHttpContext = Substitute.For<HttpContext>();
        _mockProblemDetailsFactory = Substitute.For<ProblemDetailsFactory>();
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = _mockHttpContext
        };
        _sut.ProblemDetailsFactory = _mockProblemDetailsFactory;
    }

    [Fact]
    public void Problem_WhenCalledWithNoErrors_ShouldReturnGenericProblem()
    {
        // Arrange
        ProblemDetailsFactory mockProblemDetailsFactory = Substitute.For<ProblemDetailsFactory>();
        mockProblemDetailsFactory.CreateProblemDetails(
            Arg.Any<HttpContext>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(new ProblemDetails());

        _sut.ProblemDetailsFactory = mockProblemDetailsFactory;

        // Act
        IActionResult result = _sut.TestProblem([]);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        ObjectResult objectResult = (ObjectResult)result;
        objectResult.Value.Should().BeOfType<ProblemDetails>();
    }

    [Fact]
    public void Problem_WhenCalledWithValidationErrors_ShouldReturnValidationProblem()
    {
        // Arrange
        List<Error> errors = [
            Error.Validation("Code1", "Description1"),
            Error.Validation("Code2", "Description2")
        ];

        _mockProblemDetailsFactory.CreateValidationProblemDetails(
            Arg.Any<HttpContext>(),
            Arg.Any<ModelStateDictionary>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(new ValidationProblemDetails());

        // Act
        IActionResult result = _sut.TestProblem(errors);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        ObjectResult objectResult = (ObjectResult)result;
        objectResult.Value.Should().BeOfType<ValidationProblemDetails>();
    }

    [Theory]
    [InlineData(ErrorType.Conflict, StatusCodes.Status409Conflict)]
    [InlineData(ErrorType.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorType.Validation, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorType.Failure, StatusCodes.Status403Forbidden)]
    [InlineData(ErrorType.Unexpected, StatusCodes.Status500InternalServerError)]
    public void Problem_WhenCalledWithSingleError_ShouldReturnCorrectStatusCode(ErrorType errorType, int expectedStatusCode)
    {
        // Arrange
        Error error = errorType switch
        {
            ErrorType.Conflict => Error.Conflict("ErrorCode", "ErrorDescription"),
            ErrorType.NotFound => Error.NotFound("ErrorCode", "ErrorDescription"),
            ErrorType.Validation => Error.Validation("ErrorCode", "ErrorDescription"),
            ErrorType.Failure => Error.Failure("ErrorCode", "ErrorDescription"),
            _ => Error.Unexpected("ErrorCode", "ErrorDescription"),
        };
        List<Error> errors = [error];

        ProblemDetailsFactory mockProblemDetailsFactory = Substitute.For<ProblemDetailsFactory>();
        mockProblemDetailsFactory.CreateProblemDetails(
            Arg.Any<HttpContext>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(new ProblemDetails { Status = expectedStatusCode });

        mockProblemDetailsFactory.CreateValidationProblemDetails(
            Arg.Any<HttpContext>(),
            Arg.Any<ModelStateDictionary>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(new ValidationProblemDetails { Status = expectedStatusCode });

        _sut.ProblemDetailsFactory = mockProblemDetailsFactory;

        // Act
        IActionResult result = _sut.TestProblem(errors);

        // Assert
        if (errorType == ErrorType.Validation)
        {
            result.Should().BeOfType<BadRequestObjectResult>();
            BadRequestObjectResult badRequestResult = (BadRequestObjectResult)result;
            badRequestResult.StatusCode.Should().Be(expectedStatusCode);
            badRequestResult.Value.Should().BeOfType<ValidationProblemDetails>();
            ((ValidationProblemDetails)badRequestResult.Value!).Status.Should().Be(expectedStatusCode);

            mockProblemDetailsFactory.Received(1).CreateValidationProblemDetails(
                Arg.Any<HttpContext>(),
                Arg.Any<ModelStateDictionary>(),
                Arg.Is<int?>(status => status == null),
                Arg.Is<string>(title => title == "One or more validation errors occurred."),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>());
        }
        else
        {
            result.Should().BeOfType<ObjectResult>();
            ObjectResult objectResult = (ObjectResult)result;
            objectResult.StatusCode.Should().Be(expectedStatusCode);
            objectResult.Value.Should().BeOfType<ProblemDetails>();
            ((ProblemDetails)objectResult.Value!).Status.Should().Be(expectedStatusCode);

            mockProblemDetailsFactory.Received(1).CreateProblemDetails(
                Arg.Any<HttpContext>(),
                Arg.Is<int?>(status => status == expectedStatusCode),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>());
        }
    }

    [Fact]
    public void Problem_WhenCalledWithMixedErrors_ShouldReturnBadRequestObjectResult()
    {
        // Arrange
        List<Error> errors = [
            Error.Validation("ValidationCode", "Validation Error"),
        Error.Failure("FailureCode", "Failure Error")
        ];

        ProblemDetailsFactory mockProblemDetailsFactory = Substitute.For<ProblemDetailsFactory>();
        mockProblemDetailsFactory.CreateProblemDetails(
            Arg.Any<HttpContext>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(new ProblemDetails { Status = StatusCodes.Status400BadRequest });

        _sut.ProblemDetailsFactory = mockProblemDetailsFactory;

        // Act
        IActionResult result = _sut.TestProblem(errors);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        ObjectResult objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        objectResult.Value.Should().BeOfType<ProblemDetails>();
        ((ProblemDetails)objectResult.Value!).Status.Should().Be(StatusCodes.Status400BadRequest);

        mockProblemDetailsFactory.Received(1).CreateProblemDetails(
            Arg.Any<HttpContext>(),
            Arg.Is<int?>(status => status == StatusCodes.Status400BadRequest),
            Arg.Is<string>(title => title == "Validation"),
            Arg.Any<string>(),
            Arg.Is<string>(detail => detail == "Validation Error"),
            Arg.Any<string>());
    }

    [Fact]
    public void Problem_WhenCalled_ShouldAddErrorsToHttpContext()
    {
        // Arrange
        List<Error> errors = [Error.Failure("ErrorCode", "ErrorDescription")];
        Dictionary<object, object> mockItems = [];
        _mockHttpContext.Items.Returns(mockItems!);

        ProblemDetailsFactory mockProblemDetailsFactory = Substitute.For<ProblemDetailsFactory>();
        mockProblemDetailsFactory.CreateProblemDetails(
            Arg.Any<HttpContext>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(new ProblemDetails());

        _sut.ProblemDetailsFactory = mockProblemDetailsFactory;

        // Act
        _sut.TestProblem(errors);

        // Assert
        mockItems.Should().ContainKey(HttpContextItemKeys.ERRORS);
        mockItems[HttpContextItemKeys.ERRORS].Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void ValidationProblem_WhenCalled_ShouldAddErrorsToModelState()
    {
        // Arrange
        List<Error> errors = [
            Error.Validation("Code1", "Description1"),
            Error.Validation("Code2", "Description2")
        ];

        _mockProblemDetailsFactory.CreateValidationProblemDetails(
            Arg.Any<HttpContext>(),
            Arg.Any<ModelStateDictionary>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(callInfo =>
            {
                ModelStateDictionary modelState = callInfo.ArgAt<ModelStateDictionary>(1);
                return new ValidationProblemDetails(modelState);
            });

        // Act
        IActionResult result = _sut.TestValidationProblem(errors);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        ObjectResult objectResult = (ObjectResult)result;
        ValidationProblemDetails validationProblem = objectResult.Value.Should().BeOfType<ValidationProblemDetails>().Subject;
        validationProblem.Errors.Should().ContainKeys("Code1", "Code2");
        validationProblem.Errors["Code1"].Should().Contain("Description1");
        validationProblem.Errors["Code2"].Should().Contain("Description2");
    }
}
