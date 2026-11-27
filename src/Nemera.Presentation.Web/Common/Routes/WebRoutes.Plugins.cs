namespace Nemera.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of routes defined in the Web application.
/// </summary>
public static partial class WebRoutes
{
    /// <summary>
    /// Routes for the plugins pages.
    /// </summary>
    public static class Plugins
    {
        /// <summary>
        /// The route for the plugin management page.
        /// </summary>
        public const string INDEX = "{culture}/admin/manage-plugins";

        /// <summary>
        /// The route for the action that retrieves the installed plugins.
        /// </summary>
        public const string GET_PLUGINS = "{culture}/admin/manage-plugins/api-get-plugins";

        /// <summary>
        /// The route for the action that retrieves the settings of a plugin.
        /// </summary>
        public const string GET_PLUGIN_SETTINGS = "{culture}/admin/manage-plugins/api-get-plugin-settings/{pluginId}";

        /// <summary>
        /// The route for the action that updates the settings of a plugin.
        /// </summary>
        public const string UPDATE_PLUGIN_SETTINGS = "{culture}/admin/manage-plugins/api-update-plugin-settings";

        /// <summary>
        /// The route for the action that installs a plugin.
        /// </summary>
        public const string INSTALL_PLUGIN = "{culture}/admin/manage-plugins/api-install-plugin";
    }
}
