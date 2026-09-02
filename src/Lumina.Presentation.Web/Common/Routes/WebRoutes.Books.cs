namespace Lumina.Presentation.Web.Common.Routes;

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
        public const string INDEX = "{culture}/library/written-content-library/books-library/books";
        public const string GET_LIBRARY_ITEMS = "{culture}/library/written-content-library/books-library/books/api-get-library-items";
        public const string EDIT_BOOK = "{culture}/library/written-content-library/books-library/books/{id}";
        public const string READ = "{culture}/library/written-content-library/books-library/books/{bookId}/read";
        public const string GET_READING_MANIFEST = "{culture}/library/written-content-library/books-library/books/{bookId}/api-get-reading-manifest";
        public const string GET_READING_AVAILABILITY = "{culture}/library/written-content-library/books-library/books/{bookId}/api-get-reading-availability";
        public const string GET_READING_SECTION = "{culture}/library/written-content-library/books-library/books/{bookId}/api-get-reading-section";
        public const string GET_READING_RESOURCE = "{culture}/library/written-content-library/books-library/books/{bookId}/api-get-reading-resource";
    }
}
