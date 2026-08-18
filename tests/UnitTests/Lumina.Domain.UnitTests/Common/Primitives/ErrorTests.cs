#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Primitives;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Common.Primitives;

/// <summary>
/// Contains unit tests for the <see cref="Error"/> class and its derived error records.
/// </summary>
[ExcludeFromCodeCoverage]
public class ErrorTests
{
    [Fact]
    public void NotFound_WhenCalledWithoutArguments_ShouldUseDefaultCodeAndDescription()
    {
        // Act
        Error error = Error.NotFound();

        // Assert
        AssertError(error, ErrorType.NotFound, "General.NotFound", "A 'Not Found' error has occurred.");
    }

    [Fact]
    public void NotFound_WhenCalledWithCodeAndDescription_ShouldUseProvidedValues()
    {
        // Act
        Error error = Error.NotFound("Library.NotFound", "The library was not found.");

        // Assert
        AssertError(error, ErrorType.NotFound, "Library.NotFound", "The library was not found.");
    }

    [Fact]
    public void InvalidOperation_WhenCalledWithoutArguments_ShouldUseDefaultCodeAndDescription()
    {
        // Act
        Error error = Error.InvalidOperation();

        // Assert
        AssertError(error, ErrorType.Unexpected, "General.InvalidOperation", "An invalid operation error has occurred.");
    }

    [Fact]
    public void Forbidden_WhenCalledWithoutArguments_ShouldUseDefaultCodeAndDescription()
    {
        // Act
        Error error = Error.Forbidden();

        // Assert
        AssertError(error, ErrorType.Forbidden, "General.Forbidden", "A 'Forbidden' error has occurred.");
    }

    [Fact]
    public void Failure_WhenCalledWithoutArguments_ShouldUseDefaultCodeAndDescription()
    {
        // Act
        Error error = Error.Failure();

        // Assert
        AssertError(error, ErrorType.Failure, "General.Failure", "A failure has occurred.");
    }

    [Fact]
    public void Validation_WhenCalledWithoutArguments_ShouldUseDefaultCodeAndDescription()
    {
        // Act
        Error error = Error.Validation();

        // Assert
        AssertError(error, ErrorType.Validation, "General.Validation", "A validation error has occurred.");
    }

    [Fact]
    public void Validation_WhenCalledWithCodeAndDescription_ShouldUseProvidedValues()
    {
        // Act
        Error error = Error.Validation("Title.CannotBeEmpty", "The title cannot be empty.");

        // Assert
        AssertError(error, ErrorType.Validation, "Title.CannotBeEmpty", "The title cannot be empty.");
    }

    [Fact]
    public void Conflict_WhenCalledWithoutArguments_ShouldUseDefaultCodeAndDescription()
    {
        // Act
        Error error = Error.Conflict();

        // Assert
        AssertError(error, ErrorType.Conflict, "General.Conflict", "A conflict error has occurred.");
    }

    [Fact]
    public void Timeout_WhenCalledWithoutArguments_ShouldUseDefaultCodeAndDescription()
    {
        // Act
        Error error = Error.Timeout();

        // Assert
        AssertError(error, ErrorType.Unexpected, "General.Timeout", "The operation timed out.");
    }

    [Fact]
    public void Unauthorized_WhenCalledWithoutArguments_ShouldUseDefaultCodeAndDescription()
    {
        // Act
        Error error = Error.Unauthorized();

        // Assert
        AssertError(error, ErrorType.Unauthorized, "General.Unauthorized", "An 'Unauthorized' error has occurred.");
    }

    [Fact]
    public void Unexpected_WhenCalledWithoutArguments_ShouldUseDefaultCodeAndDescription()
    {
        // Act
        Error error = Error.Unexpected();

        // Assert
        AssertError(error, ErrorType.Unexpected, "General.Unexpected", "An unexpected error has occurred.");
    }

    [Fact]
    public void Internal_WhenCalledWithoutArguments_ShouldUseDefaultCodeAndDescription()
    {
        // Act
        Error error = Error.Internal();

        // Assert
        AssertError(error, ErrorType.Unexpected, "General.Internal", "An internal error has occurred.");
    }

    [Fact]
    public void ResourceUnavailable_WhenCalledWithoutArguments_ShouldUseDefaultCodeAndDescription()
    {
        // Act
        Error error = Error.ResourceUnavailable();

        // Assert
        AssertError(error, ErrorType.Unexpected, "General.ResourceUnavailable", "The required resource is currently unavailable.");
    }

    [Fact]
    public void NotImplemented_WhenCalledWithoutArguments_ShouldUseDefaultCodeAndDescription()
    {
        // Act
        Error error = Error.NotImplemented();

        // Assert
        AssertError(error, ErrorType.Unexpected, "General.NotImplemented", "The requested functionality is not implemented.");
    }

    [Fact]
    public void ToString_WhenCalled_ShouldReturnRecordRepresentation()
    {
        // Arrange
        Error error = Error.NotFound("Library.NotFound", "The library was not found.");

        // Act
        string result = error.ToString();

        // Assert
        Assert.Equal("NotFoundError { Type = NotFound, Code = Library.NotFound, Description = The library was not found. }", result);
    }

    [Fact]
    public void Equals_WhenSameCodeAndDescription_ShouldReturnTrue()
    {
        // Act
        Error firstError = Error.Validation("Code", "Description");
        Error secondError = Error.Validation("Code", "Description");

        // Assert
        Assert.Equal(firstError, secondError);
        Assert.Equal(firstError.GetHashCode(), secondError.GetHashCode());
    }

    [Fact]
    public void Equals_WhenDifferentCode_ShouldReturnFalse()
    {
        // Act
        Error firstError = Error.Validation("Code1", "Description");
        Error secondError = Error.Validation("Code2", "Description");

        // Assert
        Assert.NotEqual(firstError, secondError);
    }

    private static void AssertError(Error error, ErrorType expectedType, string expectedCode, string expectedDescription)
    {
        Assert.Equal(expectedType, error.Type);
        Assert.Equal(expectedCode, error.Code);
        Assert.Equal(expectedDescription, error.Description);
    }
}
