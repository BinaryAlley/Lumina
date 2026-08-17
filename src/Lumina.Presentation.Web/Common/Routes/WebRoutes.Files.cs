namespace Lumina.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of routes defined in the Web application.
/// </summary>
public static partial class WebRoutes
{
    /// <summary>
    /// Routes for the file system files endpoints.
    /// </summary>
    public static class Files
    {
        public const string GET_TREE_FILES = "files/api-get-tree-files";
        public const string GET_FILES = "files/api-get-files";
    }
}
