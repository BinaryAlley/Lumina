#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Nemera.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;

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
    [InlineData("")] // Empty name.
    [InlineData("   ")] // Whitespace name.
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
