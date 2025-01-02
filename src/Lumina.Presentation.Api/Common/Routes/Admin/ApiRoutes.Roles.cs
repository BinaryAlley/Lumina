namespace Lumina.Presentation.Api.Common.Routes.UsersManagement;

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
        public const string GET_ROLE_PERMISSIONS_BY_ROLE_ID = "/auth/roles/{roleId}/permissions";
        public const string GET_ROLES = "/auth/roles";
        public const string CREATE_ROLE = "/auth/roles";
        public const string UPDATE_ROLE = "/auth/roles";
        public const string DELETE_ROLE = "/auth/roles/{roleId}";
    }
}
