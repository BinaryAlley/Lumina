#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.MovieLibraryAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.Services;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.Services;

/// <summary>
/// Service for managing video content.
/// </summary>
public class VideoLibraryService : IVideoLibraryService
{
    /// <summary>
    /// Gets the service for managing a movie library.
    /// </summary>
    public IMovieLibraryService MovieLibraryService { get; private set; }

    /// <summary>
    /// Gets the service for managing a TV shows library.
    /// </summary>
    public ITvShowLibraryService TvShowLibraryService { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VideoLibraryService"/> class.
    /// </summary>
    /// <param name="movieLibraryService">Injected movie library service.</param>
    /// <param name="tvShowLibraryService">Injected TV show library service.</param>
    public VideoLibraryService(IMovieLibraryService movieLibraryService, ITvShowLibraryService tvShowLibraryService)
    {
        MovieLibraryService = movieLibraryService;
        TvShowLibraryService = tvShowLibraryService;
    }
}
