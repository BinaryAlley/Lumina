#region ========================================================================= USING =====================================================================================
using ErrorOr;
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
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Equal(dto.Value!.Value, result.Value.Value);
        Assert.Equal(dto.MaxValue!.Value, result.Value.MaxValue);

        if (dto.Source.HasValue)
        {
            Assert.True(result.Value.Source.HasValue);
            Assert.Equal(dto.Source.Value, result.Value.Source.Value);
        }
        else
            Assert.False(result.Value.Source.HasValue);

        if (dto.VoteCount.HasValue)
        {
            Assert.True(result.Value.VoteCount.HasValue);
            Assert.Equal(dto.VoteCount.Value, result.Value.VoteCount.Value);
        }
        else
            Assert.False(result.Value.VoteCount.HasValue);
    }

    [Fact]
    public void ToDomainEntity_WhenMappingMinimalBookRatingDto_ShouldMapRequiredPropertiesCorrectly()
    {
        // Arrange
        BookRatingDto dto = _bookRatingDtoFixture.CreateMinimal();

        // Act
        ErrorOr<BookRating> result = dto.ToDomainEntity();

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Equal(dto.Value!.Value, result.Value.Value);
        Assert.Equal(dto.MaxValue!.Value, result.Value.MaxValue);
        Assert.False(result.Value.Source.HasValue);
        Assert.False(result.Value.VoteCount.HasValue);
    }

    [Fact]
    public void ToDomainEntity_WhenMappingInvalidBookRatingDto_ShouldReturnError()
    {
        // Arrange
        BookRatingDto dto = _bookRatingDtoFixture.CreateInvalid();

        // Act
        ErrorOr<BookRating> result = dto.ToDomainEntity();

        // Assert
        Assert.True(result.IsError);
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
        Assert.NotNull(results);
        Assert.Equal(dtos.Count, results.Count());

        List<ErrorOr<BookRating>> resultList = results.ToList();
        foreach (ErrorOr<BookRating> result in resultList)
            Assert.False(result.IsError);

        // complete rating
        Assert.Equal(dtos[0].Value!.Value, resultList[0].Value.Value);
        Assert.Equal(dtos[0].MaxValue!.Value, resultList[0].Value.MaxValue);
        Assert.Equal(dtos[0].Source!.Value, resultList[0].Value.Source.Value);
        Assert.Equal(dtos[0].VoteCount!.Value, resultList[0].Value.VoteCount.Value);

        // minimal rating
        Assert.Equal(dtos[1].Value!.Value, resultList[1].Value.Value);
        Assert.Equal(dtos[1].MaxValue!.Value, resultList[1].Value.MaxValue);
        Assert.False(resultList[1].Value.Source.HasValue);
        Assert.False(resultList[1].Value.VoteCount.HasValue);
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
        Assert.NotNull(results);
        Assert.Equal(dtos.Count, results.Count());

        List<ErrorOr<BookRating>> resultList = results.ToList();

        Assert.False(resultList[0].IsError);
        Assert.True(resultList[1].IsError);
        Assert.False(resultList[2].IsError);
    }
}
