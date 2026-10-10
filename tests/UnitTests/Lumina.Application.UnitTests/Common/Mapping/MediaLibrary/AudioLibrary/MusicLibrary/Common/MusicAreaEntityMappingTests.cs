#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;

/// <summary>
/// Contains unit tests for the <see cref="MusicAreaEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicAreaEntityMappingTests
{
    private readonly MusicAreaEntityFixture _musicAreaEntityFixture = new();

    [Fact]
    public void ToDomainValueObject_WhenMappingValidMusicAreaEntity_ShouldMapCorrectly()
    {
        // Arrange
        MusicAreaEntity entity = _musicAreaEntityFixture.Create(name: "United Kingdom", iso3166Code: "GB");

        // Act
        Result<MusicArea> result = entity.ToDomainValueObject();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(entity.MusicBrainzAreaId, result.Value.MusicBrainzAreaId.Value);
        Assert.Equal("United Kingdom", result.Value.Name);
        Assert.Equal("GB", result.Value.Iso3166Code.Value);
    }

    [Theory]
    [InlineData("")] // empty name
    [InlineData("   ")] // whitespace name
    public void ToDomainValueObject_WhenNameIsEmpty_ShouldReturnError(string name)
    {
        // Arrange
        MusicAreaEntity entity = _musicAreaEntityFixture.Create(name: name);

        // Act
        Result<MusicArea> result = entity.ToDomainValueObject();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.AreaNameCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void ToResponse_WhenEntityIsNull_ShouldReturnNull()
    {
        // Arrange
        MusicAreaEntity? entity = null;

        // Act
        MusicAreaDto? result = entity.ToResponse();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ToResponse_WhenMappingValidEntity_ShouldMapCorrectly()
    {
        // Arrange
        MusicAreaEntity entity = _musicAreaEntityFixture.Create(name: "United Kingdom", iso3166Code: "GB");

        // Act
        MusicAreaDto? result = entity.ToResponse();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.MusicBrainzAreaId, result!.MusicBrainzAreaId);
        Assert.Equal("United Kingdom", result.Name);
        Assert.Equal("GB", result.Iso3166Code);
    }
}
