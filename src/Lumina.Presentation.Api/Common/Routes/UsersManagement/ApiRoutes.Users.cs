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
        public const string GET_USER_BY_ID = "/users/{id}";
        public const string GET_USER_SETTINGS = "/users/me/settings"; // uses "me" because it is always for the currently authenticated user
        public const string UPDATE_USER_SETTINGS = "/users/me/settings";
    }
}
