namespace Nemera.Presentation.Api.Common.Routes.UsersManagement;

/// <summary>
/// Class for the collection of routes defined in this API.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the Roles route.
    /// </summary>
    public static class Roles
    {
        /// <summary>
        /// The route for getting the permissions granted to a role.
        /// </summary>
        public const string GET_ROLE_PERMISSIONS_BY_ROLE_ID = "/auth/roles/{roleId}/permissions";

        /// <summary>
        /// The route for getting the roles.
        /// </summary>
        public const string GET_ROLES = "/auth/roles";

        /// <summary>
        /// The route for creating a role.
        /// </summary>
        public const string CREATE_ROLE = "/auth/roles";

        /// <summary>
        /// The route for updating a role.
        /// </summary>
        public const string UPDATE_ROLE = "/auth/roles";

        /// <summary>
        /// The route for deleting a role.
        /// </summary>
        public const string DELETE_ROLE = "/auth/roles/{roleId}";
    }
}
