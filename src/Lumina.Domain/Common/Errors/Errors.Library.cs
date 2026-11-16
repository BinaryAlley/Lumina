#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Media Library error types.
/// </summary>
public static partial class Errors
{
    /// <summary>
    /// Groups the errors that report media library rule violations and missing libraries.
    /// </summary>
    public static class Library
    {     
        /// <summary>
        /// Gets the error that reports that a locked media library cannot be scanned.
        /// </summary>
        public static Error CannotScanLockedLibrary => Error.Forbidden(description: nameof(CannotScanLockedLibrary));

        /// <summary>
        /// Gets the error that reports that a disabled media library cannot be scanned.
        /// </summary>
        public static Error CannotScanDisabledLibrary => Error.Forbidden(description: nameof(CannotScanDisabledLibrary));

        /// <summary>
        /// Gets the error that reports that the library identifier cannot be empty.
        /// </summary>
        public static Error LibraryIdCannotBeEmpty => Error.Validation(description: nameof(LibraryIdCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that a library-scoped filter must include a library identifier.
        /// </summary>
        public static Error FilterMustIncludeLibraryId => Error.Validation(description: nameof(FilterMustIncludeLibraryId));

        /// <summary>
        /// Gets the error that reports that the alphabetical filter key is not valid.
        /// </summary>
        public static Error InvalidFilterAlphaKey => Error.Validation(description: nameof(InvalidFilterAlphaKey));

        /// <summary>
        /// Gets the error that reports that a media library with the same identifier already exists.
        /// </summary>
        public static Error LibraryAlreadyExists => Error.Conflict(description: nameof(LibraryAlreadyExists));

        /// <summary>
        /// Gets the error that reports that the requested media library was not found.
        /// </summary>
        public static Error LibraryNotFound => Error.NotFound(description: nameof(LibraryNotFound));

        /// <summary>
        /// Gets the error that reports that the library type must not be <see langword="null"/>.
        /// </summary>
        public static Error LibraryTypeCannotBeNull => Error.Validation(description: nameof(LibraryTypeCannotBeNull));

        /// <summary>
        /// Gets the error that reports that the library cover file must be an image.
        /// </summary>
        public static Error CoverFileMustBeAnImage => Error.Validation(description: nameof(CoverFileMustBeAnImage));

        /// <summary>
        /// Gets the error that reports that the operation cannot be performed because the library type is not recognized.
        /// </summary>
        public static Error UnknownLibraryType => Error.Forbidden(description: nameof(UnknownLibraryType));

        /// <summary>
        /// Gets the error that reports that the list of library content paths must not be <see langword="null"/>.
        /// </summary>
        public static Error PathsListCannotBeNull => Error.Validation(description: nameof(PathsListCannotBeNull));

        /// <summary>
        /// Gets the error that reports that the list of library content paths cannot be empty.
        /// </summary>
        public static Error PathsListCannotBeEmpty => Error.Validation(description: nameof(PathsListCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the library title cannot be empty.
        /// </summary>
        public static Error TitleCannotBeEmpty => Error.Validation(description: nameof(TitleCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the library title exceeds the maximum length of 255 characters.
        /// </summary>
        public static Error TitleMustBeMaximum255CharactersLong => Error.Validation(description: nameof(TitleMustBeMaximum255CharactersLong));

        /// <summary>
        /// Gets the error that reports that a literal part of the path template cannot be empty.
        /// </summary>
        public static Error PathTemplateLiteralCannotBeEmpty => Error.Validation(description: nameof(PathTemplateLiteralCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that a value part of the path template must contain exactly one placeholder.
        /// </summary>
        public static Error PathTemplateValuePartMustContainSinglePlaceholder => Error.Validation(description: nameof(PathTemplateValuePartMustContainSinglePlaceholder));

        /// <summary>
        /// Gets the error that reports that a path template part kind is not supported for the library type.
        /// </summary>
        public static Error PathTemplatePartKindNotSupportedForLibraryType => Error.Validation(description: nameof(PathTemplatePartKindNotSupportedForLibraryType));

        /// <summary>
        /// Gets the error that reports that path templates are not supported for the library type.
        /// </summary>
        public static Error PathTemplateIsNotSupportedForLibraryType => Error.Validation(description: nameof(PathTemplateIsNotSupportedForLibraryType));

        /// <summary>
        /// Gets the error that reports that the completion threshold percentage must be omitted when progress tracking is disabled.
        /// </summary>
        public static Error CompletionThresholdPercentageMustBeNullWhenProgressTrackingIsOff => Error.Validation(description: nameof(CompletionThresholdPercentageMustBeNullWhenProgressTrackingIsOff));

        /// <summary>
        /// Gets the error that reports that the completion threshold percentage must be between 0 and 100.
        /// </summary>
        public static Error CompletionThresholdPercentageMustBeBetweenZeroAndOneHundred => Error.Validation(description: nameof(CompletionThresholdPercentageMustBeBetweenZeroAndOneHundred));
    }
}
