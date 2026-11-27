namespace Nemera.Presentation.Api.Common.Routes.FileSystemManagement;

/// <summary>
/// Class for the collection of routes defined in this API.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the Files route.
    /// </summary>
    public static class Files
    {
        /// <summary>
        /// The route for getting the files of a directory as a tree.
        /// </summary>
        public const string GET_TREE_FILES = "/files/get-tree-files";

        /// <summary>
        /// The route for getting the files of a directory.
        /// </summary>
        public const string GET_FILES = "/files/get-files";
    }
}
