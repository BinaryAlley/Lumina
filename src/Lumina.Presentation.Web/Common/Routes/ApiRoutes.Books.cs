namespace Lumina.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of remote API routes called by this Web application.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the books endpoints of the remote API.
    /// </summary>
    public static class Books
    {
        public const string GET_BOOKS_LITE = "books/lite";
        public const string GET_BOOK_READING_MANIFEST = "books/{bookId}/reading/manifest";
        public const string GET_BOOK_READING_AVAILABILITY = "books/{bookId}/reading/availability";
        public const string GET_BOOK_READING_SECTION = "books/{bookId}/reading/sections/{locationRef}";
        public const string GET_BOOK_READING_RESOURCE = "books/{bookId}/reading/resources/{resourceKey}";
    }
}
