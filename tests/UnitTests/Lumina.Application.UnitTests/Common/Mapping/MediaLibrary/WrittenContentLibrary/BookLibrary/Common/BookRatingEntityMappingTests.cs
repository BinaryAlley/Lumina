#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;
using Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Common.Fixtures;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        result.Should().NotBeNull();
        result.Value.Should().Be(entity.Value!.Value);
        result.MaxValue.Should().Be(entity.MaxValue!.Value);
        result.Source.Should().Be(entity.Source);
        result.VoteCount.Should().Be(entity.VoteCount);
    }

    [Fact]
    public void ToResponse_WhenMappingInvalidBookRatingEntity_ShouldMapToDefaults()
    {
        // Arrange
        BookRatingEntity entity = _fixture.CreateInvalidBookRating();

        // Act
        BookRatingDto result = entity.ToResponse();

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(default);
        result.MaxValue.Should().Be(default);
        result.Source.Should().Be(entity.Source);
        result.VoteCount.Should().Be(entity.VoteCount);
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
        results.Should().NotBeNull();
        results.Should().HaveCount(entities.Count);
        results.Should().AllSatisfy(result =>
        {
            result.Should().NotBeNull();
            result.Value.Should().BeInRange(1, 5);
            result.MaxValue.Should().Be(5);
            result.VoteCount.Should().BeInRange(1, 1000);
        });
    }

    [Fact]
    public void ToDomainEntity_WhenMappingValidBookRatingEntity_ShouldMapCorrectly()
    {
        // Arrange
        BookRatingEntity entity = _fixture.CreateBookRating();

        // Act
        ErrorOr<BookRating> result = entity.ToDomainEntity();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Value.Should().Be(entity.Value);
        result.Value.MaxValue.Should().Be(entity.MaxValue);
        result.Value.Source.Value.Should().Be(entity.Source);
        result.Value.VoteCount.Value.Should().Be(entity.VoteCount);
    }

    [Fact]
    public void ToDomainEntity_WhenMappingInvalidBookRatingEntity_ShouldMapToDefaults()
    {
        // Arrange
        BookRatingEntity entity = _fixture.CreateInvalidBookRating();

        // Act
        ErrorOr<BookRating> result = entity.ToDomainEntity();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Value.Should().Be(default);
        result.Value.MaxValue.Should().Be(default);
        result.Value.Source.HasValue.Should().BeFalse();
        result.Value.VoteCount.HasValue.Should().BeFalse();
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
        results.Should().NotBeNull();
        results.Should().HaveCount(entities.Count);
        results.Should().AllSatisfy(result =>
        {
            result.IsError.Should().BeFalse();
            result.Value.Should().NotBeNull();
            result.Value.Value.Should().BeInRange(1, 5);
            result.Value.MaxValue.Should().Be(5);
            result.Value.Source.HasValue.Should().BeTrue();
            result.Value.VoteCount.Value.Should().BeInRange(1, 1000);
        });
    }
}
