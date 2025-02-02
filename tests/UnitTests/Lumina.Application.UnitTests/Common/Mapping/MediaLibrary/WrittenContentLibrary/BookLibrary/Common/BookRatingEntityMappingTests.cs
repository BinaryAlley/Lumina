#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;
using Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Common.Fixtures;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;

/// <summary>
/// Contains unit tests for the <see cref="BookRatingEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookRatingEntityMappingTests
{
    private readonly BookRatingEntityFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookRatingEntityMappingTests"/> class.
    /// </summary>
    public BookRatingEntityMappingTests()
    {
        _fixture = new BookRatingEntityFixture();
    }

    [Fact]
    public void ToResponse_WhenMappingValidBookRatingEntity_ShouldMapCorrectly()
    {
        // Arrange
        BookRatingEntity entity = _fixture.CreateBookRating();

        // Act
        BookRatingDto result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Value!.Value, result.Value);
        Assert.Equal(entity.MaxValue!.Value, result.MaxValue);
        Assert.Equal(entity.Source, result.Source);
        Assert.Equal(entity.VoteCount, result.VoteCount);
    }

    [Fact]
    public void ToResponse_WhenMappingInvalidBookRatingEntity_ShouldMapToDefaults()
    {
        // Arrange
        BookRatingEntity entity = _fixture.CreateInvalidBookRating();

        // Act
        BookRatingDto result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Value);
        Assert.Equal(0, result.MaxValue);
        Assert.Equal(entity.Source, result.Source);
        Assert.Equal(entity.VoteCount, result.VoteCount);
    }

    [Fact]
    public void ToResponses_WhenMappingMultipleBookRatingEntities_ShouldMapAllCorrectly()
    {
        // Arrange
        List<BookRatingEntity> entities =
        [
            _fixture.CreateBookRating(),
            _fixture.CreateBookRating()
        ];

        // Act
        IEnumerable<BookRatingDto> results = entities.ToResponses();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(entities.Count, results.Count());
        foreach (BookRatingDto result in results)
        {
            Assert.NotNull(result);
            Assert.InRange(result.Value!.Value, 1, 5);
            Assert.Equal(5, result.MaxValue);
            Assert.InRange(result.VoteCount!.Value, 1, 1000);
        }
    }

    [Fact]
    public void ToDomainEntity_WhenMappingValidBookRatingEntity_ShouldMapCorrectly()
    {
        // Arrange
        BookRatingEntity entity = _fixture.CreateBookRating();

        // Act
        ErrorOr<BookRating> result = entity.ToDomainEntity();

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Equal(entity.Value, result.Value.Value);
        Assert.Equal(entity.MaxValue, result.Value.MaxValue);
        Assert.Equal(entity.Source, result.Value.Source.Value);
        Assert.Equal(entity.VoteCount, result.Value.VoteCount.Value);
    }

    [Fact]
    public void ToDomainEntity_WhenMappingInvalidBookRatingEntity_ShouldMapToDefaults()
    {
        // Arrange
        BookRatingEntity entity = _fixture.CreateInvalidBookRating();

        // Act
        ErrorOr<BookRating> result = entity.ToDomainEntity();

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Equal(default, result.Value.Value);
        Assert.Equal(default, result.Value.MaxValue);
        Assert.False(result.Value.Source.HasValue);
        Assert.False(result.Value.VoteCount.HasValue);
    }

    [Fact]
    public void ToDomainEntities_WhenMappingMultipleBookRatingEntities_ShouldMapAllCorrectly()
    {
        // Arrange
        List<BookRatingEntity> entities =
        [
            _fixture.CreateBookRating(),
            _fixture.CreateBookRating()
        ];

        // Act
        IEnumerable<ErrorOr<BookRating>> results = entities.ToDomainEntities();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(entities.Count, results.Count());
        foreach (ErrorOr<BookRating> result in results)
        {
            Assert.False(result.IsError);
            Assert.NotNull(result.Value);
            Assert.InRange(result.Value.Value, 1, 5);
            Assert.Equal(5, result.Value.MaxValue);
            Assert.True(result.Value.Source.HasValue);
            Assert.InRange(result.Value.VoteCount.Value, 1, 1000);
        }
    }
}
