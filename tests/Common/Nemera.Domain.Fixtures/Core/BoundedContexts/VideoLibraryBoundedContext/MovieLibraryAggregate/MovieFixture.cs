#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.MovieLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.MovieLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.VideoLibraryBoundedContext.MovieLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.VideoLibraryBoundedContext.MovieLibraryAggregate;

/// <summary>
/// Fixture class for the <see cref="Movie"/> domain aggregate.
/// </summary>
[ExcludeFromCodeCoverage]
public class MovieFixture
{
    private readonly VideoMetadataFixture _videoMetadataFixture = new();
    private readonly MovieIdFixture _movieIdFixture = new();
    private readonly MediaContributorIdFixture _mediaContributorIdFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="Movie"/>.
    /// </summary>
    /// <param name="id">Optional. The unique identifier of the movie.</param>
    /// <param name="metadata">Optional. The metadata of the movie.</param>
    /// <param name="contributors">Optional. The list of media contributors of the movie.</param>
    /// <param name="ratings">Optional. The list of ratings for the movie.</param>
    /// <returns>The created <see cref="Movie"/>.</returns>
    public Movie Create(
        MovieId? id = null,
        VideoMetadata? metadata = null,
        List<MediaContributorId>? contributors = null,
        List<Rating>? ratings = null)
    {
        return Movie.Create(
            id ?? _movieIdFixture.Create(),
            metadata ?? _videoMetadataFixture.Create(),
            contributors ?? [_mediaContributorIdFixture.Create()],
            ratings ?? []).Value;
    }

    /// <summary>
    /// Creates multiple <see cref="Movie"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="Movie"/> instances.</returns>
    public List<Movie> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
