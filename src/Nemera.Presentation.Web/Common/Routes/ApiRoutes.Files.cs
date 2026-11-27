namespace Lumina.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of remote API routes called by this Web application.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the files endpoints of the remote API.
    /// </summary>
    public static class Files
    {
        /// <summary>
        /// The route for the API endpoint that retrieves the files of a directory as a tree.
        /// </summary>
        public const string GET_TREE_FILES = "files/get-tree-files";

        /// <summary>
        /// The route for the API endpoint that retrieves the files of a directory.
        /// </summary>
        public const string GET_FILES = "files/get-files";
    }
}
