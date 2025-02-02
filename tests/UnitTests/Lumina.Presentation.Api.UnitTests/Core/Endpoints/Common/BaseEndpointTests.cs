#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
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
        Assert.IsType<ProblemHttpResult>(result);
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
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        HttpValidationProblemDetails validationProblemDetails = Assert.IsType<HttpValidationProblemDetails>(problemDetails.ProblemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, validationProblemDetails.Status);
        Assert.Equal("General.Validation", validationProblemDetails.Title);
        Assert.Equal("OneOrMoreValidationErrorsOccurred", validationProblemDetails.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", validationProblemDetails.Type);
        Assert.Equal(2, validationProblemDetails.Errors.Count);
        Assert.Equal(new[] { "Description1" }, validationProblemDetails.Errors["Code1"]);
        Assert.Equal(new[] { "Description2" }, validationProblemDetails.Errors["Code2"]);
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
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(expectedStatusCode, problemResult.StatusCode);
        Assert.Equal(expectedStatusCode, problemResult.ProblemDetails.Status);
        Assert.Equal("ErrorCode", problemResult.ProblemDetails.Title);
        Assert.Equal("ErrorDescription", problemResult.ProblemDetails.Detail);
        Assert.NotNull(problemResult.ProblemDetails.Instance);
        Assert.Contains("traceId", problemResult.ProblemDetails.Extensions.Keys);
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
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(422, problemResult.StatusCode);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemResult.ProblemDetails.Status);
        Assert.Equal("General.Validation", problemResult.ProblemDetails.Title);
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemResult.ProblemDetails.Detail);
        Assert.NotNull(problemResult.ProblemDetails.Instance);
        Assert.Contains("traceId", problemResult.ProblemDetails.Extensions.Keys);
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
        ProblemHttpResult problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Contains(problemResult.StatusCode, new[] { StatusCodes.Status404NotFound, StatusCodes.Status400BadRequest });
        int?[] allowedStatusCodes = [StatusCodes.Status404NotFound, StatusCodes.Status400BadRequest];
        Assert.Contains(problemResult.ProblemDetails.Status, allowedStatusCodes);
        Assert.Equal("FailureCode", problemResult.ProblemDetails.Title);
        Assert.Equal("Failure Error", problemResult.ProblemDetails.Detail);
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
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails.StatusCode);
        Assert.Equal("application/problem+json", problemDetails.ContentType);
        HttpValidationProblemDetails validationProblemDetails = Assert.IsType<HttpValidationProblemDetails>(problemDetails.ProblemDetails);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, validationProblemDetails.Status);
        Assert.Equal("General.Validation", validationProblemDetails.Title);
        Assert.Equal("OneOrMoreValidationErrorsOccurred", validationProblemDetails.Detail);
        Assert.Equal("https://tools.ietf.org/html/rfc4918#section-11.2", validationProblemDetails.Type);
        Assert.Equal(2, validationProblemDetails.Errors.Count);
        Assert.Equal(new[] { "Description1" }, validationProblemDetails.Errors["Code1"]);
        Assert.Equal(new[] { "Description2" }, validationProblemDetails.Errors["Code2"]);
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
        ProblemHttpResult problemDetails = Assert.IsType<ProblemHttpResult>(result);
        HttpValidationProblemDetails validationProblemDetails = Assert.IsType<HttpValidationProblemDetails>(problemDetails.ProblemDetails);
        Assert.Contains("Code1", validationProblemDetails.Errors.Keys);
        Assert.Contains("Code2", validationProblemDetails.Errors.Keys);
        Assert.Equal(new[] { "Description1", "Description2" }, validationProblemDetails.Errors["Code1"]);
        Assert.Equal(new[] { "Description3" }, validationProblemDetails.Errors["Code2"]);
    }
}
