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
        public const string GET_USER_SETTINGS = "users/me/settings";
        public const string UPDATE_USER_SETTINGS = "users/me/settings";
    }
}
