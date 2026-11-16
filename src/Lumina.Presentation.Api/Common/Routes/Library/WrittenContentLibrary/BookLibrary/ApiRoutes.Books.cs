namespace Lumina.Presentation.Api.Common.Routes.Library.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Class for the collection of routes defined in this API.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the Books route.
    /// </summary>
    public static class Books
    {
        /// <summary>
        /// The route for getting a book of a library.
        /// </summary>
        public const string GET_BOOK_BY_ID = "/libraries/{libraryId}/books/{bookId}";

        /// <summary>
        /// The route for getting the books of a library.
        /// </summary>
        public const string GET_BOOKS = "/libraries/{libraryId}/books";

        /// <summary>
        /// The route for getting the books of a library in their lightweight form.
        /// </summary>
        public const string GET_BOOKS_LITE = "/libraries/{libraryId}/books/lite";

        /// <summary>
        /// The route for getting the deleted books of a library.
        /// </summary>
        public const string GET_DELETED_BOOKS = "/libraries/{libraryId}/books/deleted-items";

        /// <summary>
        /// The route for restoring a deleted book of a library.
        /// </summary>
        public const string RESTORE_BOOK = "/libraries/{libraryId}/books/{bookId}/restore";

        /// <summary>
        /// The route for adding a book to a library.
        /// </summary>
        public const string ADD_BOOK = "/libraries/{libraryId}/books";

        /// <summary>
        /// The route for updating a book of a library.
        /// </summary>
        public const string UPDATE_BOOK = "/libraries/{libraryId}/books/{bookId}";

        /// <summary>
        /// The route for deleting a book of a library.
        /// </summary>
        public const string DELETE_BOOK = "/libraries/{libraryId}/books/{bookId}";

        /// <summary>
        /// The route for updating the cover of a book of a library.
        /// </summary>
        public const string UPDATE_BOOK_COVER = "/libraries/{libraryId}/books/{bookId}/cover";

        /// <summary>
        /// The route for getting the reading manifest of a book of a library.
        /// </summary>
        public const string GET_BOOK_READING_MANIFEST = "/libraries/{libraryId}/books/{bookId}/reading/manifest";

        /// <summary>
        /// The route for getting the reading availability of a book of a library.
        /// </summary>
        public const string GET_BOOK_READING_AVAILABILITY = "/libraries/{libraryId}/books/{bookId}/reading/availability";

        /// <summary>
        /// The route for getting a reading section of a book of a library.
        /// </summary>
        public const string GET_BOOK_READING_SECTION = "/libraries/{libraryId}/books/{bookId}/reading/sections/{locationRef}";

        /// <summary>
        /// The route for getting a reading resource of a book of a library.
        /// </summary>
        public const string GET_BOOK_READING_RESOURCE = "/libraries/{libraryId}/books/{bookId}/reading/resources/{resourceKey}";
    }
}
