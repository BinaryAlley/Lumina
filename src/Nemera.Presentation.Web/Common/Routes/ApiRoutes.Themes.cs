namespace Nemera.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of remote API routes called by this Web application.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the themes endpoints of the remote API.
    /// </summary>
    public static class Themes
    {
        /// <summary>
        /// The route for the API endpoint that retrieves the installed themes.
        /// </summary>
        public const string GET_THEMES = "themes";

        /// <summary>
        /// The route for the API endpoint that installs a theme.
        /// </summary>
        public const string INSTALL_THEME = "themes";

        /// <summary>
        /// The route for the API endpoint that retrieves the settings of the installed themes.
        /// </summary>
        public const string GET_THEME_SETTINGS = "themes/settings";

        /// <summary>
        /// The route for the API endpoint that retrieves the current theme.
        /// </summary>
        public const string GET_CURRENT_THEME = "themes/current";

        /// <summary>
        /// The route for the API endpoint that sets the current theme.
        /// </summary>
        public const string SET_CURRENT_THEME = "themes/current";

        /// <summary>
        /// The route for the API endpoint that retrieves a page template of a theme.
        /// </summary>
        public const string GET_THEME_TEMPLATE = "themes/{themeId}/templates/{*pageKey}";

        /// <summary>
        /// The route for the API endpoint that retrieves an asset of a theme.
        /// </summary>
        public const string GET_THEME_ASSET = "themes/{themeId}/assets/{*assetPath}";

        /// <summary>
        /// The route for the API endpoint that downloads the archive of a theme.
        /// </summary>
        public const string GET_THEME_ARCHIVE = "themes/{themeId}/archive";

        /// <summary>
        /// The route for the API endpoint that restores a deleted theme.
        /// </summary>
        public const string RESTORE_THEME = "themes/{themeId}/restore";

        /// <summary>
        /// The route for the API endpoint that deletes a theme.
        /// </summary>
        public const string DELETE_THEME = "themes/{themeId}";
    }
}
