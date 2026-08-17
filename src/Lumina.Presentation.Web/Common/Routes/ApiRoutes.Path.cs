namespace Lumina.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of remote API routes called by this Web application.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the paths endpoints of the remote API.
    /// </summary>
    public static class Path
    {
        public const string GET_PATH_ROOT = "path/get-path-root";
        public const string GET_PATH_SEPARATOR = "path/get-path-separator";
        public const string GET_PATH_PARENT = "path/get-path-parent";
        public const string SPLIT = "path/split";
        public const string VALIDATE = "path/validate";
        public const string CHECK_PATH_EXISTS = "path/check-path-exists";
    }
}
