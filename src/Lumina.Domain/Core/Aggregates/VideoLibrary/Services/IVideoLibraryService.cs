#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.Aggregates.VideoLibrary.MovieLibraryAggregate.Services;
using Lumina.Domain.Core.Aggregates.VideoLibrary.TvShowLibraryAggregate.Services;
#endregion

namespace Lumina.Domain.Core.Aggregates.VideoLibrary.Services;

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