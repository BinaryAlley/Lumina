#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.UnitTests.Common.Primitives;

/// <summary>
/// Contains unit tests for the <see cref="Result{TValue}"/> structure.
/// </summary>
[ExcludeFromCodeCoverage]
public class ResultTests
{
    [Fact]
    public void Success_WhenCalledWithValue_ShouldCreateSuccessfulResult()
    {
        // Act
        Result<string> result = Result<string>.Success("test");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal("test", result.Value);
    }

    [Fact]
    public void Failure_WhenCalledWithSingleError_ShouldCreateFailedResult()
    {
        // Arrange
        Error error = Error.Validation("Code", "Description");

        // Act
        Result<string> result = Result<string>.Failure(error);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Single(result.Errors);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public void Failure_WhenCalledWithMultipleErrors_ShouldCreateFailedResultWithAllErrors()
    {
        // Arrange
        Error firstError = Error.Validation("Code1", "Description1");
        Error secondError = Error.NotFound("Code2", "Description2");

        // Act
        Result<string> result = Result<string>.Failure(new List<Error>([firstError, secondError]));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(2, result.Errors.Count);
        Assert.Equal(firstError, result.Errors[0]);
        Assert.Equal(secondError, result.Errors[1]);
        Assert.Equal(firstError, result.FirstError);
    }

    [Fact]
    public void ImplicitConversion_WhenConvertingValue_ShouldCreateSuccessfulResult()
    {
        // Act
        Result<string> result = "test";

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("test", result.Value);
    }

    [Fact]
    public void ImplicitConversion_WhenConvertingError_ShouldCreateFailedResult()
    {
        // Arrange
        Error error = Error.Conflict("Code", "Description");

        // Act
        Result<string> result = error;

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public void ImplicitConversion_WhenConvertingErrorList_ShouldCreateFailedResult()
    {
        // Arrange
        List<Error> errors = [Error.Validation("Code", "Description")];

        // Act
        Result<string> result = errors;

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(errors, result.Errors);
    }

    [Fact]
    public void Map_WhenResultIsSuccessful_ShouldMapValue()
    {
        // Arrange
        Result<int> result = Result<int>.Success(5);

        // Act
        Result<string> mappedResult = result.Map(value => value.ToString());

        // Assert
        Assert.True(mappedResult.IsSuccess);
        Assert.Equal("5", mappedResult.Value);
    }

    [Fact]
    public void Map_WhenResultIsFailed_ShouldPreserveErrors()
    {
        // Arrange
        Error error = Error.Validation("Code", "Description");
        Result<int> result = Result<int>.Failure(error);

        // Act
        Result<string> mappedResult = result.Map(value => value.ToString());

        // Assert
        Assert.True(mappedResult.IsFailure);
        Assert.Equal(error, mappedResult.FirstError);
    }

    [Fact]
    public void Bind_WhenResultIsSuccessful_ShouldReturnBinderResult()
    {
        // Arrange
        Result<int> result = Result<int>.Success(5);

        // Act
        Result<string> boundResult = result.Bind(value => Result<string>.Success(value.ToString()));

        // Assert
        Assert.True(boundResult.IsSuccess);
        Assert.Equal("5", boundResult.Value);
    }

    [Fact]
    public void Bind_WhenResultIsFailed_ShouldPreserveErrors()
    {
        // Arrange
        Error error = Error.Validation("Code", "Description");
        Result<int> result = Result<int>.Failure(error);

        // Act
        Result<string> boundResult = result.Bind(value => Result<string>.Success(value.ToString()));

        // Assert
        Assert.True(boundResult.IsFailure);
        Assert.Equal(error, boundResult.FirstError);
    }

    [Fact]
    public void OnSuccess_WhenResultIsSuccessful_ShouldExecuteAction()
    {
        // Arrange
        Result<int> result = Result<int>.Success(5);
        bool actionExecuted = false;

        // Act
        result.OnSuccess(value => actionExecuted = true);

        // Assert
        Assert.True(actionExecuted);
    }

    [Fact]
    public void OnSuccess_WhenResultIsFailed_ShouldNotExecuteAction()
    {
        // Arrange
        Result<int> result = Result<int>.Failure(Error.Validation("Code", "Description"));
        bool actionExecuted = false;

        // Act
        result.OnSuccess(value => actionExecuted = true);

        // Assert
        Assert.False(actionExecuted);
    }

    [Fact]
    public void OnFailure_WhenResultIsFailed_ShouldExecuteAction()
    {
        // Arrange
        Error error = Error.Validation("Code", "Description");
        Result<int> result = Result<int>.Failure(error);
        List<Error>? receivedErrors = null;

        // Act
        result.OnFailure(errors => receivedErrors = [.. errors]);

        // Assert
        Assert.NotNull(receivedErrors);
        Assert.Single(receivedErrors);
        Assert.Equal(error, receivedErrors[0]);
    }

    [Fact]
    public void OnFailure_WhenResultIsSuccessful_ShouldNotExecuteAction()
    {
        // Arrange
        Result<int> result = Result<int>.Success(5);
        bool actionExecuted = false;

        // Act
        result.OnFailure(errors => actionExecuted = true);

        // Assert
        Assert.False(actionExecuted);
    }

    [Fact]
    public void Match_WhenResultIsSuccessful_ShouldExecuteOnSuccessFunction()
    {
        // Arrange
        Result<int> result = Result<int>.Success(5);

        // Act
        string matchedResult = result.Match(value => $"Success: {value}", errors => "Failure");

        // Assert
        Assert.Equal("Success: 5", matchedResult);
    }

    [Fact]
    public void Match_WhenResultIsFailed_ShouldExecuteOnFailureFunction()
    {
        // Arrange
        Result<int> result = Result<int>.Failure(Error.Validation("Code", "Description"));

        // Act
        string matchedResult = result.Match(value => "Success", errors => $"Failure: {errors.Count}");

        // Assert
        Assert.Equal("Failure: 1", matchedResult);
    }

    [Fact]
    public void Value_WhenResultIsFailed_ShouldThrowInvalidOperationException()
    {
        // Arrange
        Result<string> result = Result<string>.Failure(Error.Validation("Code", "Description"));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Errors_WhenResultIsSuccessful_ShouldThrowInvalidOperationException()
    {
        // Arrange
        Result<string> result = Result<string>.Success("test");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => result.Errors);
    }

    [Fact]
    public void Equals_WhenBothResultsAreSuccessfulWithSameValue_ShouldReturnTrue()
    {
        // Act
        Result<string> firstResult = Result<string>.Success("test");
        Result<string> secondResult = Result<string>.Success("test");

        // Assert
        Assert.True(firstResult.Equals(secondResult));
    }

    [Fact]
    public void Equals_WhenResultsHaveDifferentSuccessValues_ShouldReturnFalse()
    {
        // Act
        Result<string> firstResult = Result<string>.Success("first");
        Result<string> secondResult = Result<string>.Success("second");

        // Assert
        Assert.False(firstResult.Equals(secondResult));
    }

    [Fact]
    public void Equals_WhenBothResultsAreFailedWithSameErrors_ShouldReturnTrue()
    {
        // Act
        Result<string> firstResult = Result<string>.Failure(Error.Validation("Code", "Description"));
        Result<string> secondResult = Result<string>.Failure(Error.Validation("Code", "Description"));

        // Assert
        Assert.True(firstResult.Equals(secondResult));
    }

    [Fact]
    public void EqualityOperator_WhenResultsAreEqual_ShouldReturnTrue()
    {
        // Act
        Result<int> firstResult = Result<int>.Success(5);
        Result<int> secondResult = Result<int>.Success(5);

        // Assert
        Assert.True(firstResult == secondResult);
    }

    [Fact]
    public void InequalityOperator_WhenResultsAreNotEqual_ShouldReturnTrue()
    {
        // Act
        Result<int> firstResult = Result<int>.Success(5);
        Result<int> secondResult = Result<int>.Success(6);

        // Assert
        Assert.True(firstResult != secondResult);
    }

    [Fact]
    public void GetHashCode_WhenResultsAreEqual_ShouldReturnSameHashCode()
    {
        // Act
        Result<int> firstResult = Result<int>.Success(5);
        Result<int> secondResult = Result<int>.Success(5);

        // Assert
        Assert.Equal(firstResult.GetHashCode(), secondResult.GetHashCode());
    }

    [Fact]
    public void ToString_WhenResultIsSuccessful_ShouldReturnSuccessString()
    {
        // Arrange
        Result<string> result = Result<string>.Success("test");

        // Act
        string resultString = result.ToString();

        // Assert
        Assert.Equal("Success: test", resultString);
    }

    [Fact]
    public void ToString_WhenResultIsFailed_ShouldReturnFailureString()
    {
        // Arrange
        Result<string> result = Result<string>.Failure(Error.Validation("Code", "Description"));

        // Act
        string resultString = result.ToString();

        // Assert
        Assert.Equal("Failure: [ValidationError { Type = Validation, Code = Code, Description = Description }]", resultString);
    }

    [Fact]
    public void From_WhenCalledWithValue_ShouldCreateSuccessfulResult()
    {
        // Act
        Result<int> result = Result.From(5);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value);
    }

    [Fact]
    public void Marker_WhenAccessed_ShouldExposePredefinedSuccessResults()
    {
        // Act & Assert
        Assert.Equal(new Success(), Result.Success);
        Assert.Equal(new Created(), Result.Created);
        Assert.Equal(new Updated(), Result.Updated);
        Assert.Equal(new Deleted(), Result.Deleted);
    }
}
