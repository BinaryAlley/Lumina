#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Nemera.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;

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
