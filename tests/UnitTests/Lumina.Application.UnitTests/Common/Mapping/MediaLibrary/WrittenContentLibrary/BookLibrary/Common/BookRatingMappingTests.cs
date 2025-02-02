#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;
using Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Common.Fixtures;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;

/// <summary>
/// Contains unit tests for the <see cref="BookRatingMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookRatingMappingTests
{
    private readonly BookRatingFixture _bookRatingFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookRatingMappingTests"/> class.
    /// </summary>
    public BookRatingMappingTests()
    {
        _bookRatingFixture = new BookRatingFixture();
    }

    [Fact]
    public void ToRepositoryEntity_WhenMappingCompleteBookRating_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        BookRating bookRating = _bookRatingFixture.CreateBookRating();

        // Act
        BookRatingEntity result = bookRating.ToRepositoryEntity();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(bookRating.Value, result.Value);
        Assert.Equal(bookRating.MaxValue, result.MaxValue);
        Assert.Equal(bookRating.Source.Value, result.Source);
        Assert.Equal(bookRating.VoteCount.Value, result.VoteCount);
    }

    [Fact]
    public void ToRepositoryEntity_WhenMappingBookRatingWithoutSource_ShouldMapCorrectly()
    {
        // Arrange
        BookRating bookRating = _bookRatingFixture.CreateBookRatingWithoutSource();

        // Act
        BookRatingEntity result = bookRating.ToRepositoryEntity();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(bookRating.Value, result.Value);
        Assert.Equal(bookRating.MaxValue, result.MaxValue);
        Assert.Null(result.Source);
        Assert.Equal(bookRating.VoteCount.Value, result.VoteCount);
    }

    [Fact]
    public void ToRepositoryEntity_WhenMappingBookRatingWithoutVoteCount_ShouldMapCorrectly()
    {
        // Arrange
        BookRating bookRating = _bookRatingFixture.CreateBookRatingWithoutVoteCount();

        // Act
        BookRatingEntity result = bookRating.ToRepositoryEntity();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(bookRating.Value, result.Value);
        Assert.Equal(bookRating.MaxValue, result.MaxValue);
        Assert.Equal(bookRating.Source.Value, result.Source);
        Assert.Null(result.VoteCount);
    }

    [Fact]
    public void ToRepositoryEntities_WhenMappingMultipleBookRatings_ShouldMapAllCorrectly()
    {
        // Arrange
        List<BookRating> bookRatings =
        [
            _bookRatingFixture.CreateBookRating(),
            _bookRatingFixture.CreateBookRatingWithoutSource(),
            _bookRatingFixture.CreateBookRatingWithoutVoteCount()
        ];

        // Act
        IEnumerable<BookRatingEntity> results = bookRatings.ToRepositoryEntities();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(bookRatings.Count, results.Count());

        List<BookRatingEntity> resultList = results.ToList();

        // complete rating
        Assert.Equal(bookRatings[0].Value, resultList[0].Value);
        Assert.Equal(bookRatings[0].MaxValue, resultList[0].MaxValue);
        Assert.Equal(bookRatings[0].Source.Value, resultList[0].Source);
        Assert.Equal(bookRatings[0].VoteCount.Value, resultList[0].VoteCount);

        // rating without source
        Assert.Equal(bookRatings[1].Value, resultList[1].Value);
        Assert.Equal(bookRatings[1].MaxValue, resultList[1].MaxValue);
        Assert.Null(resultList[1].Source);
        Assert.Equal(bookRatings[1].VoteCount.Value, resultList[1].VoteCount);

        // rating without vote count
        Assert.Equal(bookRatings[2].Value, resultList[2].Value);
        Assert.Equal(bookRatings[2].MaxValue, resultList[2].MaxValue);
        Assert.Equal(bookRatings[2].Source.Value, resultList[2].Source);
        Assert.Null(resultList[2].VoteCount);
    }
}
