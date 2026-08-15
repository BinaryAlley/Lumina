#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Common.Primitives;
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
        BookRatingDto dto = _bookRatingDtoFixture.Create();

        // Act
        Result<BookRating> result = dto.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
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
        BookRatingDto dto = _bookRatingDtoFixture.Create(includeOptionalProperties: false);

        // Act
        Result<BookRating> result = dto.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
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
        BookRatingDto dto = _bookRatingDtoFixture.Create(value: 10, maxValue: 5);

        // Act
        Result<BookRating> result = dto.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ToDomainModels_WhenMappingMultipleValidBookRatingDtos_ShouldMapAllCorrectly()
    {
        // Arrange
        List<BookRatingDto> dtos =
        [
            _bookRatingDtoFixture.Create(),
            _bookRatingDtoFixture.Create(includeOptionalProperties: false)
        ];

        // Act
        IEnumerable<Result<BookRating>> results = dtos.ToDomainEntities();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(dtos.Count, results.Count());

        List<Result<BookRating>> resultList = [.. results];
        foreach (Result<BookRating> result in resultList)
            Assert.False(result.IsFailure);

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
            _bookRatingDtoFixture.Create(),
            _bookRatingDtoFixture.Create(value: 10, maxValue: 5),
            _bookRatingDtoFixture.Create(includeOptionalProperties: false)
        ];

        // Act
        IEnumerable<Result<BookRating>> results = dtos.ToDomainEntities();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(dtos.Count, results.Count());

        List<Result<BookRating>> resultList = [.. results];

        Assert.False(resultList[0].IsFailure);
        Assert.True(resultList[1].IsFailure);
        Assert.False(resultList[2].IsFailure);
    }
}
