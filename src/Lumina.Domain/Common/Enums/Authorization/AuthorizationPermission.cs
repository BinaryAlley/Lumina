namespace Lumina.Domain.Common.Enums.Authorization;

/// <summary>
/// Enumeration for authorization permissions.
/// </summary>
public enum AuthorizationPermission
{
    None = 0,
    CanViewUsers,
    CanDeleteUsers,
    CanRegisterUsers,
    CanCreateLibraries
}
