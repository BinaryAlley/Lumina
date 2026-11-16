namespace Lumina.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of remote API routes called by this Web application.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the media libraries endpoints of the remote API.
    /// </summary>
    public static class Libraries
    {
        /// <summary>
        /// The route for the API endpoint that retrieves a media library by its Id.
        /// </summary>
        public const string GET_LIBRARY_BY_ID = "libraries/{id}";

        /// <summary>
        /// The route for the API endpoint that retrieves all media libraries.
        /// </summary>
        public const string GET_LIBRARIES = "libraries";

        /// <summary>
        /// The route for the API endpoint that retrieves the enabled media libraries.
        /// </summary>
        public const string GET_ENABLED_LIBRARIES = "libraries/enabled";

        /// <summary>
        /// The route for the API endpoint that adds a media library.
        /// </summary>
        public const string ADD_LIBRARY = "libraries";

        /// <summary>
        /// The route for the API endpoint that updates a media library.
        /// </summary>
        public const string UPDATE_LIBRARY = "libraries/{id}";

        /// <summary>
        /// The route for the API endpoint that deletes a media library.
        /// </summary>
        public const string DELETE_LIBRARY = "libraries/{id}";

        /// <summary>
        /// The route for the API endpoint that scans all media libraries.
        /// </summary>
        public const string SCAN_LIBRARIES = "libraries/scans";

        /// <summary>
        /// The route for the API endpoint that scans a single media library.
        /// </summary>
        public const string SCAN_LIBRARY = "libraries/{id}/scans";

        /// <summary>
        /// The route for the API endpoint that retrieves the scans currently running on the media libraries.
        /// </summary>
        public const string GET_RUNNING_LIBRARIES_SCAN = "libraries/scans/running";

        /// <summary>
        /// The route for the API endpoint that cancels the scans running on all media libraries.
        /// </summary>
        public const string CANCEL_LIBRARIES_SCAN = "libraries/scans/cancel";

        /// <summary>
        /// The route for the API endpoint that cancels a scan running on a single media library.
        /// </summary>
        public const string CANCEL_LIBRARY_SCAN = "libraries/{libraryId}/scans/{scanId}/cancel";

        /// <summary>
        /// The route for the API endpoint that retrieves the path template parts of a library type.
        /// </summary>
        public const string GET_LIBRARY_PATH_TEMPLATE_PARTS = "libraries/path-template-parts/{libraryType}";

        /// <summary>
        /// The route for the API endpoint that retrieves the metadata providers of a media library.
        /// </summary>
        public const string GET_LIBRARY_METADATA_PROVIDERS = "libraries/{libraryId}/metadata-providers";

        /// <summary>
        /// The route for the API endpoint that retrieves the artwork providers of a media library.
        /// </summary>
        public const string GET_LIBRARY_ARTWORK_PROVIDERS = "libraries/{libraryId}/artwork-providers";

        /// <summary>
        /// The route for the API endpoint that retrieves the book readers of a media library.
        /// </summary>
        public const string GET_LIBRARY_BOOK_READERS = "libraries/{libraryId}/book-readers";

        /// <summary>
        /// The route for the API endpoint that retrieves the metadata providers of a library type.
        /// </summary>
        public const string GET_LIBRARY_TYPE_METADATA_PROVIDERS = "libraries/metadata-providers/{libraryType}";

        /// <summary>
        /// The route for the API endpoint that retrieves the artwork providers of a library type.
        /// </summary>
        public const string GET_LIBRARY_TYPE_ARTWORK_PROVIDERS = "libraries/artwork-providers/{libraryType}";

        /// <summary>
        /// The route for the API endpoint that retrieves the book readers of a library type.
        /// </summary>
        public const string GET_LIBRARY_TYPE_BOOK_READERS = "libraries/book-readers/{libraryType}";
    }
}
