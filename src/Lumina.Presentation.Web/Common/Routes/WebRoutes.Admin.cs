namespace Lumina.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of routes defined in the Web application.
/// </summary>
public static partial class WebRoutes
{
    /// <summary>
    /// Routes for the administrator pages.
    /// </summary>
    public static class Admin
    {
        public const string MANAGE_ROLES = "{culture}/admin/manage-roles";
        public const string MANAGE_PERMISSIONS = "{culture}/admin/manage-permissions";
        public const string MANAGE_THEMES = "{culture}/admin/manage-themes";
        public const string GET_PERMISSIONS_BY_ROLE_ID = "{culture}/admin/api-get-permissions-by-role-id/{roleId}";
        public const string GET_PERMISSIONS_BY_USER_ID = "{culture}/admin/api-get-permissions-by-user-id/{userId}";
        public const string GET_ROLE_BY_USER_ID = "{culture}/admin/api-get-role-by-user-id/{userId}";
        public const string GET_ROLES = "{culture}/admin/api-get-roles";
        public const string CREATE_ROLE = "{culture}/admin/api-create-role";
        public const string UPDATE_ROLE = "{culture}/admin/api-update-role";
        public const string DELETE_ROLE = "{culture}/admin/api-delete-role/{roleId}";
        public const string UPDATE_USER_AUTHORIZATION = "{culture}/admin/api-update-user-authorization";
    }
}
