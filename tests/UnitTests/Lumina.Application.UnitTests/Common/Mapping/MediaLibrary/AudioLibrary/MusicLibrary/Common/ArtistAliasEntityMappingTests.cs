#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;

/// <summary>
/// Contains unit tests for the <see cref="ArtistAliasEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ArtistAliasEntityMappingTests
{
    private readonly ArtistAliasEntityFixture _artistAliasEntityFixture = new();

    [Fact]
    public void ToDomainValueObject_WhenMappingValidArtistAliasEntity_ShouldMapCorrectly()
    {
        // Arrange
        ArtistAliasEntity entity = _artistAliasEntityFixture.Create(name: "Freddie Mercury", isPrimary: true);

        // Act
        Result<MusicArtistAlias> result = entity.ToDomainValueObject();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal("Freddie Mercury", result.Value.Name);
        Assert.True(result.Value.IsPrimary);
    }

    [Theory]
    [InlineData("")] // empty name
    [InlineData("   ")] // whitespace name
    public void ToDomainValueObject_WhenNameIsEmpty_ShouldReturnError(string name)
    {
        // Arrange
        ArtistAliasEntity entity = _artistAliasEntityFixture.Create(name: name);

        // Act
        Result<MusicArtistAlias> result = entity.ToDomainValueObject();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistAliasNameCannotBeEmpty, result.FirstError);
    }

    [Fact]
    public void ToDomainValueObjects_WhenMappingMultipleValidEntities_ShouldMapAllCorrectly()
    {
        // Arrange
        List<ArtistAliasEntity> entities = _artistAliasEntityFixture.CreateMany(2);

        // Act
        Result<List<MusicArtistAlias>> result = entities.ToDomainValueObjects();

        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(entities.Count, result.Value.Count);
        for (int index = 0; index < entities.Count; index++)
            Assert.Equal(entities[index].Name, result.Value[index].Name);
    }

    [Fact]
    public void ToDomainValueObjects_WhenAnEntityIsInvalid_ShouldReturnError()
    {
        // Arrange
        List<ArtistAliasEntity> entities =
        [
            _artistAliasEntityFixture.Create(name: "Freddie Mercury"),
            _artistAliasEntityFixture.Create(name: string.Empty)
        ];

        // Act
        Result<List<MusicArtistAlias>> result = entities.ToDomainValueObjects();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Errors.Music.ArtistAliasNameCannotBeEmpty, result.FirstError);
    }
}
