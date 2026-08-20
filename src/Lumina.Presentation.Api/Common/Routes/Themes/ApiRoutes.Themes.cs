namespace Lumina.Presentation.Api.Common.Routes.Themes;

/// <summary>
/// Class for the collection of the theme routes of the API.
/// </summary>
public static partial class ApiRoutes
{
    public static class Themes
    {
        public const string GET_THEMES = "/themes";
        public const string INSTALL_THEME = "/themes";
        public const string GET_THEME_SETTINGS = "/themes/settings";
        public const string GET_CURRENT_THEME = "/themes/current";
        public const string SET_CURRENT_THEME = "/themes/current";
        public const string GET_THEME_TEMPLATE = "/themes/{themeId}/templates/{pageKey}";
        public const string GET_THEME_ASSET = "/themes/{themeId}/assets/{*assetPath}";
        public const string GET_THEME_ARCHIVE = "/themes/{themeId}/archive";
        public const string DELETE_THEME = "/themes/{themeId}";
    }
}
