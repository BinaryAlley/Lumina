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
        public const string GET_THEMES = "{culture}/admin/themes/api-get-themes";
        public const string INSTALL_THEME = "{culture}/admin/themes/api-install-theme";
        public const string SET_CURRENT_THEME = "{culture}/admin/themes/api-set-current-theme";
        public const string RESTORE_THEME = "{culture}/admin/themes/api-restore-theme/{themeId}";
        public const string DELETE_THEME = "{culture}/admin/themes/api-delete-theme/{themeId}";
        public const string DOWNLOAD_THEME = "{culture}/admin/themes/api-download-theme/{themeId}";
        public const string THEME_ASSETS = "theme-assets/{themeId}/{*path}";
    }
}
