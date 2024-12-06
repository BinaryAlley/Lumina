namespace Lumina.Domain.Common.Enums.Authorization;

/// <summary>
/// Enumeration for authorization permissions.
/// </summary>
public enum AuthorizationPermission
{
    None = 0,
    CanViewUsers = 1,
    CanDeleteUsers = 2,
    CanRegisterUsers = 3
}
