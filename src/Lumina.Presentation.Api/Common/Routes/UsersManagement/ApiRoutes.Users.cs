namespace Lumina.Presentation.Api.Common.Routes.UsersManagement;

/// <summary>
/// Class for the collection of routes defined in this API.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the Users route.
    /// </summary>
    public static class Users
    {
        /// <summary>
        /// The route for getting a user.
        /// </summary>
        public const string GET_USER_BY_ID = "/users/{id}";

        /// <summary>
        /// The route for getting the settings of the current user.
        /// </summary>
        public const string GET_USER_SETTINGS = "/users/me/settings"; // Uses "me" because it is always for the currently authenticated user.

        /// <summary>
        /// The route for updating the settings of the current user.
        /// </summary>
        public const string UPDATE_USER_SETTINGS = "/users/me/settings";

        /// <summary>
        /// The route for getting the playback state of the current user.
        /// </summary>
        public const string GET_PLAYBACK_STATE = "/users/me/settings/playback-state";

        /// <summary>
        /// The route for updating the playback state of the current user.
        /// </summary>
        public const string UPDATE_PLAYBACK_STATE = "/users/me/settings/playback-state";
    }
}
