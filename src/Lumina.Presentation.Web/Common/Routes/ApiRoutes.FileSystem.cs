namespace Lumina.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of remote API routes called by this Web application.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the file system endpoints of the remote API.
    /// </summary>
    public static class FileSystem
    {
        /// <summary>
        /// The route for getting the type of the file system.
        /// </summary>
        public const string GET_TYPES = "file-system/get-type";
    }
}
