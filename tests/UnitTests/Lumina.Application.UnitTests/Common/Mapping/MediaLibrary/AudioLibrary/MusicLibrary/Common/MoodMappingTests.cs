#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Fixtures.Common.ValueObjects.Metadata;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;

/// <summary>
/// Contains unit tests for the <see cref="MoodMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MoodMappingTests
{
    private readonly MoodFixture _moodFixture = new();

    [Fact]
    public void ToRepositoryEntity_WhenMappingMood_ShouldMapCorrectly()
    {
        // Arrange
        Mood mood = _moodFixture.Create();

        // Act
        TrackMoodEntity result = mood.ToRepositoryEntity();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(mood.Name, result.Name);
    }

    [Fact]
    public void ToRepositoryEntities_WhenMappingMultipleMoods_ShouldMapAllCorrectly()
    {
        // Arrange
        List<Mood> moods = _moodFixture.CreateMany(2);

        // Act
        List<TrackMoodEntity> results = [.. moods.ToRepositoryEntities()];

        // Assert
        Assert.Equal(moods.Count, results.Count);
        for (int i = 0; i < moods.Count; i++)
            Assert.Equal(moods[i].Name, results[i].Name);
    }

    [Fact]
    public void ToRepositoryEntities_WhenMappingEmptyCollection_ShouldReturnEmptyCollection()
    {
        // Arrange
        List<Mood> moods = [];

        // Act
        IEnumerable<TrackMoodEntity> results = moods.ToRepositoryEntities();

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }
}
