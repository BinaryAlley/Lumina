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
        /// <summary>
        /// The route for the API endpoint that retrieves the lightweight list of books of a library.
        /// </summary>
        public const string GET_BOOKS_LITE = "libraries/{libraryId}/books/lite";

        /// <summary>
        /// The route for the API endpoint that retrieves a book of a library.
        /// </summary>
        public const string GET_BOOK = "libraries/{libraryId}/books/{bookId}";

        /// <summary>
        /// The route for the API endpoint that updates a book of a library.
        /// </summary>
        public const string UPDATE_BOOK = "libraries/{libraryId}/books/{bookId}";

        /// <summary>
        /// The route for the API endpoint that deletes a book of a library.
        /// </summary>
        public const string DELETE_BOOK = "libraries/{libraryId}/books/{bookId}";

        /// <summary>
        /// The route for the API endpoint that retrieves the deleted books of a library.
        /// </summary>
        public const string GET_DELETED_BOOKS = "libraries/{libraryId}/books/deleted-items";

        /// <summary>
        /// The route for the API endpoint that restores a deleted book of a library.
        /// </summary>
        public const string RESTORE_BOOK = "libraries/{libraryId}/books/{bookId}/restore";

        /// <summary>
        /// The route for the API endpoint that updates the cover of a book of a library.
        /// </summary>
        public const string UPDATE_BOOK_COVER = "libraries/{libraryId}/books/{bookId}/cover";

        /// <summary>
        /// The route for the API endpoint that retrieves the reading manifest of a book.
        /// </summary>
        public const string GET_BOOK_READING_MANIFEST = "libraries/{libraryId}/books/{bookId}/reading/manifest";

        /// <summary>
        /// The route for the API endpoint that retrieves the reading availability of a book.
        /// </summary>
        public const string GET_BOOK_READING_AVAILABILITY = "libraries/{libraryId}/books/{bookId}/reading/availability";

        /// <summary>
        /// The route for the API endpoint that retrieves a reading section of a book.
        /// </summary>
        public const string GET_BOOK_READING_SECTION = "libraries/{libraryId}/books/{bookId}/reading/sections/{locationRef}";

        /// <summary>
        /// The route for the API endpoint that retrieves a reading resource of a book.
        /// </summary>
        public const string GET_BOOK_READING_RESOURCE = "libraries/{libraryId}/books/{bookId}/reading/resources/{resourceKey}";
    }
}
