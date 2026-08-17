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
    }
}
