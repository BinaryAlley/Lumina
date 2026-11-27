namespace Nemera.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of routes defined in the Web application.
/// </summary>
public static partial class WebRoutes
{
    /// <summary>
    /// Routes for the books pages.
    /// </summary>
    public static class Books
    {
        /// <summary>
        /// The route for the books library page.
        /// </summary>
        public const string INDEX = "{culture}/library/written-content-library/books-library/books";

        /// <summary>
        /// The route for the action that retrieves the library items of the books library page.
        /// </summary>
        public const string GET_LIBRARY_ITEMS = "{culture}/library/written-content-library/books-library/books/api-get-library-items";

        /// <summary>
        /// The route for the book editing page.
        /// </summary>
        public const string EDIT_BOOK = "{culture}/library/written-content-library/books-library/books/{bookId}";

        /// <summary>
        /// The route for the action that retrieves the details of a book.
        /// </summary>
        public const string GET_BOOK_DETAILS = "{culture}/library/written-content-library/books-library/books/{bookId}/api-get-book";

        /// <summary>
        /// The route for the action that saves a book.
        /// </summary>
        public const string SAVE_BOOK = "{culture}/library/written-content-library/books-library/books/{bookId}/api-save-book";

        /// <summary>
        /// The route for the action that deletes a book.
        /// </summary>
        public const string DELETE_BOOK = "{culture}/library/written-content-library/books-library/books/{bookId}/api-delete-book";

        /// <summary>
        /// The route for the deleted books page.
        /// </summary>
        public const string DELETED_BOOKS = "{culture}/library/written-content-library/books-library/books/deleted-items";

        /// <summary>
        /// The route for the action that restores a deleted book.
        /// </summary>
        public const string RESTORE_BOOK = "{culture}/library/written-content-library/books-library/books/{bookId}/api-restore-book";

        /// <summary>
        /// The route for the action that updates the cover of a book.
        /// </summary>
        public const string UPDATE_BOOK_COVER = "{culture}/library/written-content-library/books-library/books/{bookId}/api-update-cover";

        /// <summary>
        /// The route for the book reading page.
        /// </summary>
        public const string READ = "{culture}/library/written-content-library/books-library/books/{bookId}/read";

        /// <summary>
        /// The route for the action that retrieves the reading manifest of a book.
        /// </summary>
        public const string GET_READING_MANIFEST = "{culture}/library/written-content-library/books-library/books/{bookId}/api-get-reading-manifest";

        /// <summary>
        /// The route for the action that retrieves the reading availability of a book.
        /// </summary>
        public const string GET_READING_AVAILABILITY = "{culture}/library/written-content-library/books-library/books/{bookId}/api-get-reading-availability";

        /// <summary>
        /// The route for the action that retrieves a reading section of a book.
        /// </summary>
        public const string GET_READING_SECTION = "{culture}/library/written-content-library/books-library/books/{bookId}/api-get-reading-section";

        /// <summary>
        /// The route for the action that retrieves a reading resource of a book.
        /// </summary>
        public const string GET_READING_RESOURCE = "{culture}/library/written-content-library/books-library/books/{bookId}/api-get-reading-resource";
    }
}
