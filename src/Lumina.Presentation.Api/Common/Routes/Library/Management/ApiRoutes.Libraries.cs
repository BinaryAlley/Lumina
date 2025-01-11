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
        public const string ADD_LIBRARY = "/libraries";
        public const string UPDATE_LIBRARY = "/libraries/{id}";
        public const string DELETE_LIBRARY = "/libraries/{id}";
    }
}
