namespace Lumina.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of routes defined in the Web application.
/// </summary>
public static partial class WebRoutes
{
    /// <summary>
    /// Routes for the themes pages.
    /// </summary>
    public static class Themes
    {
        /// <summary>
        /// The route for the action that retrieves the installed themes.
        /// </summary>
        public const string GET_THEMES = "{culture}/admin/themes/api-get-themes";

        /// <summary>
        /// The route for the action that installs a theme.
        /// </summary>
        public const string INSTALL_THEME = "{culture}/admin/themes/api-install-theme";

        /// <summary>
        /// The route for the action that sets the current theme.
        /// </summary>
        public const string SET_CURRENT_THEME = "{culture}/admin/themes/api-set-current-theme";

        /// <summary>
        /// The route for the action that restores a deleted theme.
        /// </summary>
        public const string RESTORE_THEME = "{culture}/admin/themes/api-restore-theme/{themeId}";

        /// <summary>
        /// The route for the action that deletes a theme.
        /// </summary>
        public const string DELETE_THEME = "{culture}/admin/themes/api-delete-theme/{themeId}";

        /// <summary>
        /// The route for the action that downloads the archive of a theme.
        /// </summary>
        public const string DOWNLOAD_THEME = "{culture}/admin/themes/api-download-theme/{themeId}";

        /// <summary>
        /// The route for the action that serves the assets of a theme.
        /// </summary>
        public const string THEME_ASSETS = "theme-assets/{themeId}/{*path}";
    }
}
