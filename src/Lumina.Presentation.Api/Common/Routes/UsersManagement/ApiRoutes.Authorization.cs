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
        /// <summary>
        /// The route for updating the role and permissions of a user.
        /// </summary>
        public const string UPDATE_USER_ROLE_AND_PERMISSIONS_BY_USER_ID = "/auth/users/{userId}/role-and-permissions";

        /// <summary>
        /// The route for getting the permissions of a user.
        /// </summary>
        public const string GET_USER_PERMISSIONS_BY_USER_ID = "/auth/users/{userId}/permissions";

        /// <summary>
        /// The route for getting the role of a user.
        /// </summary>
        public const string GET_USER_ROLE_BY_USER_ID = "/auth/users/{userId}/role";

        /// <summary>
        /// The route for getting the authorization roles and permissions of an account.
        /// </summary>
        public const string GET_AUTHORIZATION = "/auth/get-authorization";
    }
}
