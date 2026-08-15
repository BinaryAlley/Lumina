#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;

/// <summary>
/// Contains unit tests for the <see cref="IsbnDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class IsbnDtoMappingTests
{
    private readonly IsbnDtoFixture _isbnDtoFixture = new();

    [Fact]
    public void ToDomainEntity_WhenMappingValidIsbn10Dto_ShouldMapCorrectly()
    {
        // Arrange
        IsbnDto dto = _isbnDtoFixture.Create(format: IsbnFormat.Isbn10);

        // Act
        Result<Isbn> result = dto.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(dto.Value, result.Value.Value);
        Assert.Equal(dto.Format!.Value, result.Value.Format);
    }

    [Fact]
    public void ToDomainEntity_WhenMappingValidIsbn13Dto_ShouldMapCorrectly()
    {
        // Arrange
        IsbnDto dto = _isbnDtoFixture.Create(format: IsbnFormat.Isbn13);

        // Act
        Result<Isbn> result = dto.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(dto.Value, result.Value.Value);
        Assert.Equal(dto.Format!.Value, result.Value.Format);
    }

    [Fact]
    public void ToDomainEntity_WhenMappingDtoWithoutFormat_ShouldUseDefaultFormat()
    {
        // Arrange
        IsbnDto dto = _isbnDtoFixture.Create();

        // Act
        Result<Isbn> result = dto.ToDomainEntity();

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(dto.Value, result.Value.Value);
        Assert.Equal(IsbnFormat.Isbn13, result.Value.Format); // default format
    }

    [Fact]
    public void ToDomainEntity_WhenMappingInvalidIsbnDto_ShouldReturnError()
    {
        // Arrange
        IsbnDto dto = _isbnDtoFixture.Create("invalid-isbn", IsbnFormat.Isbn13);

        // Act
        Result<Isbn> result = dto.ToDomainEntity();

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ToDomainModels_WhenMappingMultipleValidIsbnDtos_ShouldMapAllCorrectly()
    {
        // Arrange
        List<IsbnDto> dtos =
        [
            _isbnDtoFixture.Create(format: IsbnFormat.Isbn10),
            _isbnDtoFixture.Create(format: IsbnFormat.Isbn13),
            _isbnDtoFixture.Create(),
        ];

        // Act
        IEnumerable<Result<Isbn>> results = dtos.ToDomainEntities();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(dtos.Count, results.Count());

        List<Result<Isbn>> resultList = [.. results];
        Assert.All(resultList, result => Assert.False(result.IsFailure));

        Assert.Equal(dtos[0].Value, resultList[0].Value.Value);
        Assert.Equal(dtos[0].Format!.Value, resultList[0].Value.Format);

        Assert.Equal(dtos[1].Value, resultList[1].Value.Value);
        Assert.Equal(dtos[1].Format!.Value, resultList[1].Value.Format);

        Assert.Equal(dtos[2].Value, resultList[2].Value.Value);
        Assert.Equal(IsbnFormat.Isbn13, resultList[2].Value.Format); // default format
    }

    [Fact]
    public void ToDomainModels_WhenMappingMixedValidAndInvalidIsbnDtos_ShouldReturnMixedResults()
    {
        // Arrange
        List<IsbnDto> dtos =
        [
            _isbnDtoFixture.Create(format: IsbnFormat.Isbn10),
            _isbnDtoFixture.Create("invalid-isbn", IsbnFormat.Isbn13),
            _isbnDtoFixture.Create(format: IsbnFormat.Isbn13)
        ];

        // Act
        IEnumerable<Result<Isbn>> results = dtos.ToDomainEntities();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(dtos.Count, results.Count());

        List<Result<Isbn>> resultList = [.. results];

        Assert.False(resultList[0].IsFailure);
        Assert.Equal(dtos[0].Value, resultList[0].Value.Value);

        Assert.True(resultList[1].IsFailure);

        Assert.False(resultList[2].IsFailure);
        Assert.Equal(dtos[2].Value, resultList[2].Value.Value);
    }
}
