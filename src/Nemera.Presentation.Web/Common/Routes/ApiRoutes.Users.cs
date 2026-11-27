namespace Lumina.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of remote API routes called by this Web application.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the users endpoints of the remote API.
    /// </summary>
    public static class Users
    {
        /// <summary>
        /// The route for the API endpoint that retrieves the settings of the current user.
        /// </summary>
        public const string GET_USER_SETTINGS = "users/me/settings";

        /// <summary>
        /// The route for the API endpoint that updates the settings of the current user.
        /// </summary>
        public const string UPDATE_USER_SETTINGS = "users/me/settings";

        /// <summary>
        /// The route for the API endpoint that retrieves the playback state of the current user.
        /// </summary>
        public const string GET_PLAYBACK_STATE = "users/me/settings/playback-state";

        /// <summary>
        /// The route for the API endpoint that updates the playback state of the current user.
        /// </summary>
        public const string UPDATE_PLAYBACK_STATE = "users/me/settings/playback-state";
    }
}
