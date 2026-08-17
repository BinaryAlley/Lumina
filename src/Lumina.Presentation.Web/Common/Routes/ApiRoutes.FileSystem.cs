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
        public const string GET_TYPES = "file-system/get-type";
    }
}
