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
        public const string GET_PLUGINS = "/plugins";
        public const string GET_PLUGIN_SETTINGS = "/plugins/{pluginId}/settings";
        public const string UPDATE_PLUGIN_SETTINGS = "/plugins/{pluginId}/settings";
        public const string INSTALL_PLUGIN = "/plugins";
    }
}
