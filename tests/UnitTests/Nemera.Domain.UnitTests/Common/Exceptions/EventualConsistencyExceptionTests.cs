#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Exceptions;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Common.Exceptions;

/// <summary>
/// Contains unit tests for the <see cref="EventualConsistencyException"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class EventualConsistencyExceptionTests
{
    [Fact]
    public void Constructor_WhenCalledWithError_ShouldSetErrorAndMessage()
    {
        // Arrange
        Error error = Error.Validation("Code", "The operation could not be completed.");

        // Act
        EventualConsistencyException exception = new(error);

        // Assert
        Assert.Equal(error, exception.EventualConsistencyError);
        Assert.Equal(error.Description, exception.Message);
        Assert.Empty(exception.UnderlyingErrors);
    }

    [Fact]
    public void Constructor_WhenCalledWithUnderlyingErrors_ShouldSetUnderlyingErrors()
    {
        // Arrange
        Error primaryError = Error.Validation("Primary", "Primary error.");
        List<Error> underlyingErrors = [Error.NotFound("Underlying1", "Underlying error 1."), Error.Conflict("Underlying2", "Underlying error 2.")];

        // Act
        EventualConsistencyException exception = new(primaryError, underlyingErrors);

        // Assert
        Assert.Equal(primaryError, exception.EventualConsistencyError);
        Assert.Equal(underlyingErrors, exception.UnderlyingErrors);
    }

    [Fact]
    public void Constructor_WhenUnderlyingErrorsAreNull_ShouldUseEmptyList()
    {
        // Act
        EventualConsistencyException exception = new(Error.Failure("Code", "Description"), null);

        // Assert
        Assert.NotNull(exception.UnderlyingErrors);
        Assert.Empty(exception.UnderlyingErrors);
    }
}
