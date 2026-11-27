#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Common;

/// <summary>
/// Contains unit tests for the <see cref="IsrcMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class IsrcMappingTests
{
    private readonly IsrcFixture _isrcFixture = new();

    [Fact]
    public void ToRepositoryEntity_WhenMappingIsrc_ShouldMapCorrectly()
    {
        // Arrange
        Isrc isrc = _isrcFixture.Create();

        // Act
        TrackIsrcEntity result = isrc.ToRepositoryEntity();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(isrc.Value, result.Value);
    }

    [Fact]
    public void ToRepositoryEntities_WhenMappingMultipleIsrcs_ShouldMapAllCorrectly()
    {
        // Arrange
        List<Isrc> isrcs = _isrcFixture.CreateMany(2);

        // Act
        List<TrackIsrcEntity> results = [.. isrcs.ToRepositoryEntities()];

        // Assert
        Assert.Equal(isrcs.Count, results.Count);
        for (int i = 0; i < isrcs.Count; i++)
            Assert.Equal(isrcs[i].Value, results[i].Value);
    }

    [Fact]
    public void ToRepositoryEntities_WhenMappingEmptyCollection_ShouldReturnEmptyCollection()
    {
        // Arrange
        List<Isrc> isrcs = [];

        // Act
        IEnumerable<TrackIsrcEntity> results = isrcs.ToRepositoryEntities();

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }
}
