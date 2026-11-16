namespace Lumina.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of remote API routes called by this Web application.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the permissions endpoints of the remote API.
    /// </summary>
    public static class Permissions
    {
        /// <summary>
        /// The route for the permissions retrieval API endpoint.
        /// </summary>
        public const string GET_PERMISSIONS = "auth/permissions";
    }
}
