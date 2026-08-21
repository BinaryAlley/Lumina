namespace Lumina.Presentation.Web.Common.Routes;

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
        public const string INDEX = "{culture}/admin/manage-plugins";
        public const string GET_PLUGINS = "{culture}/admin/manage-plugins/api-get-plugins";
        public const string GET_PLUGIN_SETTINGS = "{culture}/admin/manage-plugins/api-get-plugin-settings/{pluginId}";
        public const string UPDATE_PLUGIN_SETTINGS = "{culture}/admin/manage-plugins/api-update-plugin-settings";
    }
}
