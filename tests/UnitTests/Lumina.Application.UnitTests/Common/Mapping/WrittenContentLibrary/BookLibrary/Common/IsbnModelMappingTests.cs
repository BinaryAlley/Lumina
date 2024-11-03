#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;
using Lumina.Application.UnitTests.Core.WrittenContentLibrary.BooksLibrary.Common.Fixtures;
using Lumina.Contracts.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Contracts.Enums.BookLibrary;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.WrittenContentLibrary.BookLibrary.Common;

/// <summary>
/// Contains unit tests for the <see cref="IsbnEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class IsbnModelMappingTests
{
    private readonly IsbnEntityFixture _isbnEntityFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="IsbnModelMappingTests"/> class.
    /// </summary>
    public IsbnModelMappingTests()
    {
        _isbnEntityFixture = new IsbnEntityFixture();
    }

    [Fact]
    public void ToDomainModel_WhenMappingValidIsbn10Entity_ShouldMapCorrectly()
    {
        // Arrange
        IsbnEntity entity = _isbnEntityFixture.CreateIsbn10();

        // Act
        ErrorOr<Isbn> result = entity.ToDomainEntity();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Value.Should().Be(entity.Value);
        result.Value.Format.Should().Be(entity.Format!.Value);
    }

    [Fact]
    public void ToDomainModel_WhenMappingValidIsbn13Entity_ShouldMapCorrectly()
    {
        // Arrange
        IsbnEntity entity = _isbnEntityFixture.CreateIsbn13();

        // Act
        ErrorOr<Isbn> result = entity.ToDomainEntity();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Value.Should().Be(entity.Value);
        result.Value.Format.Should().Be(entity.Format!.Value);
    }

    [Fact]
    public void ToDomainModel_WhenMappingEntityWithoutFormat_ShouldUseDefaultFormat()
    {
        // Arrange
        IsbnEntity entity = _isbnEntityFixture.CreateWithoutFormat();

        // Act
        ErrorOr<Isbn> result = entity.ToDomainEntity();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Value.Should().Be(entity.Value);
        result.Value.Format.Should().Be(IsbnFormat.Isbn13); // Default format
    }

    [Fact]
    public void ToDomainModel_WhenMappingInvalidIsbnEntity_ShouldReturnError()
    {
        // Arrange
        IsbnEntity entity = _isbnEntityFixture.CreateInvalid();

        // Act
        ErrorOr<Isbn> result = entity.ToDomainEntity();

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public void ToDomainModels_WhenMappingMultipleValidIsbnEntities_ShouldMapAllCorrectly()
    {
        // Arrange
        List<IsbnEntity> entities =
        [
            _isbnEntityFixture.CreateIsbn10(),
            _isbnEntityFixture.CreateIsbn13(),
            _isbnEntityFixture.CreateWithoutFormat()
        ];

        // Act
        IEnumerable<ErrorOr<Isbn>> results = entities.ToDomainEntities();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(entities.Count);

        List<ErrorOr<Isbn>> resultList = results.ToList();
        resultList.Should().AllSatisfy(result => result.IsError.Should().BeFalse());

        resultList[0].Value.Value.Should().Be(entities[0].Value);
        resultList[0].Value.Format.Should().Be(entities[0].Format!.Value);

        resultList[1].Value.Value.Should().Be(entities[1].Value);
        resultList[1].Value.Format.Should().Be(entities[1].Format!.Value);

        resultList[2].Value.Value.Should().Be(entities[2].Value);
        resultList[2].Value.Format.Should().Be(IsbnFormat.Isbn13); // Default format
    }

    [Fact]
    public void ToDomainModels_WhenMappingMixedValidAndInvalidIsbnEntities_ShouldReturnMixedResults()
    {
        // Arrange
        List<IsbnEntity> entities =
        [
            _isbnEntityFixture.CreateIsbn10(),
            _isbnEntityFixture.CreateInvalid(),
            _isbnEntityFixture.CreateIsbn13()
        ];

        // Act
        IEnumerable<ErrorOr<Isbn>> results = entities.ToDomainEntities();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(entities.Count);

        List<ErrorOr<Isbn>> resultList = results.ToList();

        resultList[0].IsError.Should().BeFalse();
        resultList[0].Value.Value.Should().Be(entities[0].Value);

        resultList[1].IsError.Should().BeTrue();

        resultList[2].IsError.Should().BeFalse();
        resultList[2].Value.Value.Should().Be(entities[2].Value);
    }
}
