#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Theme error types.
/// </summary>
public static partial class Errors
{
    public static class Themes
    {
        public static Error ThemeNotFound => Error.NotFound(description: nameof(ThemeNotFound));
        public static Error ThemeIdCannotBeEmpty => Error.Validation(description: nameof(ThemeIdCannotBeEmpty));
        public static Error PageKeyCannotBeEmpty => Error.Validation(description: nameof(PageKeyCannotBeEmpty));
        public static Error ThemeAssetPathCannotBeEmpty => Error.Validation(description: nameof(ThemeAssetPathCannotBeEmpty));
        public static Error ThemeArchiveCannotBeNull => Error.Validation(description: nameof(ThemeArchiveCannotBeNull));
        public static Error LastBundledThemeCannotBeDeleted => Error.Forbidden(description: nameof(LastBundledThemeCannotBeDeleted));
        public static Error ThemeCannotBeDeleted => Error.Forbidden(description: nameof(ThemeCannotBeDeleted));
        public static Error ThemeCannotBeRestored => Error.Forbidden(description: nameof(ThemeCannotBeRestored));
        public static Error ThemeArchiveNotReadable => Error.Failure(description: nameof(ThemeArchiveNotReadable));
        public static Error ThemeFilesUnreadable => Error.Failure(description: nameof(ThemeFilesUnreadable));
        public static Error ThemeNotAvailable => Error.Failure(description: nameof(ThemeNotAvailable));
    }
}
