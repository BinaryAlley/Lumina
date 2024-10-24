namespace Lumina.Presentation.Api.Common.Routes.WrittenContentLibrary.BookLibrary;

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
        public const string ADD_BOOK = "/books";
    }
}
