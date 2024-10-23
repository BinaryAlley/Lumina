namespace Lumina.Presentation.Api.Common.Routes.FileSystemManagement;

/// <summary>
/// Class for the collection of routes defined in this API.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the Path route.
    /// </summary>
    public static class Path
    {
        public const string GET_PATH_ROOT = "/path/get-path-root";
        public const string GET_PATH_SEPARATOR = "/path/get-path-separator";
        public const string GET_PATH_PARENT = "/path/get-path-parent";
        public const string COMBINE = "/path/combine";
        public const string SPLIT = "/path/split";
        public const string VALIDATE = "/path/validate";
        public const string CHECK_PATH_EXISTS = "/path/check-path-exists";
    }
}
