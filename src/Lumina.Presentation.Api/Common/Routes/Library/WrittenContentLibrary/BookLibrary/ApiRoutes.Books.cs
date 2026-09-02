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
        public const string GET_BOOK_BY_ID = "/books/{id}";
        public const string GET_BOOKS = "/books";
        public const string GET_BOOKS_LITE = "/books/lite";
        public const string ADD_BOOK = "/books";
        public const string GET_BOOK_READING_MANIFEST = "/books/{bookId}/reading/manifest";
        public const string GET_BOOK_READING_AVAILABILITY = "/books/{bookId}/reading/availability";
        public const string GET_BOOK_READING_SECTION = "/books/{bookId}/reading/sections/{locationRef}";
        public const string GET_BOOK_READING_RESOURCE = "/books/{bookId}/reading/resources/{resourceKey}";
    }
}
