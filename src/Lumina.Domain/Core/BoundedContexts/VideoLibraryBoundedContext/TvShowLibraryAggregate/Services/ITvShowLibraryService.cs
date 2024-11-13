#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.VideoLibraryBoundedContext.TvShowLibraryAggregate.Services;

/// <summary>
/// Interface for the service for managing TV show libraries.
/// </summary>
public interface ITvShowLibraryService
{
    /// <summary>
    /// Gets the collection of TV shows in the library.
    /// </summary>
    public ICollection<TvShow> TvShows { get; }
}
