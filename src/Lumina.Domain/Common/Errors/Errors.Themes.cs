#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Theme error types.
/// </summary>
public static partial class Errors
{
    /// <summary>
    /// Groups the errors that report theme rule violations and theme file failures.
    /// </summary>
    public static class Themes
    {
        /// <summary>
        /// Gets the error that reports that the requested theme was not found.
        /// </summary>
        public static Error ThemeNotFound => Error.NotFound(description: nameof(ThemeNotFound));

        /// <summary>
        /// Gets the error that reports that the theme identifier cannot be empty.
        /// </summary>
        public static Error ThemeIdCannotBeEmpty => Error.Validation(description: nameof(ThemeIdCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the theme page key cannot be empty.
        /// </summary>
        public static Error PageKeyCannotBeEmpty => Error.Validation(description: nameof(PageKeyCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the theme asset path cannot be empty.
        /// </summary>
        public static Error ThemeAssetPathCannotBeEmpty => Error.Validation(description: nameof(ThemeAssetPathCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the theme archive must not be <see langword="null"/>.
        /// </summary>
        public static Error ThemeArchiveCannotBeNull => Error.Validation(description: nameof(ThemeArchiveCannotBeNull));

        /// <summary>
        /// Gets the error that reports that the last bundled theme cannot be deleted.
        /// </summary>
        public static Error LastBundledThemeCannotBeDeleted => Error.Forbidden(description: nameof(LastBundledThemeCannotBeDeleted));

        /// <summary>
        /// Gets the error that reports that the theme cannot be deleted.
        /// </summary>
        public static Error ThemeCannotBeDeleted => Error.Forbidden(description: nameof(ThemeCannotBeDeleted));

        /// <summary>
        /// Gets the error that reports that the theme cannot be restored.
        /// </summary>
        public static Error ThemeCannotBeRestored => Error.Forbidden(description: nameof(ThemeCannotBeRestored));

        /// <summary>
        /// Gets the error that reports that the theme archive could not be read.
        /// </summary>
        public static Error ThemeArchiveNotReadable => Error.Failure(description: nameof(ThemeArchiveNotReadable));

        /// <summary>
        /// Gets the error that reports that the files of the theme could not be read.
        /// </summary>
        public static Error ThemeFilesUnreadable => Error.Failure(description: nameof(ThemeFilesUnreadable));

        /// <summary>
        /// Gets the error that reports that the requested theme template was not found.
        /// </summary>
        public static Error ThemeTemplateNotFound => Error.NotFound(description: nameof(ThemeTemplateNotFound));

        /// <summary>
        /// Gets the error that reports that the theme is not available for the requested page.
        /// </summary>
        public static Error ThemeNotAvailable => Error.Failure(description: nameof(ThemeNotAvailable));
    }
}
