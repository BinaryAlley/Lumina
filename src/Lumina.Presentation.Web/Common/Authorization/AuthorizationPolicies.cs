namespace Lumina.Presentation.Web.Common.Authorization;

/// <summary>
/// Contains the names of the authorization policies defined in the Web application.
/// </summary>
public static class AuthorizationPolicies
{
    public const string REQUIRE_ADMIN_ROLE = "RequireAdminRole";
    public const string REQUIRE_CREATE_LIBRARIES_PERMISSION = "RequireCreateLibrariesPermission";
    public const string REQUIRE_INITIALIZATION = "RequireInitialization";
}
