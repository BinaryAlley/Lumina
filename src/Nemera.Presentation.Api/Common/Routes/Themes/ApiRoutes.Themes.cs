namespace Nemera.Presentation.Api.Common.Routes.Themes;

/// <summary>
/// Class for the collection of the theme routes of the API.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the themes of the application.
    /// </summary>
    public static class Themes
    {
        /// <summary>
        /// The route for getting the installed themes.
        /// </summary>
        public const string GET_THEMES = "/themes";

        /// <summary>
        /// The route for installing a theme.
        /// </summary>
        public const string INSTALL_THEME = "/themes";

        /// <summary>
        /// The route for getting the theme engine settings.
        /// </summary>
        public const string GET_THEME_SETTINGS = "/themes/settings";

        /// <summary>
        /// The route for getting the current theme.
        /// </summary>
        public const string GET_CURRENT_THEME = "/themes/current";

        /// <summary>
        /// The route for setting the current theme.
        /// </summary>
        public const string SET_CURRENT_THEME = "/themes/current";

        /// <summary>
        /// The route for getting a page template of a theme.
        /// </summary>
        public const string GET_THEME_TEMPLATE = "/themes/{themeId}/templates/{*pageKey}";

        /// <summary>
        /// The route for getting an asset of a theme.
        /// </summary>
        public const string GET_THEME_ASSET = "/themes/{themeId}/assets/{*assetPath}";

        /// <summary>
        /// The route for getting the archive of a theme.
        /// </summary>
        public const string GET_THEME_ARCHIVE = "/themes/{themeId}/archive";

        /// <summary>
        /// The route for restoring a deleted theme.
        /// </summary>
        public const string RESTORE_THEME = "/themes/{themeId}/restore";

        /// <summary>
        /// The route for deleting a theme.
        /// </summary>
        public const string DELETE_THEME = "/themes/{themeId}";
    }
}
