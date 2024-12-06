#region ========================================================================= USING =====================================================================================
using ErrorOr;
#endregion

namespace Lumina.Application.Common.Errors;

/// <summary>
/// Application authorization related error types.
/// </summary>
public static partial class Errors
{
    public static class Authorization
    {
        public static Error NotAuthorized => Error.Unauthorized(description: nameof(NotAuthorized));
        public static Error AdminAccountAlreadyCreated => Error.Unauthorized(description: nameof(AdminAccountAlreadyCreated));
        public static Error AdminAccountNotFound => Error.Unauthorized(description: nameof(AdminAccountNotFound));
        public static Error AdminRoleNotFound => Error.Unauthorized(description: nameof(AdminRoleNotFound));
        public static Error PermissionAlreadyExists => Error.Unauthorized(description: nameof(PermissionAlreadyExists));
        public static Error RoleAlreadyExists => Error.Unauthorized(description: nameof(RoleAlreadyExists));
    }
}
