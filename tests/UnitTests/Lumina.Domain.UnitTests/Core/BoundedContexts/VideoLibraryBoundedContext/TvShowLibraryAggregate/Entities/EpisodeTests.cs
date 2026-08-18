#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.ValueObjects;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Domain.UnitTests.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.Entities;

/// <summary>
/// Contains unit tests for the <see cref="Episode"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class EpisodeTests
{
    private readonly EpisodeIdFixture _episodeIdFixture = new();

    [Fact]
    public void Constructor_WhenCalled_ShouldSetId()
    {
        // Arrange
        EpisodeId id = _episodeIdFixture.Create();

        // Act
        Episode episode = new(id);

        // Assert
        Assert.Equal(id, episode.Id);
    }
}
