#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using FluentAssertions;
using Lumina.Presentation.Api.UnitTests.Core.Endpoints.Common.Fixtures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.Common;

/// <summary>
/// Contains unit tests for the <see cref="BaseEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BaseEndpointTests
{
    private readonly BaseEndpointFixture _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseEndpointTests"/> class.
    /// </summary>
    public BaseEndpointTests()
    {
        _sut = Factory.Create<BaseEndpointFixture>();
    }

    [Fact]
    public void Problem_WhenCalledWithNoErrors_ShouldReturnGenericProblem()
    {
        // Act
        IResult result = _sut.TestProblem([]);

        // Assert
        result.Should().BeOfType<ProblemHttpResult>();
    }

    [Fact]
    public void Problem_WhenCalledWithValidationErrors_ShouldReturnValidationProblem()
    {
        // Arrange
        List<Error> errors = [
            Error.Validation("Code1", "Description1"),
            Error.Validation("Code2", "Description2")
        ];

        // Act
        IResult result = _sut.TestProblem(errors);

        // Assert
        result.Should().BeOfType<ProblemHttpResult>();
        ProblemHttpResult problemDetails = (ProblemHttpResult)result;
        problemDetails.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        problemDetails.ContentType.Should().Be("application/problem+json");
        problemDetails.ProblemDetails.Should().BeOfType<HttpValidationProblemDetails>();
        HttpValidationProblemDetails validationProblemDetails = (HttpValidationProblemDetails)problemDetails.ProblemDetails;
        validationProblemDetails.Status.Should().Be(StatusCodes.Status422UnprocessableEntity);
        validationProblemDetails.Title.Should().Be("Validation Error");
        validationProblemDetails.Detail.Should().Be("One or more validation errors occurred.");
        validationProblemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc4918#section-11.2");
        validationProblemDetails.Errors.Should().HaveCount(2);
        validationProblemDetails.Errors["Code1"].Should().BeEquivalentTo(["Description1"]);
        validationProblemDetails.Errors["Code2"].Should().BeEquivalentTo(["Description2"]);
    }

    [Theory]
    [InlineData(ErrorType.Conflict, StatusCodes.Status409Conflict)]
    [InlineData(ErrorType.NotFound, StatusCodes.Status404NotFound)]
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

        // Act
        IResult result = _sut.TestProblem(errors);

        // Assert
        result.Should().BeOfType<ProblemHttpResult>();
        ProblemHttpResult problemResult = (ProblemHttpResult)result;
        problemResult.StatusCode.Should().Be(expectedStatusCode);
        problemResult.ProblemDetails.Status.Should().Be(expectedStatusCode);
        problemResult.ProblemDetails.Title.Should().Be("ErrorCode");
        problemResult.ProblemDetails.Detail.Should().Be("ErrorDescription");
        problemResult.ProblemDetails.Instance.Should().NotBeNull();
        problemResult.ProblemDetails.Extensions.Should().ContainKey("traceId");
    }

    [Fact]
    public void Problem_WhenCalledWithSingleValidationError_ShouldReturnCorrectStatusCode()
    {
        // Arrange
        Error error = Error.Validation();
        List<Error> errors = [error];

        // Act
        IResult result = _sut.TestProblem(errors);

        // Assert
        result.Should().BeOfType<ProblemHttpResult>();
        ProblemHttpResult problemResult = (ProblemHttpResult)result;
        problemResult.StatusCode.Should().Be(422);
        problemResult.ProblemDetails.Status.Should().Be(StatusCodes.Status422UnprocessableEntity);
        problemResult.ProblemDetails.Title.Should().Be("Validation Error");
        problemResult.ProblemDetails.Detail.Should().Be("One or more validation errors occurred.");
        problemResult.ProblemDetails.Instance.Should().NotBeNull();
        problemResult.ProblemDetails.Extensions.Should().ContainKey("traceId");
    }

    [Fact]
    public void Problem_WhenCalledWithMixedErrors_ShouldReturnProblemResult()
    {
        // Arrange
        List<Error> errors = [
            Error.NotFound("FailureCode", "Failure Error"),
            Error.Failure("FailureCode", "Failure Error")
        ];

        // Act
        IResult result = _sut.TestProblem(errors);

        // Assert
        result.Should().BeOfType<ProblemHttpResult>();
        ProblemHttpResult problemResult = (ProblemHttpResult)result;
        problemResult.StatusCode.Should().BeOneOf(StatusCodes.Status404NotFound, StatusCodes.Status400BadRequest);
        problemResult.ProblemDetails.Status.Should().BeOneOf(StatusCodes.Status404NotFound, StatusCodes.Status400BadRequest);
        problemResult.ProblemDetails.Title.Should().Be("FailureCode");
        problemResult.ProblemDetails.Detail.Should().Be("Failure Error");
    }

    [Fact]
    public void ValidationProblem_WhenCalled_ShouldReturnValidationProblemResult()
    {
        // Arrange
        List<Error> errors = [
            Error.Validation("Code1", "Description1"),
            Error.Validation("Code2", "Description2")
        ];

        // Act
        IResult result = _sut.TestValidationProblem(errors);

        // Assert
        result.Should().BeOfType<ProblemHttpResult>();
        ProblemHttpResult problemDetails = (ProblemHttpResult)result;
        problemDetails.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        problemDetails.ContentType.Should().Be("application/problem+json");
        problemDetails.ProblemDetails.Should().BeOfType<HttpValidationProblemDetails>();
        HttpValidationProblemDetails validationProblemDetails = (HttpValidationProblemDetails)problemDetails.ProblemDetails;
        validationProblemDetails.Status.Should().Be(StatusCodes.Status422UnprocessableEntity);
        validationProblemDetails.Title.Should().Be("Validation Error");
        validationProblemDetails.Detail.Should().Be("One or more validation errors occurred.");
        validationProblemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc4918#section-11.2");
        validationProblemDetails.Errors.Should().HaveCount(2);
        validationProblemDetails.Errors["Code1"].Should().BeEquivalentTo(["Description1"]);
        validationProblemDetails.Errors["Code2"].Should().BeEquivalentTo(["Description2"]);
    }

    [Fact]
    public void ValidationProblem_WhenCalledWithDuplicateErrorCodes_ShouldMergeDescriptions()
    {
        // Arrange
        List<Error> errors = [
            Error.Validation("Code1", "Description1"),
            Error.Validation("Code1", "Description2"),
            Error.Validation("Code2", "Description3")
        ];

        // Act
        IResult result = _sut.TestValidationProblem(errors);

        // Assert
        result.Should().BeOfType<ProblemHttpResult>();
        ProblemHttpResult problemDetails = (ProblemHttpResult)result;
        HttpValidationProblemDetails validationProblemDetails = (HttpValidationProblemDetails)problemDetails.ProblemDetails;
        validationProblemDetails.Errors.Should().ContainKeys("Code1", "Code2");
        validationProblemDetails.Errors["Code1"].Should().BeEquivalentTo(["Description1", "Description2"]);
        validationProblemDetails.Errors["Code2"].Should().BeEquivalentTo(["Description3"]);
    }
}
