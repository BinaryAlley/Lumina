#region ========================================================================= USING =====================================================================================
using ErrorOr;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Media contributor error types.
/// </summary>
public static partial class Errors
{
    public static class MediaContributor
    {
        #region ==================================================================== PROPERTIES =================================================================================

        public static Error ContributorsListCannotBeNull => Error.Validation(nameof(ContributorsListCannotBeNull)); 
        public static Error ContributorNameCannotBeEmpty => Error.Validation(nameof(ContributorNameCannotBeEmpty));
        public static Error ContributorNameMustBeMaximum100CharactersLong => Error.Validation(nameof(ContributorNameMustBeMaximum100CharactersLong));
        public static Error RoleNameCannotBeEmpty => Error.Validation(nameof(RoleNameCannotBeEmpty));
        public static Error RoleNameMustBeMaximum50CharactersLong => Error.Validation(nameof(RoleNameMustBeMaximum50CharactersLong));
        public static Error RoleCategoryCannotBeEmpty => Error.Validation(nameof(RoleCategoryCannotBeEmpty));
        public static Error RoleCategoryMustBeMaximum50CharactersLong => Error.Validation(nameof(RoleCategoryMustBeMaximum50CharactersLong));
        public static Error ContributorRoleCannotBeNull => Error.Validation(nameof(ContributorRoleCannotBeNull));
        #endregion
    }
}