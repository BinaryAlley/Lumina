#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
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
/// Contains unit tests for the <see cref="BookRatingDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookRatingDtoMappingTests
{
    private readonly BookRatingDtoFixture _bookRatingDtoFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookRatingDtoMappingTests"/> class.
    /// </summary>
    public BookRatingDtoMappingTests()
    {
        _bookRatingDtoFixture = new BookRatingDtoFixture();
    }

    [Fact]
    public void ToDomainEntity_WhenMappingCompleteBookRatinDto_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        BookRatingDto dto = _bookRatingDtoFixture.CreateComplete();

        // Act
        ErrorOr<BookRating> result = dto.ToDomainEntity();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Value.Should().Be(dto.Value!.Value);
        result.Value.MaxValue.Should().Be(dto.MaxValue!.Value);

        if (dto.Source.HasValue)
        {
            result.Value.Source.HasValue.Should().BeTrue();
            result.Value.Source.Value.Should().Be(dto.Source.Value);
        }
        else
            result.Value.Source.HasValue.Should().BeFalse();

        if (dto.VoteCount.HasValue)
        {
            result.Value.VoteCount.HasValue.Should().BeTrue();
            result.Value.VoteCount.Value.Should().Be(dto.VoteCount.Value);
        }
        else
            result.Value.VoteCount.HasValue.Should().BeFalse();
    }

    [Fact]
    public void ToDomainEntity_WhenMappingMinimalBookRatingDto_ShouldMapRequiredPropertiesCorrectly()
    {
        // Arrange
        BookRatingDto dto = _bookRatingDtoFixture.CreateMinimal();

        // Act
        ErrorOr<BookRating> result = dto.ToDomainEntity();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Value.Should().Be(dto.Value!.Value);
        result.Value.MaxValue.Should().Be(dto.MaxValue!.Value);
        result.Value.Source.HasValue.Should().BeFalse();
        result.Value.VoteCount.HasValue.Should().BeFalse();
    }

    [Fact]
    public void ToDomainEntity_WhenMappingInvalidBookRatingDto_ShouldReturnError()
    {
        // Arrange
        BookRatingDto dto = _bookRatingDtoFixture.CreateInvalid();

        // Act
        ErrorOr<BookRating> result = dto.ToDomainEntity();

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public void ToDomainModels_WhenMappingMultipleValidBookRatingDtos_ShouldMapAllCorrectly()
    {
        // Arrange
        List<BookRatingDto> dtos =
        [
            _bookRatingDtoFixture.CreateComplete(),
            _bookRatingDtoFixture.CreateMinimal()
        ];

        // Act
        IEnumerable<ErrorOr<BookRating>> results = dtos.ToDomainEntities();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(dtos.Count);

        List<ErrorOr<BookRating>> resultList = results.ToList();
        resultList.Should().AllSatisfy(result => result.IsError.Should().BeFalse());

        // Complete rating
        resultList[0].Value.Value.Should().Be(dtos[0].Value!.Value);
        resultList[0].Value.MaxValue.Should().Be(dtos[0].MaxValue!.Value);
        resultList[0].Value.Source.Value.Should().Be(dtos[0].Source!.Value);
        resultList[0].Value.VoteCount.Value.Should().Be(dtos[0].VoteCount!.Value);

        // Minimal rating
        resultList[1].Value.Value.Should().Be(dtos[1].Value!.Value);
        resultList[1].Value.MaxValue.Should().Be(dtos[1].MaxValue!.Value);
        resultList[1].Value.Source.HasValue.Should().BeFalse();
        resultList[1].Value.VoteCount.HasValue.Should().BeFalse();
    }

    [Fact]
    public void ToDomainModels_WhenMappingMixedValidAndInvalidBookRatingDtos_ShouldReturnMixedResults()
    {
        // Arrange
        List<BookRatingDto> dtos =
        [
            _bookRatingDtoFixture.CreateComplete(),
            _bookRatingDtoFixture.CreateInvalid(),
            _bookRatingDtoFixture.CreateMinimal()
        ];

        // Act
        IEnumerable<ErrorOr<BookRating>> results = dtos.ToDomainEntities();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(dtos.Count);

        List<ErrorOr<BookRating>> resultList = results.ToList();

        resultList[0].IsError.Should().BeFalse();
        resultList[1].IsError.Should().BeTrue();
        resultList[2].IsError.Should().BeFalse();
    }
}
