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
        /// <summary>
        /// The route for the user settings page.
        /// </summary>
        public const string INDEX = "{culture}/tools/settings";

        /// <summary>
        /// The route for the action that retrieves the settings of the current user.
        /// </summary>
        public const string GET_USER_SETTINGS = "{culture}/tools/settings/api-get-user-settings";

        /// <summary>
        /// The route for the action that updates the settings of the current user.
        /// </summary>
        public const string UPDATE_USER_SETTINGS = "{culture}/tools/settings/api-update-user-settings";

        /// <summary>
        /// The route for the action that retrieves the playback state of the current user.
        /// </summary>
        public const string GET_PLAYBACK_STATE = "{culture}/tools/settings/api-get-playback-state";

        /// <summary>
        /// The route for the action that updates the playback state of the current user.
        /// </summary>
        public const string UPDATE_PLAYBACK_STATE = "{culture}/tools/settings/api-update-playback-state";
    }
}
