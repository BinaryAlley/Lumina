#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.ValueObjects.Metadata;
using Lumina.Domain.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.MovieLibraryAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.MovieLibraryAggregate;

/// <summary>
/// Entity for a movie.
/// </summary>
[DebuggerDisplay("{Id}: {Title}")]
public sealed class Movie : Entity<MovieId>
{
    private readonly List<MediaContributorId> _contributors;
    private readonly List<Rating> _ratings;

    /// <summary>
    /// Gets the video metadata of the movie.
    /// </summary>
    public VideoMetadata Metadata { get; private set; }

    /// <summary>
    /// Gets the list of ratings for this movie.
    /// </summary>
    public IReadOnlyCollection<Rating> Ratings => _ratings.AsReadOnly();

    /// <summary>
    /// Gets the list of media contributors (actors, directors, etc) starring in this movie.
    /// </summary>
    public IReadOnlyCollection<MediaContributorId> Contributors => _contributors.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="Movie"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the movie.</param>
    /// <param name="metadata">The metadata of the movie.</param>
    /// <param name="contributors">The list of the contributors of the movie.</param>
    /// <param name="ratings">The list of ratings for the movie.</param>
    public Movie(MovieId id, VideoMetadata metadata, List<MediaContributorId> contributors, List<Rating> ratings) : base(id)
    {
        Id = id;
        Metadata = metadata;
        _contributors = contributors;
        _ratings = ratings;
    }

#pragma warning disable CS8618
    /// <summary>
    /// Initializes a new instance of the <see cref="Movie"/> class.
    /// </summary>
    private Movie() // only needed during reflection
    {

    }
#pragma warning restore CS8618

    /// <summary>
    /// Creates a new instance of the <see cref="Movie"/> class.
    /// </summary>
    /// <param name="metadata">The metadata of the movie.</param>
    /// <param name="contributors">The list of media contributors of the movie.</param>
    /// <param name="ratings">The list of ratings for the movie.</param>
    /// <returns>The created <see cref="Movie"/>.</returns>
    public static ErrorOr<Movie> Create(VideoMetadata metadata, List<MediaContributorId> contributors, List<Rating> ratings)
    {
        // TODO: enforce invariants
        return new Movie(MovieId.CreateUnique(), metadata, contributors, ratings);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Movie"/> class, with a pre-existing <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The object representing the id of the movie.</param>
    /// <param name="metadata">The metadata of the movie.</param>
    /// <param name="contributors">The list of media contributors of the movie.</param>
    /// <param name="ratings">The list of ratings for the movie.</param>
    /// <returns>The created <see cref="Movie"/>.</returns>
    public static ErrorOr<Movie> Create(MovieId id, VideoMetadata metadata, List<MediaContributorId> contributors, List<Rating> ratings)
    {
        // TODO: enforce invariants
        return new Movie(id, metadata, contributors, ratings);
    }
}
