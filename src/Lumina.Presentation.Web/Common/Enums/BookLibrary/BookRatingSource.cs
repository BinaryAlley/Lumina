namespace Lumina.Presentation.Web.Common.Enums.BookLibrary;

/// <summary>
/// Enumeration for the various sources of book ratings.
/// </summary>
public enum BookRatingSource
{
    /// <summary>
    /// Ratings provided by users of the Lumina system.
    /// </summary>
    User,

    /// <summary>
    /// Ratings sourced from Goodreads, a social network for book readers that provides book recommendations and reviews.
    /// </summary>
    Goodreads,

    /// <summary>
    /// Ratings obtained from Amazon, an e-commerce platform that includes customer reviews and ratings for books.
    /// </summary>
    Amazon,

    /// <summary>
    /// Ratings from Google Books, a service that searches the full text of books and magazines that Google has scanned and converted to text.
    /// </summary>
    GoogleBooks,
}