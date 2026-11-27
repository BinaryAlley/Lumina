namespace Nemera.Presentation.Web.Common.Enums.Authorization;

/// <summary>
/// Enumeration for authorization permissions.
/// </summary>
public enum AuthorizationPermission
{
    /// <summary>
    /// No permission is granted.
    /// </summary>
    None = 0,

    /// <summary>
    /// Permission to view the existing users.
    /// </summary>
    CanViewUsers,

    /// <summary>
    /// Permission to delete the existing users.
    /// </summary>
    CanDeleteUsers,

    /// <summary>
    /// Permission to register new users.
    /// </summary>
    CanRegisterUsers,

    /// <summary>
    /// Permission to create and manage media libraries.
    /// </summary>
    CanCreateLibraries
}
