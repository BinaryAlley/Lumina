#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.Aggregates.VideoLibrary.MovieLibraryAggregate.Services;
using Lumina.Domain.Core.Aggregates.VideoLibrary.TvShowLibraryAggregate.Services;
#endregion

namespace Lumina.Domain.Core.Aggregates.VideoLibrary.Services;

/// <summary>
/// Service for managing video content.
/// </summary>
public class VideoLibraryService : IVideoLibraryService
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the service for managing a movie library.
    /// </summary>
    public IMovieLibraryService MovieLibraryService { get; private set; }

    /// <summary>
    /// Gets the service for managing a TV shows library.
    /// </summary>
    public ITvShowLibraryService TvShowLibraryService { get; private set; }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
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
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    #endregion
}