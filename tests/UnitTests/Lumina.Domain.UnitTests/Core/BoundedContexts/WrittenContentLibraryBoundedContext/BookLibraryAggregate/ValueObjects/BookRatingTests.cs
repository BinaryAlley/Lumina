#region ========================================================================= USING =====================================================================================
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;

/// <summary>
/// Contains unit tests for the <see cref="BookRating"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookRatingTests
{
    private readonly BookRatingFixture _bookRatingFixture = new();

    [Fact]
    public void Create_WhenCalledWithValidValues_ShouldCreateRating()
    {
        // Act
        Result<BookRating> result = BookRating.Create(
            value: 4.5m,
            maxValue: 5,
            Optional<BookRatingSource>.Some(BookRatingSource.Goodreads),
            Optional<int>.Some(100));

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(4.5m, result.Value.Value);
        Assert.Equal(5, result.Value.MaxValue);
        Assert.True(result.Value.Source.HasValue);
        Assert.Equal(BookRatingSource.Goodreads, result.Value.Source.Value);
        Assert.True(result.Value.VoteCount.HasValue);
        Assert.Equal(100, result.Value.VoteCount.Value);
    }

    [Theory]
    [InlineData(-1)] // negative value
    [InlineData(-0.5)] // negative fractional value
    public void Create_WhenValueIsNegative_ShouldReturnError(double value)
    {
        // Act
        Result<BookRating> result = BookRating.Create((decimal)value, maxValue: 5, Optional<BookRatingSource>.None(), Optional<int>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Metadata.RatingValueMustBePositive, result.FirstError);
    }

    [Fact]
    public void Create_WhenMaxValueIsNegative_ShouldReturnError()
    {
        // Act
        Result<BookRating> result = BookRating.Create(value: 4.5m, maxValue: -5, Optional<BookRatingSource>.None(), Optional<int>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Metadata.RatingValueMustBePositive, result.FirstError);
    }

    [Fact]
    public void Create_WhenValueExceedsMaxValue_ShouldReturnError()
    {
        // Act
        Result<BookRating> result = BookRating.Create(value: 6, maxValue: 5, Optional<BookRatingSource>.None(), Optional<int>.None());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Metadata.RatingValueCannotBeGreaterThanMaxValue, result.FirstError);
    }

    [Fact]
    public void AsPercentage_WhenCalled_ShouldReturnValueAsPercentageOfMaxValue()
    {
        // Arrange
        BookRating rating = _bookRatingFixture.Create(value: 4.5m, maxValue: 5);

        // Act
        decimal result = rating.AsPercentage();

        // Assert
        Assert.Equal(90, result);
    }

    [Fact]
    public void ToString_WhenSourceAndVoteCountArePresent_ShouldIncludeThem()
    {
        // Arrange
        BookRating rating = _bookRatingFixture.Create(
            value: 4.5m,
            maxValue: 5,
            source: Optional<BookRatingSource>.Some(BookRatingSource.Goodreads),
            voteCount: Optional<int>.Some(100));

        // Act
        string result = rating.ToString();

        // Assert
        Assert.Equal("4.5/5 (Goodreads) [100 votes]", result);
    }

    [Fact]
    public void ToString_WhenSourceAndVoteCountAreAbsent_ShouldReturnBaseString()
    {
        // Arrange
        BookRating rating = _bookRatingFixture.Create(
            value: 4.5m,
            maxValue: 5,
            source: Optional<BookRatingSource>.None(),
            voteCount: Optional<int>.None());

        // Act
        string result = rating.ToString();

        // Assert
        Assert.Equal("4.5/5", result);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        BookRating firstRating = _bookRatingFixture.Create(
            value: 4.5m,
            maxValue: 5,
            source: Optional<BookRatingSource>.Some(BookRatingSource.Goodreads),
            voteCount: Optional<int>.Some(100));
        BookRating secondRating = _bookRatingFixture.Create(
            value: 4.5m,
            maxValue: 5,
            source: Optional<BookRatingSource>.Some(BookRatingSource.Goodreads),
            voteCount: Optional<int>.Some(100));

        // Act
        bool result = firstRating.Equals(secondRating);

        // Assert
        Assert.True(result);
    }
}
