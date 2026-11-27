namespace Lumina.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of remote API routes called by this Web application.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the drives endpoints of the remote API.
    /// </summary>
    public static class Drives
    {
        /// <summary>
        /// The route for the API endpoint that retrieves the drives of the host machine.
        /// </summary>
        public const string GET_DRIVES = "drives/get-drives";
    }
}
