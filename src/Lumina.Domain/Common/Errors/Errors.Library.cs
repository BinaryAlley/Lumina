#region ========================================================================= USING =====================================================================================
using ErrorOr;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Media Library error types.
/// </summary>
public static partial class Errors
{
    public static class Library
    {     
        public static Error UserIdCannotBeEmpty => Error.Forbidden(description: nameof(UserIdCannotBeEmpty));
        public static Error LibraryAlreadyExists => Error.Conflict(description: nameof(LibraryAlreadyExists));
        public static Error LibraryTypeCannotBeNull => Error.Validation(description: nameof(LibraryTypeCannotBeNull));
        public static Error UnknownLibraryType => Error.Unexpected(description: nameof(UnknownLibraryType));
        public static Error PathsListCannotBeNull => Error.Validation(description: nameof(PathsListCannotBeNull));
        public static Error PathsListCannotBeEmpty => Error.Validation(description: nameof(PathsListCannotBeEmpty));
        public static Error TitleCannotBeEmpty => Error.Validation(description: nameof(TitleCannotBeEmpty));
        public static Error TitleMustBeMaximum255CharactersLong => Error.Validation(description: nameof(TitleMustBeMaximum255CharactersLong));
    }
}
