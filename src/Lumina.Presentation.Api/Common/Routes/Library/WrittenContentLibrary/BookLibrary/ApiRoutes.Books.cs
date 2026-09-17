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
        public const string GET_BOOK_BY_ID = "/libraries/{libraryId}/books/{bookId}";
        public const string GET_BOOKS = "/libraries/{libraryId}/books";
        public const string GET_BOOKS_LITE = "/libraries/{libraryId}/books/lite";
        public const string ADD_BOOK = "/libraries/{libraryId}/books";
        public const string UPDATE_BOOK = "/libraries/{libraryId}/books/{bookId}";
        public const string UPDATE_BOOK_COVER = "/libraries/{libraryId}/books/{bookId}/cover";
        public const string GET_BOOK_READING_MANIFEST = "/libraries/{libraryId}/books/{bookId}/reading/manifest";
        public const string GET_BOOK_READING_AVAILABILITY = "/libraries/{libraryId}/books/{bookId}/reading/availability";
        public const string GET_BOOK_READING_SECTION = "/libraries/{libraryId}/books/{bookId}/reading/sections/{locationRef}";
        public const string GET_BOOK_READING_RESOURCE = "/libraries/{libraryId}/books/{bookId}/reading/resources/{resourceKey}";
    }
}
