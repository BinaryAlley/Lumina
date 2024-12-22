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
        public static Error AdminAccountAlreadyCreated => Error.Conflict(description: nameof(AdminAccountAlreadyCreated));
        public static Error AdminAccountNotFound => Error.NotFound(description: nameof(AdminAccountNotFound));
        public static Error AdminRoleNotFound => Error.NotFound(description: nameof(AdminRoleNotFound));
        public static Error AdminRoleCannotBeDeleted => Error.Forbidden(description: nameof(AdminRoleCannotBeDeleted));
        public static Error RoleNotFound => Error.NotFound(description: nameof(RoleNotFound));
        public static Error PermissionAlreadyExists => Error.Unauthorized(description: nameof(PermissionAlreadyExists));
        public static Error RoleAlreadyExists => Error.Conflict(description: nameof(RoleAlreadyExists));
        public static Error RoleIdCannotBeEmpty => Error.Validation(description: nameof(RoleIdCannotBeEmpty));
        public static Error PermissionsListCannotBeNull => Error.Validation(description: nameof(PermissionsListCannotBeNull));
        public static Error PermissionsListCannotBeEmpty => Error.Validation(description: nameof(PermissionsListCannotBeEmpty));
        public static Error PermissionIdCannotBeEmpty => Error.Validation(description: nameof(PermissionIdCannotBeEmpty));
        public static Error RoleNameCannotBeEmpty => Error.Validation(description: nameof(RoleNameCannotBeEmpty));
        public static Error RoleNameCannotBeNull => Error.Validation(description: nameof(RoleNameCannotBeNull));
        public static Error CannotRemoveLastAdmin => Error.Forbidden(description: nameof(CannotRemoveLastAdmin));
        public static Error UserMustHaveRoleOrDirectPermissions => Error.Forbidden(description: nameof(UserMustHaveRoleOrDirectPermissions));
    }
}
