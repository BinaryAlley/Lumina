#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.Aggregates.VideoLibrary.MovieLibraryAggregate.Services;

/// <summary>
/// Service for managing movie libraries.
/// </summary>
public sealed class MovieLibraryService : IMovieLibraryService
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the collection of movies in the library.
    /// </summary>
    public ICollection<Movie> Movies => throw new NotImplementedException();
    #endregion
}