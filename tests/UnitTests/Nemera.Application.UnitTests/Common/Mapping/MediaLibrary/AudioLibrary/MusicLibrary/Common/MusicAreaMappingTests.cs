#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;

/// <summary>
/// Contains unit tests for the <see cref="MusicAreaMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicAreaMappingTests
{
    private readonly MusicAreaFixture _musicAreaFixture = new();

    [Fact]
    public void ToRepositoryEntity_WhenMappingValidMusicArea_ShouldMapCorrectly()
    {
        // Arrange
        MusicArea area = _musicAreaFixture.Create(name: "United Kingdom", iso3166Code: Optional<string>.Some("GB"));

        // Act
        MusicAreaEntity result = area.ToRepositoryEntity();

        // Assert
        Assert.Equal(area.MusicBrainzAreaId.Value, result.MusicBrainzAreaId);
        Assert.Equal("United Kingdom", result.Name);
        Assert.Equal("GB", result.Iso3166Code);
    }

    [Fact]
    public void ToRepositoryEntity_WhenOptionalValuesAreAbsent_ShouldMapThemToNull()
    {
        // Arrange
        MusicArea area = _musicAreaFixture.Create(
            sortName: Optional<string>.None(),
            disambiguation: Optional<string>.None(),
            type: Optional<string>.None(),
            iso3166Code: Optional<string>.None());

        // Act
        MusicAreaEntity result = area.ToRepositoryEntity();

        // Assert
        Assert.Null(result.SortName);
        Assert.Null(result.Disambiguation);
        Assert.Null(result.Type);
        Assert.Null(result.Iso3166Code);
    }
}
