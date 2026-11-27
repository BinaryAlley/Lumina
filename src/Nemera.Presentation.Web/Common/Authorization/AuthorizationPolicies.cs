namespace Lumina.Presentation.Web.Common.Authorization;

/// <summary>
/// Contains the names of the authorization policies defined in the Web application.
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>
    /// The name of the authorization policy that requires the administrator role.
    /// </summary>
    public const string REQUIRE_ADMIN_ROLE = "RequireAdminRole";

    /// <summary>
    /// The name of the authorization policy that requires the permission to create media libraries.
    /// </summary>
    public const string REQUIRE_CREATE_LIBRARIES_PERMISSION = "RequireCreateLibrariesPermission";

    /// <summary>
    /// The name of the authorization policy that requires the application to be initialized.
    /// </summary>
    public const string REQUIRE_INITIALIZATION = "RequireInitialization";
}
