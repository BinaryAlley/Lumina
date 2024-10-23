namespace Lumina.Presentation.Api.Common.Routes.FileSystemManagement;

/// <summary>
/// Class for the collection of routes defined in this API.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the Directories route.
    /// </summary>
    public static class Directories
    {
        public const string GET_DIRECTORY_TREE = "/directories/get-directory-tree";
        public const string GET_TREE_DIRECTORIES = "/directories/get-tree-directories";
        public const string GET_DIRECTORIES = "/directories/get-directories";
    }
}
