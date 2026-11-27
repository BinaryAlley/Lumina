namespace Lumina.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of routes defined in the Web application.
/// </summary>
public static partial class WebRoutes
{
    /// <summary>
    /// Routes for the file system endpoints.
    /// </summary>
    public static class FileSystem
    {
        /// <summary>
        /// The route for the action that retrieves the file system type of the host machine.
        /// </summary>
        public const string GET_TYPE = "file-system/api-get-type";
    }
}
