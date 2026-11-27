namespace Nemera.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of remote API routes called by this Web application.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the initialization endpoint of the remote API.
    /// </summary>
    public static class Initialization
    {
        /// <summary>
        /// The route for the application setup API endpoint.
        /// </summary>
        public const string SETUP_APPLICATION = "initialization";

        /// <summary>
        /// The route for the initialization status API endpoint.
        /// </summary>
        public const string CHECK_INITIALIZATION = "initialization";
    }
}
