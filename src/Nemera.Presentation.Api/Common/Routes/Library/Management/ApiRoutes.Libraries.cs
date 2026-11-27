namespace Nemera.Presentation.Api.Common.Routes.Library.Management;

/// <summary>
/// Class for the collection of routes defined in this API.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the Libraries route.
    /// </summary>
    public static class Libraries
    {
        /// <summary>
        /// The route for getting a media library.
        /// </summary>
        public const string GET_LIBRARY_BY_ID = "/libraries/{id}";

        /// <summary>
        /// The route for getting the media libraries.
        /// </summary>
        public const string GET_LIBRARIES = "/libraries";

        /// <summary>
        /// The route for getting the enabled media libraries.
        /// </summary>
        public const string GET_ENABLED_LIBRARIES = "/libraries/enabled";

        /// <summary>
        /// The route for adding a media library.
        /// </summary>
        public const string ADD_LIBRARY = "/libraries";

        /// <summary>
        /// The route for updating a media library.
        /// </summary>
        public const string UPDATE_LIBRARY = "/libraries/{id}";

        /// <summary>
        /// The route for deleting a media library.
        /// </summary>
        public const string DELETE_LIBRARY = "/libraries/{id}";

        /// <summary>
        /// The route for starting a scan of all media libraries.
        /// </summary>
        public const string SCAN_LIBRARIES = "/libraries/scans";

        /// <summary>
        /// The route for starting a scan of a media library.
        /// </summary>
        public const string SCAN_LIBRARY = "/libraries/{id}/scans";

        /// <summary>
        /// The route for getting the progress of a scan of a media library.
        /// </summary>
        public const string LIBRARY_SCAN_PROGRESS = "/libraries/{libraryId}/scans/{scanId}/progress";

        /// <summary>
        /// The route for getting the scans of media libraries that are currently running.
        /// </summary>
        public const string GET_RUNNING_LIBRARIES_SCAN = "/libraries/scans/running";

        /// <summary>
        /// The route for cancelling the running scan of all media libraries.
        /// </summary>
        public const string CANCEL_LIBRARIES_SCAN = "/libraries/scans/cancel";

        /// <summary>
        /// The route for cancelling a scan of a media library.
        /// </summary>
        public const string CANCEL_LIBRARY_SCAN = "/libraries/{libraryId}/scans/{scanId}/cancel";

        /// <summary>
        /// The route for getting the metadata providers available to a media library.
        /// </summary>
        public const string GET_LIBRARY_METADATA_PROVIDERS = "/libraries/{libraryId}/metadata-providers";

        /// <summary>
        /// The route for getting the artwork providers available to a media library.
        /// </summary>
        public const string GET_LIBRARY_ARTWORK_PROVIDERS = "/libraries/{libraryId}/artwork-providers";

        /// <summary>
        /// The route for getting the path template parts available to a media library type.
        /// </summary>
        public const string GET_LIBRARY_PATH_TEMPLATE_PARTS = "/libraries/path-template-parts/{libraryType}";

        /// <summary>
        /// The route for getting the book readers available to a media library.
        /// </summary>
        public const string GET_LIBRARY_BOOK_READERS = "/libraries/{libraryId}/book-readers";

        /// <summary>
        /// The route for getting the metadata providers available to a media library type.
        /// </summary>
        public const string GET_LIBRARY_TYPE_METADATA_PROVIDERS = "/libraries/metadata-providers/{libraryType}";

        /// <summary>
        /// The route for getting the artwork providers available to a media library type.
        /// </summary>
        public const string GET_LIBRARY_TYPE_ARTWORK_PROVIDERS = "/libraries/artwork-providers/{libraryType}";

        /// <summary>
        /// The route for getting the book readers available to a media library type.
        /// </summary>
        public const string GET_LIBRARY_TYPE_BOOK_READERS = "/libraries/book-readers/{libraryType}";
    }
}
