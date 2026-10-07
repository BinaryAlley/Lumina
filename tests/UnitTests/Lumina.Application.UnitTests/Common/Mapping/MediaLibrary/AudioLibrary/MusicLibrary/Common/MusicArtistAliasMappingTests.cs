#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;

/// <summary>
/// Contains unit tests for the <see cref="MusicArtistAliasMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicArtistAliasMappingTests
{
    private readonly MusicArtistAliasFixture _musicArtistAliasFixture = new();

    [Fact]
    public void ToRepositoryEntity_WhenMappingValidMusicArtistAlias_ShouldMapCorrectly()
    {
        // Arrange
        MusicArtistAlias alias = _musicArtistAliasFixture.Create(name: "Freddie Mercury", isPrimary: true);

        // Act
        ArtistAliasEntity result = alias.ToRepositoryEntity();

        // Assert
        Assert.Equal("Freddie Mercury", result.Name);
        Assert.True(result.IsPrimary);
    }

    [Fact]
    public void ToRepositoryEntity_WhenOptionalValuesAreAbsent_ShouldMapThemToNull()
    {
        // Arrange
        MusicArtistAlias alias = _musicArtistAliasFixture.Create(
            sortName: Optional<string>.None(),
            type: Optional<string>.None(),
            locale: Optional<string>.None(),
            beginDate: Optional<System.DateOnly>.None(),
            endDate: Optional<System.DateOnly>.None());

        // Act
        ArtistAliasEntity result = alias.ToRepositoryEntity();

        // Assert
        Assert.Null(result.SortName);
        Assert.Null(result.Type);
        Assert.Null(result.Locale);
        Assert.Null(result.BeginDate);
        Assert.Null(result.EndDate);
    }

    [Fact]
    public void ToRepositoryEntities_WhenMappingMultipleAliases_ShouldMapAllCorrectly()
    {
        // Arrange
        List<MusicArtistAlias> aliases = _musicArtistAliasFixture.CreateMany(2);

        // Act
        List<ArtistAliasEntity> result = aliases.ToRepositoryEntities();

        // Assert
        Assert.Equal(aliases.Count, result.Count);
        for (int index = 0; index < aliases.Count; index++)
            Assert.Equal(aliases[index].Name, result[index].Name);
    }
}
