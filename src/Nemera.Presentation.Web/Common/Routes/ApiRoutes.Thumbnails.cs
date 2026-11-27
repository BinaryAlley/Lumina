namespace Nemera.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of remote API routes called by this Web application.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the thumbnail endpoints of the remote API.
    /// </summary>
    public static class Thumbnails
    {
        /// <summary>
        /// The route for the thumbnail retrieval API endpoint.
        /// </summary>
        public const string GET_THUMBNAIL = "thumbnails/get-thumbnail";
    }
}
