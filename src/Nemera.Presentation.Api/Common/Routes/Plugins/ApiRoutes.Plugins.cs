namespace Lumina.Presentation.Api.Common.Routes.Plugins;

/// <summary>
/// Class for the collection of routes defined in this API.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the Plugins route.
    /// </summary>
    public static class Plugins
    {
        /// <summary>
        /// The route for getting the installed plugins.
        /// </summary>
        public const string GET_PLUGINS = "/plugins";

        /// <summary>
        /// The route for getting the settings of a plugin.
        /// </summary>
        public const string GET_PLUGIN_SETTINGS = "/plugins/{pluginId}/settings";

        /// <summary>
        /// The route for updating the settings of a plugin.
        /// </summary>
        public const string UPDATE_PLUGIN_SETTINGS = "/plugins/{pluginId}/settings";

        /// <summary>
        /// The route for installing a plugin.
        /// </summary>
        public const string INSTALL_PLUGIN = "/plugins";
    }
}
