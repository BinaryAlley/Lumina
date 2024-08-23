namespace Lumina.Domain.Core.Aggregates.VideoLibrary.MovieLibraryAggregate.Services;

/// <summary>
/// Service for managing movie libraries.
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