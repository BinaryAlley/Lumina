#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FluentAssertions;
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.Mapping.Common.Metadata;
using Lumina.Contracts.DTO.Common;
using Lumina.Domain.Common.ValueObjects.Metadata;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Common.Metadata;

/// <summary>
/// Contains unit tests for the <see cref="GenreEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GenreEntityMappingTests
{
    [Fact]
    public void ToResponse_WhenMappingValidGenreEntity_ShouldMapCorrectly()
    {
        // Arrange
        GenreEntity entity = new("Fiction");

        // Act
        GenreDto result = entity.ToResponse();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(entity.Name);
    }

    [Theory]
    [InlineData("Fiction")]
    [InlineData("Science Fiction")]
    [InlineData("Mystery")]
    [InlineData("Romance")]
    public void ToResponse_WhenMappingDifferentValidGenreEntities_ShouldMapCorrectly(string name)
    {
        // Arrange
        GenreEntity entity = new(name);

        // Act
        GenreDto result = entity.ToResponse();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(entity.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ToResponse_WhenMappingInvalidGenreEntity_ShouldMapToDefault(string? invalidName)
    {
        // Arrange
        GenreEntity entity = new(invalidName);

        // Act
        GenreDto result = entity.ToResponse();

        // Assert
        result.Name.Should().Be(invalidName);
    }

    [Fact]
    public void ToDomainEntity_WhenMappingValidGenreEntity_ShouldMapCorrectly()
    {
        // Arrange
        GenreEntity entity = new("Fiction");

        // Act
        ErrorOr<Genre> result = entity.ToDomainEntity();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(entity.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ToDomainEntity_WhenMappingInvalidGenreEntity_ShouldMapToDefault(string? invalidName)
    {
        // Arrange
        GenreEntity entity = new(invalidName);

        // Act
        ErrorOr<Genre> result = entity.ToDomainEntity();

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(Errors.Metadata.GenreNameCannotBeEmpty);
    }

    [Fact]
    public void ToDomainEntities_WhenMappingMultipleValidGenreEntities_ShouldMapAllCorrectly()
    {
        // Arrange
        List<GenreEntity> entities =
        [
            new GenreEntity("Fiction"),
            new GenreEntity("Mystery"),
            new GenreEntity("Romance"),
            new GenreEntity("Thriller")
        ];

        // Act
        IEnumerable<ErrorOr<Genre>> results = entities.ToDomainEntities();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(entities.Count);

        List<ErrorOr<Genre>> resultList = results.ToList();
        for (int i = 0; i < entities.Count; i++)
        {
            resultList[i].IsError.Should().BeFalse();
            resultList[i].Value.Name.Should().Be(entities[i].Name);
        }
    }

    [Fact]
    public void ToResponses_WhenMappingMultipleValidGenreEntities_ShouldMapAllCorrectly()
    {
        // Arrange
        List<GenreEntity> entities =
        [
            new GenreEntity("Fiction"),
            new GenreEntity("Mystery"),
            new GenreEntity("Romance"),
            new GenreEntity("Thriller")
        ];

        // Act
        IEnumerable<GenreDto> results = entities.ToResponses();

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(entities.Count);

        List<GenreDto> resultList = results.ToList();
        for (int i = 0; i < entities.Count; i++)
        {
            resultList[i].Name.Should().Be(entities[i].Name);
        }
    }
}
