namespace Lumina.Presentation.Api.Common.Routes.UsersManagement;

/// <summary>
/// Class for the collection of routes defined in this API.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the Authorization route.
    /// </summary>
    public static class Authorization
    {
        public const string UPDATE_USER_ROLE_AND_PERMISSIONS_BY_USER_ID = "/auth/users/{userId}/role-and-permissions";
        public const string GET_USER_PERMISSIONS_BY_USER_ID = "/auth/users/{userId}/permissions";
        public const string GET_USER_ROLE_BY_USER_ID = "/auth/users/{userId}/role";
        public const string GET_AUTHORIZATION = "/auth/get-authorization";
    }
}
