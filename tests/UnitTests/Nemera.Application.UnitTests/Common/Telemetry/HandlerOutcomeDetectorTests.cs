#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Telemetry;
using Lumina.Domain.Common.Primitives;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Telemetry;

/// <summary>
/// Contains unit tests for the <see cref="HandlerOutcomeDetector"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class HandlerOutcomeDetectorTests
{
    [Fact]
    public void IsSuccessful_WhenResultIsNull_ShouldReturnTrue()
    {
        // Act
        bool result = HandlerOutcomeDetector.IsSuccessful(null);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSuccessful_WhenResultExposesNoSuccessFlag_ShouldReturnTrue()
    {
        // Act
        bool result = HandlerOutcomeDetector.IsSuccessful("a plain dto");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSuccessful_WhenResultIsSuccessful_ShouldReturnTrue()
    {
        // Arrange
        Result<string> resultValue = Result<string>.Success("value");

        // Act
        bool result = HandlerOutcomeDetector.IsSuccessful(resultValue);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSuccessful_WhenResultIsFailed_ShouldReturnFalse()
    {
        // Arrange
        Result<string> resultValue = Result<string>.Failure(Error.Validation("Test.Code", "Test description"));

        // Act
        bool result = HandlerOutcomeDetector.IsSuccessful(resultValue);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetErrorDescription_WhenResultIsNull_ShouldReturnNull()
    {
        // Act
        string? result = HandlerOutcomeDetector.GetErrorDescription(null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetErrorDescription_WhenResultExposesNoFirstError_ShouldReturnNull()
    {
        // Act
        string? result = HandlerOutcomeDetector.GetErrorDescription("a plain dto");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetErrorDescription_WhenResultIsFailed_ShouldReturnFirstErrorDescription()
    {
        // Arrange
        Result<string> resultValue = Result<string>.Failure(Error.Validation("Test.Code", "Test description"));

        // Act
        string? result = HandlerOutcomeDetector.GetErrorDescription(resultValue);

        // Assert
        Assert.Equal("Test description", result);
    }
}
