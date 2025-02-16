namespace Lumina.Presentation.Api.Common.Routes.Library.Management;

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
        public const string GET_LIBRARY_BY_ID = "/libraries/{id}";
        public const string GET_LIBRARIES = "/libraries";
        public const string GET_ENABLED_LIBRARIES = "/libraries/enabled";
        public const string ADD_LIBRARY = "/libraries";
        public const string UPDATE_LIBRARY = "/libraries/{id}";
        public const string DELETE_LIBRARY = "/libraries/{id}";
        public const string SCAN_LIBRARIES = "/libraries/scans";
        public const string SCAN_LIBRARY = "/libraries/{id}/scans";
        public const string LIBRARY_SCAN_PROGRESS = "/libraries/{libraryId}/scans/{scanId}/progress";
        public const string CANCEL_LIBRARIES_SCAN = "/libraries/scans/cancel";
        public const string CANCEL_LIBRARY_SCAN = "/libraries/{libraryId}/scans/{scanId}/cancel";
    }
}
