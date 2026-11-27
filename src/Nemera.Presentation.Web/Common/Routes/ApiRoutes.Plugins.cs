namespace Nemera.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of remote API routes called by this Web application.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the plugins endpoints of the remote API.
    /// </summary>
    public static class Plugins
    {
        /// <summary>
        /// The route for the API endpoint that retrieves the installed plugins.
        /// </summary>
        public const string GET_PLUGINS = "plugins";

        /// <summary>
        /// The route for the API endpoint that retrieves the settings of a plugin.
        /// </summary>
        public const string GET_PLUGIN_SETTINGS = "plugins/{pluginId}/settings";

        /// <summary>
        /// The route for the API endpoint that updates the settings of a plugin.
        /// </summary>
        public const string UPDATE_PLUGIN_SETTINGS = "plugins/{pluginId}/settings";

        /// <summary>
        /// The route for the API endpoint that installs a plugin.
        /// </summary>
        public const string INSTALL_PLUGIN = "plugins";
    }
}
