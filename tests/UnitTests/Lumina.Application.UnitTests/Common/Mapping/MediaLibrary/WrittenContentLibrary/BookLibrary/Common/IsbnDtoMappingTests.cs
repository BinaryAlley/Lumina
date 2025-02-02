#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;
using Lumina.Application.UnitTests.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Common.Fixtures;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Common.Enums.BookLibrary;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
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
    private readonly IsbnDtoFixture _isbnDtoFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="IsbnDtoMappingTests"/> class.
    /// </summary>
    public IsbnDtoMappingTests()
    {
        _isbnDtoFixture = new IsbnDtoFixture();
    }

    [Fact]
    public void ToDomainEntity_WhenMappingValidIsbn10Dto_ShouldMapCorrectly()
    {
        // Arrange
        IsbnDto dto = _isbnDtoFixture.CreateIsbn10();

        // Act
        ErrorOr<Isbn> result = dto.ToDomainEntity();

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Equal(dto.Value, result.Value.Value);
        Assert.Equal(dto.Format!.Value, result.Value.Format);
    }

    [Fact]
    public void ToDomainEntity_WhenMappingValidIsbn13Dto_ShouldMapCorrectly()
    {
        // Arrange
        IsbnDto dto = _isbnDtoFixture.CreateIsbn13();

        // Act
        ErrorOr<Isbn> result = dto.ToDomainEntity();

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Equal(dto.Value, result.Value.Value);
        Assert.Equal(dto.Format!.Value, result.Value.Format);
    }

    [Fact]
    public void ToDomainEntity_WhenMappingDtoWithoutFormat_ShouldUseDefaultFormat()
    {
        // Arrange
        IsbnDto dto = _isbnDtoFixture.CreateWithoutFormat();

        // Act
        ErrorOr<Isbn> result = dto.ToDomainEntity();

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(result.Value);
        Assert.Equal(dto.Value, result.Value.Value);
        Assert.Equal(IsbnFormat.Isbn13, result.Value.Format); // default format
    }

    [Fact]
    public void ToDomainEntity_WhenMappingInvalidIsbnDto_ShouldReturnError()
    {
        // Arrange
        IsbnDto dto = _isbnDtoFixture.CreateInvalid();

        // Act
        ErrorOr<Isbn> result = dto.ToDomainEntity();

        // Assert
        Assert.True(result.IsError);
    }

    [Fact]
    public void ToDomainModels_WhenMappingMultipleValidIsbnDtos_ShouldMapAllCorrectly()
    {
        // Arrange
        List<IsbnDto> dtos =
        [
            _isbnDtoFixture.CreateIsbn10(),
            _isbnDtoFixture.CreateIsbn13(),
            _isbnDtoFixture.CreateWithoutFormat()
        ];

        // Act
        IEnumerable<ErrorOr<Isbn>> results = dtos.ToDomainEntities();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(dtos.Count, results.Count());

        List<ErrorOr<Isbn>> resultList = results.ToList();
        Assert.All(resultList, result => Assert.False(result.IsError));

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
            _isbnDtoFixture.CreateIsbn10(),
            _isbnDtoFixture.CreateInvalid(),
            _isbnDtoFixture.CreateIsbn13()
        ];

        // Act
        IEnumerable<ErrorOr<Isbn>> results = dtos.ToDomainEntities();

        // Assert
        Assert.NotNull(results);
        Assert.Equal(dtos.Count, results.Count());

        List<ErrorOr<Isbn>> resultList = results.ToList();

        Assert.False(resultList[0].IsError);
        Assert.Equal(dtos[0].Value, resultList[0].Value.Value);

        Assert.True(resultList[1].IsError);

        Assert.False(resultList[2].IsError);
        Assert.Equal(dtos[2].Value, resultList[2].Value.Value);
    }
}
