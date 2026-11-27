namespace Nemera.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of remote API routes called by this Web application.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the authorization endpoints of the remote API.
    /// </summary>
    public static class Authorization
    {
        /// <summary>
        /// The route for the API endpoint that updates the role and permissions of a user.
        /// </summary>
        public const string UPDATE_USER_ROLE_AND_PERMISSIONS_BY_USER_ID = "auth/users/{userId}/role-and-permissions";

        /// <summary>
        /// The route for the API endpoint that retrieves the permissions of a user.
        /// </summary>
        public const string GET_USER_PERMISSIONS_BY_USER_ID = "auth/users/{userId}/permissions";

        /// <summary>
        /// The route for the API endpoint that retrieves the role of a user.
        /// </summary>
        public const string GET_USER_ROLE_BY_USER_ID = "auth/users/{userId}/role";

        /// <summary>
        /// The route for the authorization retrieval API endpoint.
        /// </summary>
        public const string GET_AUTHORIZATION = "auth/get-authorization";
    }
}
