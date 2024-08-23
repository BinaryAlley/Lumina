namespace Lumina.Domain.Common.Enums;

/// <summary>
/// Enumeration for the various sources of video ratings.
/// </summary>
public enum VideoRatingSource
{
    /// <summary>
    /// Ratings provided by users of the Lumina media library.
    /// </summary>
    User,

    /// <summary>
    /// Ratings sourced from IMDb (Internet Movie Database), an online database of information related to films, TV programs, and video games.
    /// </summary>
    Imdb,

    /// <summary>
    /// Ratings obtained from TheTVDB, an open database for television shows, including episode information and user ratings.
    /// </summary>
    TheTvDb,

    /// <summary>
    /// Ratings from The Movie Database (TMDb), a community-built movie and TV database.
    /// </summary>
    TheMovieDb,

    /// <summary>
    /// Ratings from Rotten Tomatoes, a review-aggregation website for film and television.
    /// </summary>
    RottenTomatoes,
}