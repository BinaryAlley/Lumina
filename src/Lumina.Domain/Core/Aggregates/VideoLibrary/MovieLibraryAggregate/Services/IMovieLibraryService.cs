#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
#endregion

namespace Lumina.Domain.Core.Aggregates.VideoLibrary.MovieLibraryAggregate.Services;

/// <summary>
/// Interface for the service for managing movie libraries.
/// </summary>
public interface IMovieLibraryService
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the collection of movies in the library.
    /// </summary>
    public ICollection<Movie> Movies { get; }
    #endregion
}