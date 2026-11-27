namespace Lumina.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of remote API routes called by this Web application.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the directories endpoints of the remote API.
    /// </summary>
    public static class Directories
    {
        /// <summary>
        /// The route for the API endpoint that retrieves the subdirectories of a directory.
        /// </summary>
        public const string GET_DIRECTORIES = "directories/get-directories";
    }
}
