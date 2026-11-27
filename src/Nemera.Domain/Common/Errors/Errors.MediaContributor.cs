#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Media contributor error types.
/// </summary>
public static partial class Errors
{
    /// <summary>
    /// Groups the errors that report media contributor rule violations and missing contributors.
    /// </summary>
    public static class MediaContributor
    {     
        /// <summary>
        /// Gets the error that reports that the contributor name cannot be empty.
        /// </summary>
        public static Error ContributorNameCannotBeEmpty => Error.Validation(description: nameof(ContributorNameCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the contributor display name cannot be empty.
        /// </summary>
        public static Error ContributorDisplayNameCannotBeEmpty => Error.Validation(description: nameof(ContributorDisplayNameCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the contributor display name exceeds the maximum length of 100 characters.
        /// </summary>
        public static Error ContributorDisplayNameMustBeMaximum100CharactersLong => Error.Validation(description: nameof(ContributorDisplayNameMustBeMaximum100CharactersLong));

        /// <summary>
        /// Gets the error that reports that the contributor legal name exceeds the maximum length of 100 characters.
        /// </summary>
        public static Error ContributorLegalNameMustBeMaximum100CharactersLong => Error.Validation(description: nameof(ContributorLegalNameMustBeMaximum100CharactersLong));

        /// <summary>
        /// Gets the error that reports that the contributor role name cannot be empty.
        /// </summary>
        public static Error RoleNameCannotBeEmpty => Error.Validation(description: nameof(RoleNameCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the contributor role name exceeds the maximum length of 50 characters.
        /// </summary>
        public static Error RoleNameMustBeMaximum50CharactersLong => Error.Validation(description: nameof(RoleNameMustBeMaximum50CharactersLong));

        /// <summary>
        /// Gets the error that reports that the contributor role category cannot be empty.
        /// </summary>
        public static Error RoleCategoryCannotBeEmpty => Error.Validation(description: nameof(RoleCategoryCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the contributor role must not be <see langword="null"/>.
        /// </summary>
        public static Error ContributorRoleCannotBeNull => Error.Validation(description: nameof(ContributorRoleCannotBeNull));

        /// <summary>
        /// Gets the error that reports that the contributors list must not be <see langword="null"/>.
        /// </summary>
        public static Error ContributorsListCannotBeNull => Error.Validation(description: nameof(ContributorsListCannotBeNull));

        /// <summary>
        /// Gets the error that reports that the media contributor identifier cannot be empty.
        /// </summary>
        public static Error MediaContributorIdCannotBeEmpty => Error.Validation(description: nameof(MediaContributorIdCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the requested media contributor was not found.
        /// </summary>
        public static Error MediaContributorNotFound => Error.NotFound(description: nameof(MediaContributorNotFound));

        /// <summary>
        /// Gets the error that reports that the media contributor already exists.
        /// </summary>
        public static Error MediaContributorAlreadyExists => Error.Conflict(description: nameof(MediaContributorAlreadyExists));

        /// <summary>
        /// Gets the error that reports that the media contributor role is not a defined value.
        /// </summary>
        public static Error UnknownMediaContributorRole => Error.Validation(description: nameof(UnknownMediaContributorRole));
    }
}
