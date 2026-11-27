namespace Nemera.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of remote API routes called by this Web application.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the roles endpoints of the remote API.
    /// </summary>
    public static class Roles
    {
        /// <summary>
        /// The route for the API endpoint that retrieves the permissions of a role.
        /// </summary>
        public const string GET_ROLE_PERMISSIONS_BY_ROLE_ID = "auth/roles/{roleId}/permissions";

        /// <summary>
        /// The route for the API endpoint that retrieves the roles.
        /// </summary>
        public const string GET_ROLES = "auth/roles";

        /// <summary>
        /// The route for the API endpoint that creates a role.
        /// </summary>
        public const string CREATE_ROLE = "auth/roles";

        /// <summary>
        /// The route for the API endpoint that updates a role.
        /// </summary>
        public const string UPDATE_ROLE = "auth/roles";

        /// <summary>
        /// The route for the API endpoint that deletes a role.
        /// </summary>
        public const string DELETE_ROLE = "auth/roles/{roleId}";
    }
}
