namespace Lumina.Presentation.Web.Common.Routes;

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
        public const string INDEX = "{culture}/libraries/manage";
        public const string ADD_LIBRARY = "{culture}/libraries/manage/item";
        public const string EDIT_LIBRARY = "{culture}/libraries/manage/item/{id}";
        public const string GET_LIBRARIES = "{culture}/libraries/manage/api-get-libraries";
        public const string GET_ENABLED_LIBRARIES = "{culture}/libraries/manage/api-get-enabled-libraries";
        public const string SAVE_LIBRARY = "{culture}/libraries/manage/api-item";
        public const string DELETE_LIBRARY = "{culture}/libraries/manage/api-item/{id}";
        public const string GET_RUNNING_LIBRARY_SCANS = "{culture}/libraries/manage/api-get-running-library-scans";
        public const string SCAN_LIBRARIES = "{culture}/libraries/manage/api-scan-libraries";
        public const string SCAN_LIBRARY = "{culture}/libraries/manage/api-scan-library/{id}";
        public const string CANCEL_LIBRARIES_SCAN = "{culture}/libraries/manage/api-cancel-libraries-scan";
        public const string CANCEL_LIBRARY_SCAN = "{culture}/libraries/manage/{libraryId}/api-cancel-library-scan/{scanId}";
        public const string GET_METADATA_PROVIDERS = "{culture}/libraries/manage/api-get-metadata-providers/{libraryId}";
        public const string SET_METADATA_PROVIDER_ENABLED = "{culture}/libraries/manage/api-set-metadata-provider-enabled";
        public const string REORDER_METADATA_PROVIDERS = "{culture}/libraries/manage/api-reorder-metadata-providers";
    }
}
