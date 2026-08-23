namespace Lumina.Presentation.Web.Common.Routes;

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
        public const string GET_PLUGINS = "plugins";
        public const string GET_PLUGIN_SETTINGS = "plugins/{pluginId}/settings";
        public const string UPDATE_PLUGIN_SETTINGS = "plugins/{pluginId}/settings";
        public const string INSTALL_PLUGIN = "plugins";
    }
}
