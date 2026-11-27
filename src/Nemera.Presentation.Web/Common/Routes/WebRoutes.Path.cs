namespace Nemera.Presentation.Web.Common.Routes;

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
        /// <summary>
        /// The route for the action that retrieves the root of a path.
        /// </summary>
        public const string GET_PATH_ROOT = "path/api-get-path-root";

        /// <summary>
        /// The route for the action that retrieves the separator of the current platform.
        /// </summary>
        public const string GET_PATH_SEPARATOR = "path/api-get-path-separator";

        /// <summary>
        /// The route for the action that retrieves the parent of a path.
        /// </summary>
        public const string GET_PATH_PARENT = "path/api-get-path-parent";

        /// <summary>
        /// The route for the action that splits a path into its parts.
        /// </summary>
        public const string SPLIT_PATH = "path/api-split";

        /// <summary>
        /// The route for the action that validates a path.
        /// </summary>
        public const string VALIDATE_PATH = "path/api-validate";

        /// <summary>
        /// The route for the action that checks whether a path exists.
        /// </summary>
        public const string CHECK_PATH_EXISTS = "path/api-check-path-exists";
    }
}
