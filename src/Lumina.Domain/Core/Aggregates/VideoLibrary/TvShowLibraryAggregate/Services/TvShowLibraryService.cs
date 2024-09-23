#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.Aggregates.VideoLibrary.TvShowLibraryAggregate.Services;

/// <summary>
/// Service for managing TV show libraries.
/// </summary>
public class TvShowLibraryService : ITvShowLibraryService
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the collection of TV shows in the library.
    /// </summary>
    public ICollection<TvShow> TvShows => throw new NotImplementedException();
    #endregion
}