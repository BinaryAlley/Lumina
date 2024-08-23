namespace Lumina.Domain.Core.Aggregates.VideoLibrary.TvShowLibraryAggregate.Services
{
    /// <summary>
    /// Interface for the service for managing TV show libraries.
    /// </summary>
    public interface ITvShowLibraryService
    {
        #region ==================================================================== PROPERTIES =================================================================================
        /// <summary>
        /// Gets the collection of TV shows in the library.
        /// </summary>
        public ICollection<TvShow> TvShows { get; }
        #endregion
    }
}