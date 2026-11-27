namespace Nemera.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of routes defined in the Web application.
/// </summary>
public static partial class WebRoutes
{
    /// <summary>
    /// Routes for the media libraries management pages.
    /// </summary>
    public static class LibraryManagement
    {
        /// <summary>
        /// The route for the library management page.
        /// </summary>
        public const string INDEX = "{culture}/libraries/manage";

        /// <summary>
        /// The route for the library creation page.
        /// </summary>
        public const string ADD_LIBRARY = "{culture}/libraries/manage/item";

        /// <summary>
        /// The route for the library editing page.
        /// </summary>
        public const string EDIT_LIBRARY = "{culture}/libraries/manage/item/{id}";

        /// <summary>
        /// The route for the action that retrieves the media libraries.
        /// </summary>
        public const string GET_LIBRARIES = "{culture}/libraries/manage/api-get-libraries";

        /// <summary>
        /// The route for the action that retrieves the enabled media libraries.
        /// </summary>
        public const string GET_ENABLED_LIBRARIES = "{culture}/libraries/manage/api-get-enabled-libraries";

        /// <summary>
        /// The route for the action that saves a media library.
        /// </summary>
        public const string SAVE_LIBRARY = "{culture}/libraries/manage/api-item";

        /// <summary>
        /// The route for the action that deletes a media library.
        /// </summary>
        public const string DELETE_LIBRARY = "{culture}/libraries/manage/api-item/{id}";

        /// <summary>
        /// The route for the action that retrieves the scans currently running on the media libraries.
        /// </summary>
        public const string GET_RUNNING_LIBRARY_SCANS = "{culture}/libraries/manage/api-get-running-library-scans";

        /// <summary>
        /// The route for the action that scans all media libraries.
        /// </summary>
        public const string SCAN_LIBRARIES = "{culture}/libraries/manage/api-scan-libraries";

        /// <summary>
        /// The route for the action that scans a single media library.
        /// </summary>
        public const string SCAN_LIBRARY = "{culture}/libraries/manage/api-scan-library/{id}";

        /// <summary>
        /// The route for the action that cancels the scans running on all media libraries.
        /// </summary>
        public const string CANCEL_LIBRARIES_SCAN = "{culture}/libraries/manage/api-cancel-libraries-scan";

        /// <summary>
        /// The route for the action that cancels a scan running on a single media library.
        /// </summary>
        public const string CANCEL_LIBRARY_SCAN = "{culture}/libraries/manage/{libraryId}/api-cancel-library-scan/{scanId}";

        /// <summary>
        /// The route for the action that retrieves the path template parts of a library type.
        /// </summary>
        public const string GET_LIBRARY_PATH_TEMPLATE_PARTS = "{culture}/libraries/manage/api-get-path-template-parts/{libraryType}";

        /// <summary>
        /// The route for the action that retrieves the metadata providers of a media library.
        /// </summary>
        public const string GET_METADATA_PROVIDERS = "{culture}/libraries/manage/api-get-metadata-providers/{libraryId}";

        /// <summary>
        /// The route for the action that retrieves the artwork providers of a media library.
        /// </summary>
        public const string GET_ARTWORK_PROVIDERS = "{culture}/libraries/manage/api-get-artwork-providers/{libraryId}";

        /// <summary>
        /// The route for the action that retrieves the book readers of a media library.
        /// </summary>
        public const string GET_BOOK_READERS = "{culture}/libraries/manage/api-get-book-readers/{libraryId}";

        /// <summary>
        /// The route for the action that retrieves the metadata providers of a library type.
        /// </summary>
        public const string GET_LIBRARY_TYPE_METADATA_PROVIDERS = "{culture}/libraries/manage/api-get-library-type-metadata-providers/{libraryType}";

        /// <summary>
        /// The route for the action that retrieves the artwork providers of a library type.
        /// </summary>
        public const string GET_LIBRARY_TYPE_ARTWORK_PROVIDERS = "{culture}/libraries/manage/api-get-library-type-artwork-providers/{libraryType}";

        /// <summary>
        /// The route for the action that retrieves the book readers of a library type.
        /// </summary>
        public const string GET_LIBRARY_TYPE_BOOK_READERS = "{culture}/libraries/manage/api-get-library-type-book-readers/{libraryType}";
    }
}
