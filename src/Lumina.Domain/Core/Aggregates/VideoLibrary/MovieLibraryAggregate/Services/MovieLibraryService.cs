namespace Lumina.Domain.Core.Aggregates.VideoLibrary.MovieLibraryAggregate.Services;

public sealed class MovieLibraryService : IMovieLibraryService
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the collection of movies in the library.
    /// </summary>
    public ICollection<Movie> Movies
    {
        get { throw new NotImplementedException(); }
    }
    #endregion
}