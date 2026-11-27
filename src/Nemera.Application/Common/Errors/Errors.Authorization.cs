#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Application.Common.Errors;

/// <summary>
/// Application authorization related error types.
/// </summary>
public static partial class Errors
{
    /// <summary>
    /// Error types for application authorization operations.
    /// </summary>
    public static class Authorization
    {
        /// <summary>
        /// Gets the error that reports that the caller is not authorized to perform the operation.
        /// </summary>
        public static Error NotAuthorized => Error.Unauthorized(description: nameof(NotAuthorized));

        /// <summary>
        /// Gets the error that reports that the administrator account has already been created.
        /// </summary>
        public static Error AdminAccountAlreadyCreated => Error.Conflict(description: nameof(AdminAccountAlreadyCreated));

        /// <summary>
        /// Gets the error that reports that the administrator account does not exist.
        /// </summary>
        public static Error AdminAccountNotFound => Error.NotFound(description: nameof(AdminAccountNotFound));

        /// <summary>
        /// Gets the error that reports that the administrator role does not exist.
        /// </summary>
        public static Error AdminRoleNotFound => Error.NotFound(description: nameof(AdminRoleNotFound));

        /// <summary>
        /// Gets the error that reports that the administrator role cannot be deleted.
        /// </summary>
        public static Error AdminRoleCannotBeDeleted => Error.Forbidden(description: nameof(AdminRoleCannotBeDeleted));

        /// <summary>
        /// Gets the error that reports that a role does not exist.
        /// </summary>
        public static Error RoleNotFound => Error.NotFound(description: nameof(RoleNotFound));

        /// <summary>
        /// Gets the error that reports that a permission does not exist.
        /// </summary>
        public static Error PermissionNotFound => Error.NotFound(description: nameof(PermissionNotFound));

        /// <summary>
        /// Gets the error that reports that a permission already exists.
        /// </summary>
        public static Error PermissionAlreadyExists => Error.Conflict(description: nameof(PermissionAlreadyExists));

        /// <summary>
        /// Gets the error that reports that a role already exists.
        /// </summary>
        public static Error RoleAlreadyExists => Error.Conflict(description: nameof(RoleAlreadyExists));

        /// <summary>
        /// Gets the error that reports that a role identifier cannot be empty.
        /// </summary>
        public static Error RoleIdCannotBeEmpty => Error.Validation(description: nameof(RoleIdCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that a permissions list cannot be <see langword="null"/>.
        /// </summary>
        public static Error PermissionsListCannotBeNull => Error.Validation(description: nameof(PermissionsListCannotBeNull));

        /// <summary>
        /// Gets the error that reports that a permissions list cannot be empty.
        /// </summary>
        public static Error PermissionsListCannotBeEmpty => Error.Validation(description: nameof(PermissionsListCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that a permission identifier cannot be empty.
        /// </summary>
        public static Error PermissionIdCannotBeEmpty => Error.Validation(description: nameof(PermissionIdCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that a role name cannot be empty.
        /// </summary>
        public static Error RoleNameCannotBeEmpty => Error.Validation(description: nameof(RoleNameCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that a role name cannot be <see langword="null"/>.
        /// </summary>
        public static Error RoleNameCannotBeNull => Error.Validation(description: nameof(RoleNameCannotBeNull));

        /// <summary>
        /// Gets the error that reports that the last administrator account cannot be removed.
        /// </summary>
        public static Error CannotRemoveLastAdmin => Error.Forbidden(description: nameof(CannotRemoveLastAdmin));

        /// <summary>
        /// Gets the error that reports that a user must have a role or direct permissions.
        /// </summary>
        public static Error UserMustHaveRoleOrDirectPermissions => Error.Forbidden(description: nameof(UserMustHaveRoleOrDirectPermissions));
    }
}
