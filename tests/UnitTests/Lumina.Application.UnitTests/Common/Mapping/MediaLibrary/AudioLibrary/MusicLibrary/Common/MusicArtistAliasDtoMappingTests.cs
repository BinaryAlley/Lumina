#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;

/// <summary>
/// Contains unit tests for the <see cref="MusicArtistAliasDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicArtistAliasDtoMappingTests
{
    private readonly MusicArtistAliasDtoFixture _musicArtistAliasDtoFixture = new();

    [Fact]
    public void ToDomainValueObject_WhenMappingValidMusicArtistAliasDto_ShouldMapCorrectly()
    {
        // Arrange
        MusicArtistAliasDto dto = _musicArtistAliasDtoFixture.Create(name: "Freddie Mercury", isPrimary: true);

        // Act
        Result<MusicArtistAlias> result = dto.ToDomainValueObject();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("Freddie Mercury", result.Value.Name);
        Assert.True(result.Value.IsPrimary);
    }

    [Theory]
    [InlineData(null)] // missing name
    [InlineData("")] // empty name
    [InlineData("   ")] // whitespace name
    public void ToDomainValueObject_WhenNameIsNullOrWhitespace_ShouldReturnError(string? name)
    {
        // Arrange
        MusicArtistAliasDto dto = _musicArtistAliasDtoFixture.Create(name: name, includeName: name is not null);

        // Act
        Result<MusicArtistAlias> result = dto.ToDomainValueObject();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistAliasNameCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void ToDomainValueObjects_WhenMappingMultipleValidAliasDtos_ShouldMapAllCorrectly()
    {
        // Arrange
        List<MusicArtistAliasDto> dtos = _musicArtistAliasDtoFixture.CreateMany(2);

        // Act
        Result<List<MusicArtistAlias>> result = dtos.ToDomainValueObjects();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(dtos.Count, result.Value.Count);
        for (int index = 0; index < dtos.Count; index++)
            Assert.Equal(dtos[index].Name, result.Value[index].Name);
    }

    [Fact]
    public void ToDomainValueObjects_WhenAliasDtosIsNull_ShouldReturnAnEmptyList()
    {
        // Arrange
        IEnumerable<MusicArtistAliasDto>? dtos = null;

        // Act
        Result<List<MusicArtistAlias>> result = dtos.ToDomainValueObjects();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Empty(result.Value);
    }

    [Fact]
    public void ToDomainValueObjects_WhenAnAliasIsInvalid_ShouldReturnError()
    {
        // Arrange
        List<MusicArtistAliasDto> dtos =
        [
            _musicArtistAliasDtoFixture.Create(name: "Freddie Mercury"),
            _musicArtistAliasDtoFixture.Create(name: string.Empty, includeName: true)
        ];

        // Act
        Result<List<MusicArtistAlias>> result = dtos.ToDomainValueObjects();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistAliasNameCannotBeEmpty, result.FirstError);
    }
}
