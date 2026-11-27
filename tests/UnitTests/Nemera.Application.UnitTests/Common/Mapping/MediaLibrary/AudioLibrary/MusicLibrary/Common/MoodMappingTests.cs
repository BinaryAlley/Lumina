#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Nemera.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Nemera.Domain.Common.ValueObjects.Metadata;
using Nemera.Domain.Fixtures.Common.ValueObjects.Metadata;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;

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
