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
        public const string GET_LIBRARY_BY_ID = "libraries/{id}";
        public const string GET_LIBRARIES = "libraries";
        public const string GET_ENABLED_LIBRARIES = "libraries/enabled";
        public const string ADD_LIBRARY = "libraries";
        public const string UPDATE_LIBRARY = "libraries/{id}";
        public const string DELETE_LIBRARY = "libraries/{id}";
        public const string SCAN_LIBRARIES = "libraries/scans";
        public const string SCAN_LIBRARY = "libraries/{id}/scans";
        public const string GET_RUNNING_LIBRARIES_SCAN = "libraries/scans/running";
        public const string CANCEL_LIBRARIES_SCAN = "libraries/scans/cancel";
        public const string CANCEL_LIBRARY_SCAN = "libraries/{libraryId}/scans/{scanId}/cancel";
        public const string GET_LIBRARY_METADATA_PROVIDERS = "libraries/{libraryId}/metadata-providers";
        public const string SET_LIBRARY_METADATA_PROVIDER_ENABLED = "libraries/{libraryId}/metadata-providers/{pluginId}/enabled";
        public const string REORDER_LIBRARY_METADATA_PROVIDERS = "libraries/{libraryId}/metadata-providers/reorder";
        public const string GET_LIBRARY_ARTWORK_PROVIDERS = "libraries/{libraryId}/artwork-providers";
        public const string SET_LIBRARY_ARTWORK_PROVIDER_ENABLED = "libraries/{libraryId}/artwork-providers/{pluginId}/enabled";
        public const string REORDER_LIBRARY_ARTWORK_PROVIDERS = "libraries/{libraryId}/artwork-providers/reorder";
        public const string GET_LIBRARY_BOOK_READERS = "libraries/{libraryId}/book-readers";
        public const string SET_LIBRARY_BOOK_READER_ENABLED = "libraries/{libraryId}/book-readers/{pluginId}/enabled";
    }
}
