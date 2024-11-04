#region ========================================================================= USING =====================================================================================
using FluentAssertions;
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
        result.Should().NotBeNull();
        result.Value.Should().Be(bookRating.Value);
        result.MaxValue.Should().Be(bookRating.MaxValue);
        result.Source.Should().Be(bookRating.Source.Value);
        result.VoteCount.Should().Be(bookRating.VoteCount.Value);
    }

    [Fact]
    public void ToRepositoryEntity_WhenMappingBookRatingWithoutSource_ShouldMapCorrectly()
    {
        // Arrange
        BookRating bookRating = _bookRatingFixture.CreateBookRatingWithoutSource();

        // Act
        BookRatingEntity result = bookRating.ToRepositoryEntity();

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(bookRating.Value);
        result.MaxValue.Should().Be(bookRating.MaxValue);
        result.Source.Should().BeNull();
        result.VoteCount.Should().Be(bookRating.VoteCount.Value);
    }

    [Fact]
    public void ToRepositoryEntity_WhenMappingBookRatingWithoutVoteCount_ShouldMapCorrectly()
    {
        // Arrange
        BookRating bookRating = _bookRatingFixture.CreateBookRatingWithoutVoteCount();

        // Act
        BookRatingEntity result = bookRating.ToRepositoryEntity();

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(bookRating.Value);
        result.MaxValue.Should().Be(bookRating.MaxValue);
        result.Source.Should().Be(bookRating.Source.Value);
        result.VoteCount.Should().BeNull();
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
        results.Should().NotBeNull();
        results.Should().HaveCount(bookRatings.Count);

        List<BookRatingEntity> resultList = results.ToList();

        // Complete rating
        resultList[0].Value.Should().Be(bookRatings[0].Value);
        resultList[0].MaxValue.Should().Be(bookRatings[0].MaxValue);
        resultList[0].Source.Should().Be(bookRatings[0].Source.Value);
        resultList[0].VoteCount.Should().Be(bookRatings[0].VoteCount.Value);

        // Rating without source
        resultList[1].Value.Should().Be(bookRatings[1].Value);
        resultList[1].MaxValue.Should().Be(bookRatings[1].MaxValue);
        resultList[1].Source.Should().BeNull();
        resultList[1].VoteCount.Should().Be(bookRatings[1].VoteCount.Value);

        // Rating without vote count
        resultList[2].Value.Should().Be(bookRatings[2].Value);
        resultList[2].MaxValue.Should().Be(bookRatings[2].MaxValue);
        resultList[2].Source.Should().Be(bookRatings[2].Source.Value);
        resultList[2].VoteCount.Should().BeNull();
    }
}
