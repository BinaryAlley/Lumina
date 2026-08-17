namespace Lumina.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of routes defined in the Web application.
/// </summary>
public static partial class WebRoutes
{
    /// <summary>
    /// Routes for the user settings tool pages.
    /// </summary>
    public static class Settings
    {
        public const string INDEX = "{culture}/tools/settings";
        public const string GET_USER_SETTINGS = "{culture}/tools/settings/api-get-user-settings";
        public const string UPDATE_USER_SETTINGS = "{culture}/tools/settings/api-update-user-settings";
    }
}
