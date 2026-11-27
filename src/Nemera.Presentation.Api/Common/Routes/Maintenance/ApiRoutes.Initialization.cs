namespace Nemera.Presentation.Api.Common.Routes.Maintenance;

/// <summary>
/// Class for the collection of routes defined in this API.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the Initialization route.
    /// </summary>
    public static class Initialization
    {
        /// <summary>
        /// The route for setting up the application.
        /// </summary>
        public const string SETUP_APPLICATION = "/initialization";

        /// <summary>
        /// The route for checking whether the application is initialized.
        /// </summary>
        public const string CHECK_INITIALIZATION = "/initialization";
    }
}
