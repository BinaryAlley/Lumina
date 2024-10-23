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
        public static Error ContributorsListCannotBeNull => Error.Validation(description: nameof(ContributorsListCannotBeNull));
        public static Error ContributorNameCannotBeEmpty => Error.Validation(description: nameof(ContributorNameCannotBeEmpty));
        public static Error ContributorDisplayNameCannotBeEmpty => Error.Validation(description: nameof(ContributorDisplayNameCannotBeEmpty));
        public static Error ContributorDisplayNameMustBeMaximum100CharactersLong => Error.Validation(description: nameof(ContributorDisplayNameMustBeMaximum100CharactersLong));
        public static Error ContributorLegalNameMustBeMaximum100CharactersLong => Error.Validation(description: nameof(ContributorLegalNameMustBeMaximum100CharactersLong));
        public static Error RoleNameCannotBeEmpty => Error.Validation(description: nameof(RoleNameCannotBeEmpty));
        public static Error RoleNameMustBeMaximum50CharactersLong => Error.Validation(description: nameof(RoleNameMustBeMaximum50CharactersLong));
        public static Error RoleCategoryCannotBeEmpty => Error.Validation(description: nameof(RoleCategoryCannotBeEmpty));
        public static Error RoleCategoryMustBeMaximum50CharactersLong => Error.Validation(description: nameof(RoleCategoryMustBeMaximum50CharactersLong));
        public static Error ContributorRoleCannotBeNull => Error.Validation(description: nameof(ContributorRoleCannotBeNull));
    }
}
