namespace Nemera.Presentation.Web.Common.Routes;

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
        /// <summary>
        /// The route for the role management page.
        /// </summary>
        public const string MANAGE_ROLES = "{culture}/admin/manage-roles";

        /// <summary>
        /// The route for the permission management page.
        /// </summary>
        public const string MANAGE_PERMISSIONS = "{culture}/admin/manage-permissions";

        /// <summary>
        /// The route for the theme management page.
        /// </summary>
        public const string MANAGE_THEMES = "{culture}/admin/manage-themes";

        /// <summary>
        /// The route for the action that retrieves the permissions of a role.
        /// </summary>
        public const string GET_PERMISSIONS_BY_ROLE_ID = "{culture}/admin/api-get-permissions-by-role-id/{roleId}";

        /// <summary>
        /// The route for the action that retrieves the permissions of a user.
        /// </summary>
        public const string GET_PERMISSIONS_BY_USER_ID = "{culture}/admin/api-get-permissions-by-user-id/{userId}";

        /// <summary>
        /// The route for the action that retrieves the role of a user.
        /// </summary>
        public const string GET_ROLE_BY_USER_ID = "{culture}/admin/api-get-role-by-user-id/{userId}";

        /// <summary>
        /// The route for the action that retrieves the roles.
        /// </summary>
        public const string GET_ROLES = "{culture}/admin/api-get-roles";

        /// <summary>
        /// The route for the action that creates a role.
        /// </summary>
        public const string CREATE_ROLE = "{culture}/admin/api-create-role";

        /// <summary>
        /// The route for the action that updates a role.
        /// </summary>
        public const string UPDATE_ROLE = "{culture}/admin/api-update-role";

        /// <summary>
        /// The route for the action that deletes a role.
        /// </summary>
        public const string DELETE_ROLE = "{culture}/admin/api-delete-role/{roleId}";

        /// <summary>
        /// The route for the action that updates the authorization of a user.
        /// </summary>
        public const string UPDATE_USER_AUTHORIZATION = "{culture}/admin/api-update-user-authorization";
    }
}
