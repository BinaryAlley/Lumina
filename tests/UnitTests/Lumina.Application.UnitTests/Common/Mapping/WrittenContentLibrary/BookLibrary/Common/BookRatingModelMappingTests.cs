#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;
using Lumina.Application.UnitTests.Core.WrittenContentLibrary.BooksLibrary.Common.Fixtures;
using Lumina.Contracts.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.WrittenContentLibrary.BookLibrary.Common;

/// <summary>
/// Contains unit tests for the <see cref="BookRatingEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookRatingModelMappingTests
{
    private readonly BookRatingEntityFixture _bookRatingEntityFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookRatingModelMappingTests"/> class.
    /// </summary>
    public BookRatingModelMappingTests()
    {
        _bookRatingEntityFixture = new BookRatingEntityFixture();
    }

    [Fact]
    public void ToDomainModel_WhenMappingCompleteBookRatingEntity_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        BookRatingEntity entity = _bookRatingEntityFixture.CreateComplete();

        // Act
        ErrorOr<BookRating> result = entity.ToDomainEntity();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Value.Should().Be(entity.Value!.Value);
        result.Value.MaxValue.Should().Be(entity.MaxValue!.Value);

        if (entity.Source.HasValue)
        {
            result.Value.Source.HasValue.Should().BeTrue();
            result.Value.Source.Value.Should().Be(entity.Source.Value);
        }
        else
            result.Value.Source.HasValue.Should().BeFalse();

        if (entity.VoteCount.HasValue)
        {
            result.Value.VoteCount.HasValue.Should().BeTrue();
            result.Value.VoteCount.Value.Should().Be(entity.VoteCount.Value);
        }
        else
            result.Value.VoteCount.HasValue.Should().BeFalse();
    }

    [Fact]
    public void ToDomainModel_WhenMappingMinimalBookRatingEntity_ShouldMapRequiredPropertiesCorrectly()
    {
        // Arrange
        BookRatingEntity entity = _bookRatingEntityFixture.CreateMinimal();

        // Act
        ErrorOr<BookRating> result = entity.ToDomainEntity();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Value.Should().Be(entity.Value!.Value);
        result.Value.MaxValue.Should().Be(entity.MaxValue!.Value);
        result.Value.Source.HasValue.Should().BeFalse();
        result.Value.VoteCount.HasValue.Should().BeFalse();
    }

    [Fact]
    public void ToDomainModel_WhenMappingInvalidBookRatingEntity_ShouldReturnError()
    {
        // Arrange
        BookRatingEntity entity = _bookRatingEntityFixture.CreateInvalid();

        // Act
        ErrorOr<BookRating> result = entity.ToDomainEntity();

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public void ToDomainModels_WhenMappingMultipleValidBookRatingEntities_ShouldMapAllCorrectly()
    {
        // Arrange
        List<BookRatingEntity> entities =
        [
            _bookRatingEntityFixture.CreateComplete(),
            _bookRatingEntityFixture.CreateMinimal()
        ];

        // Act
        IEnumerable<ErrorOr<BookRating>> results = entities.ToDomainEntities();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(entities.Count);

        List<ErrorOr<BookRating>> resultList = results.ToList();
        resultList.Should().AllSatisfy(result => result.IsError.Should().BeFalse());

        // Complete rating
        resultList[0].Value.Value.Should().Be(entities[0].Value!.Value);
        resultList[0].Value.MaxValue.Should().Be(entities[0].MaxValue!.Value);
        resultList[0].Value.Source.Value.Should().Be(entities[0].Source!.Value);
        resultList[0].Value.VoteCount.Value.Should().Be(entities[0].VoteCount!.Value);

        // Minimal rating
        resultList[1].Value.Value.Should().Be(entities[1].Value!.Value);
        resultList[1].Value.MaxValue.Should().Be(entities[1].MaxValue!.Value);
        resultList[1].Value.Source.HasValue.Should().BeFalse();
        resultList[1].Value.VoteCount.HasValue.Should().BeFalse();
    }

    [Fact]
    public void ToDomainModels_WhenMappingMixedValidAndInvalidBookRatingEntities_ShouldReturnMixedResults()
    {
        // Arrange
        List<BookRatingEntity> entities =
        [
            _bookRatingEntityFixture.CreateComplete(),
            _bookRatingEntityFixture.CreateInvalid(),
            _bookRatingEntityFixture.CreateMinimal()
        ];

        // Act
        IEnumerable<ErrorOr<BookRating>> results = entities.ToDomainEntities();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(entities.Count);

        List<ErrorOr<BookRating>> resultList = results.ToList();

        resultList[0].IsError.Should().BeFalse();
        resultList[1].IsError.Should().BeTrue();
        resultList[2].IsError.Should().BeFalse();
    }
}
