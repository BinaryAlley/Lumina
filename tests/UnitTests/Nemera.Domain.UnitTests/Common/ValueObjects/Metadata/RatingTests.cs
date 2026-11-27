#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Common.ValueObjects.Metadata;

/// <summary>
/// Contains unit tests for the <see cref="Rating"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RatingTests
{
    [Fact]
    public void Constructor_WhenCalled_ShouldSetValueMaxValueAndVoteCount()
    {
        // Act
        TestRating rating = new(value: 4.5m, maxValue: 5, voteCount: Optional<int>.Some(100));

        // Assert
        Assert.Equal(4.5m, rating.Value);
        Assert.Equal(5, rating.MaxValue);
        Assert.True(rating.VoteCount.HasValue);
        Assert.Equal(100, rating.VoteCount.Value);
    }

    [Fact]
    public void AsPercentage_WhenCalled_ShouldReturnValueAsPercentageOfMaxValue()
    {
        // Arrange
        TestRating rating = new(value: 4.5m, maxValue: 5, voteCount: Optional<int>.None());

        // Act
        decimal result = rating.AsPercentage();

        // Assert
        Assert.Equal(90, result);
    }

    [Fact]
    public void AsPercentage_WhenValueEqualsMaxValue_ShouldReturnOneHundred()
    {
        // Arrange
        TestRating rating = new(value: 5, maxValue: 5, voteCount: Optional<int>.None());

        // Act
        decimal result = rating.AsPercentage();

        // Assert
        Assert.Equal(100, result);
    }

    /// <summary>
    /// Concrete test implementation of the abstract <see cref="Rating"/> class.
    /// </summary>
    private sealed class TestRating : Rating
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestRating"/> class.
        /// </summary>
        /// <param name="value">The numeric value of the rating.</param>
        /// <param name="maxValue">The maximum possible rating value.</param>
        /// <param name="voteCount">The optional number of votes or reviews.</param>
        public TestRating(decimal value, decimal maxValue, Optional<int> voteCount) : base(value, maxValue, voteCount)
        {
        }

        /// <summary>
        /// Gets the list of items that define equality of the object.
        /// </summary>
        /// <returns>A list of items defining the equality.</returns>
        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
            yield return MaxValue;
            yield return VoteCount;
        }
    }
}
