#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Domain.Common.Enums.BookLibrary;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Common;

/// <summary>
/// Contains unit tests for the <see cref="IsbnEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class IsbnEntityMappingTests
{
    [Fact]
    public void ToResponse_WhenMappingValidIsbnEntity_ShouldMapCorrectly()
    {
        // Arrange
        IsbnEntity entity = new("0-7475-3269-9", IsbnFormat.Isbn10);

        // Act  
        IsbnDto result = entity.ToResponse();

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(entity.Value);
        result.Format.Should().Be(entity.Format);
    }

    [Theory]
    [InlineData("0-7475-3269-9", IsbnFormat.Isbn10)]
    [InlineData("978-0-7475-3269-9", IsbnFormat.Isbn13)]
    public void ToResponse_WhenMappingDifferentValidIsbnEntities_ShouldMapCorrectly(string value, IsbnFormat format)
    {
        // Arrange
        IsbnEntity entity = new(value, format);

        // Act
        IsbnDto result = entity.ToResponse();

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(value);
        result.Format.Should().Be(format);
    }

    [Theory]
    [InlineData("", null)]
    [InlineData(" ", null)]
    [InlineData(null, null)]
    public void ToResponse_WhenMappingInvalidIsbnEntity_ShouldPreserveValues(string? value, IsbnFormat? format)
    {
        // Arrange
        IsbnEntity entity = new(value, format);

        // Act
        IsbnDto result = entity.ToResponse();

        // Assert
        result.Value.Should().Be(value);
        result.Format.Should().Be(format);
    }

    [Fact]
    public void ToDomainEntity_WhenMappingValidIsbnEntity_ShouldMapCorrectly()
    {
        // Arrange
        IsbnEntity entity = new("0-7475-3269-9", IsbnFormat.Isbn10);

        // Act
        ErrorOr<Isbn> result = entity.ToDomainEntity();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Value.Should().Be(entity.Value);
        result.Value.Format.Should().Be(entity.Format);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ToDomainEntity_WhenMappingInvalidIsbnEntity_ShouldReturnError(string? invalidValue)
    {
        // Arrange
        IsbnEntity entity = new(invalidValue, null);

        // Act
        ErrorOr<Isbn> result = entity.ToDomainEntity();

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public void ToDomainEntities_WhenMappingMultipleValidIsbnEntities_ShouldMapAllCorrectly()
    {
        // Arrange
        List<IsbnEntity> entities =
        [
            new IsbnEntity("0-7475-3269-9", IsbnFormat.Isbn10),
            new IsbnEntity("978-0-7475-3269-9", IsbnFormat.Isbn13)
        ];

        // Act
        IEnumerable<ErrorOr<Isbn>> results = entities.ToDomainEntities();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(entities.Count);

        List<ErrorOr<Isbn>> resultList = results.ToList();
        for (int i = 0; i < entities.Count; i++)
        {
            resultList[i].IsError.Should().BeFalse();
            resultList[i].Value.Value.Should().Be(entities[i].Value);
            resultList[i].Value.Format.Should().Be(entities[i].Format);
        }
    }

    [Fact]
    public void ToResponses_WhenMappingMultipleValidIsbnEntities_ShouldMapAllCorrectly()
    {
        // Arrange
        List<IsbnEntity> entities =
        [
            new IsbnEntity("0-7475-3269-9", IsbnFormat.Isbn10),
            new IsbnEntity("978-0-7475-3269-9", IsbnFormat.Isbn13)
        ];

        // Act
        IEnumerable<IsbnDto> results = entities.ToResponses();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(entities.Count);

        List<IsbnDto> resultList = results.ToList();
        for (int i = 0; i < entities.Count; i++)
        {
            resultList[i].Value.Should().Be(entities[i].Value);
            resultList[i].Format.Should().Be(entities[i].Format);
        }
    }
}
