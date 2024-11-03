#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.MovieLibraryAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.Services;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.Services;

/// <summary>
/// Interface for the service for managing video content.
/// </summary>
public interface IVideoLibraryService
{
    /// <summary>
    /// Gets the service for managing a movie library.
    /// </summary>
    public IMovieLibraryService MovieLibraryService { get; }

    /// <summary>
    /// Gets the service for managing a TV shows library.
    /// </summary>
    public ITvShowLibraryService TvShowLibraryService { get; }
}
