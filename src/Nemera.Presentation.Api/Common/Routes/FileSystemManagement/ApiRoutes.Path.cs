namespace Nemera.Presentation.Api.Common.Routes.FileSystemManagement;

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
        /// <summary>
        /// The route for getting the root of a file system path.
        /// </summary>
        public const string GET_PATH_ROOT = "/path/get-path-root";

        /// <summary>
        /// The route for getting the separator used by the file system paths of the current platform.
        /// </summary>
        public const string GET_PATH_SEPARATOR = "/path/get-path-separator";

        /// <summary>
        /// The route for getting the parent of a file system path.
        /// </summary>
        public const string GET_PATH_PARENT = "/path/get-path-parent";

        /// <summary>
        /// The route for combining the segments of a file system path.
        /// </summary>
        public const string COMBINE = "/path/combine";

        /// <summary>
        /// The route for splitting a file system path into its segments.
        /// </summary>
        public const string SPLIT = "/path/split";

        /// <summary>
        /// The route for validating a file system path.
        /// </summary>
        public const string VALIDATE = "/path/validate";

        /// <summary>
        /// The route for checking whether a file system path exists.
        /// </summary>
        public const string CHECK_PATH_EXISTS = "/path/check-path-exists";
    }
}
