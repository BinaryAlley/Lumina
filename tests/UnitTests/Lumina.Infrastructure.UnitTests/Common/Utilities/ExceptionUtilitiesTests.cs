#region ========================================================================= USING =====================================================================================
using Lumina.Infrastructure.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Infrastructure.UnitTests.Common.Utilities;

/// <summary>
/// Contains unit tests for the <see cref="ExceptionUtilities"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ExceptionUtilitiesTests
{
    [Fact]
    public void AggregateMessages_WhenCalledWithNestedException_ShouldConcatenateAllMessages()
    {
        // Arrange
        Exception exception = new("outer", new InvalidOperationException("inner", new ArgumentException("deepest")));

        // Act
        string result = exception.AggregateMessages();

        // Assert
        Assert.Contains("outer", result);
        Assert.Contains("inner", result);
        Assert.Contains("deepest", result);
        Assert.Contains(" -> ", result);
    }

    [Fact]
    public void AggregateMessages_WhenCalledWithSingleException_ShouldReturnItsMessageWithArrow()
    {
        // Arrange
        Exception exception = new("only-message");

        // Act
        string result = exception.AggregateMessages();

        // Assert
        Assert.Contains("only-message", result);
        Assert.Contains(" -> ", result);
    }

    [Fact]
    public void GetInnerExceptions_WhenExceptionIsNull_ShouldReturnEmptySequence()
    {
        // Act
        IEnumerable<Exception> result = ((Exception)null!).GetInnerExceptions();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetInnerExceptions_WhenExceptionHasNestedInners_ShouldReturnAllLevels()
    {
        // Arrange
        Exception exception = new("first", new InvalidOperationException("second", new ArgumentException("third")));

        // Act
        List<Exception> result = [.. exception.GetInnerExceptions()];

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("first", result[0].Message);
        Assert.Equal("second", result[1].Message);
        Assert.Equal("third", result[2].Message);
    }

    [Fact]
    public void GetInnerExceptions_WhenChainExceedsMaximumDepth_ShouldStopAtMaximumDepth()
    {
        // Arrange
        Exception exception = new("level-1", new Exception("level-2", new Exception("level-3", new Exception("level-4", new Exception("level-5", new Exception("level-6"))))));

        // Act
        List<Exception> result = [.. exception.GetInnerExceptions(maximumDepth: 3)];

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(["level-1", "level-2", "level-3"], result.Select(inner => inner.Message));
    }

    [Fact]
    public void GetInnerExceptions_WhenExceptionIsAggregate_ShouldReturnItsInnerExceptions()
    {
        // Arrange
        AggregateException exception = new(
            "aggregate",
            [new InvalidOperationException("first-inner", new Exception("first-deep")), new ArgumentException("second-inner")]);

        // Act
        List<Exception> result = [.. exception.GetInnerExceptions()];

        // Assert
        // the aggregate exception itself, its inner exceptions and their inners are traversed, and the first inner exception
        // is also reachable through the regular InnerException chain, so it is yielded twice
        Assert.Equal(6, result.Count);
        Assert.Contains(result, inner => inner is AggregateException);
        Assert.Equal(2, result.Count(inner => inner.Message == "first-inner"));
        Assert.Equal(2, result.Count(inner => inner.Message == "first-deep"));
        Assert.Single(result, inner => inner.Message == "second-inner");
    }

    [Fact]
    public void AggregateMessage_WhenCalled_ShouldContainMessageAndCallStack()
    {
        // Arrange
        Exception exception = new("some-message");

        // Act
        string result = exception.AggregateMessage();

        // Assert
        Assert.Contains("Message: some-message", result);
        Assert.Contains("CallStack:", result);
    }
}
