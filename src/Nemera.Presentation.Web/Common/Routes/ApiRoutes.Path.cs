namespace Nemera.Presentation.Web.Common.Routes;

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
        /// <summary>
        /// The route for the API endpoint that retrieves the root of a path.
        /// </summary>
        public const string GET_PATH_ROOT = "path/get-path-root";

        /// <summary>
        /// The route for the API endpoint that retrieves the separator of the current platform.
        /// </summary>
        public const string GET_PATH_SEPARATOR = "path/get-path-separator";

        /// <summary>
        /// The route for the API endpoint that retrieves the parent of a path.
        /// </summary>
        public const string GET_PATH_PARENT = "path/get-path-parent";

        /// <summary>
        /// The route for the API endpoint that splits a path into its parts.
        /// </summary>
        public const string SPLIT = "path/split";

        /// <summary>
        /// The route for the API endpoint that validates a path.
        /// </summary>
        public const string VALIDATE = "path/validate";

        /// <summary>
        /// The route for the API endpoint that checks whether a path exists.
        /// </summary>
        public const string CHECK_PATH_EXISTS = "path/check-path-exists";
    }
}
