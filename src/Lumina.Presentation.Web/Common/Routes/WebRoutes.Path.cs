namespace Lumina.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of routes defined in the Web application.
/// </summary>
public static partial class WebRoutes
{
    /// <summary>
    /// Routes for the file system paths endpoints.
    /// </summary>
    public static class Path
    {
        public const string GET_PATH_ROOT = "path/api-get-path-root";
        public const string GET_PATH_SEPARATOR = "path/api-get-path-separator";
        public const string GET_PATH_PARENT = "path/api-get-path-parent";
        public const string SPLIT_PATH = "path/api-split";
        public const string VALIDATE_PATH = "path/api-validate";
        public const string CHECK_PATH_EXISTS = "path/api-check-path-exists";
    }
}
