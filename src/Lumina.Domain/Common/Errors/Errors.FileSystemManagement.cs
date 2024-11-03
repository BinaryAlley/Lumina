#region ========================================================================= USING =====================================================================================
using ErrorOr;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// File management error types.
/// </summary>
public static partial class Errors
{
    public static class FileSystemManagement
    {
        public static Error ParentNodeCannotBeNull => Error.Failure(description: nameof(ParentNodeCannotBeNull));
        public static Error PathMustBeMaximum260CharactersLong => Error.Failure(description: nameof(PathMustBeMaximum260CharactersLong));
        public static Error FileCopyError => Error.Failure(description: nameof(FileCopyError));
        public static Error FileMoveError => Error.Failure(description: nameof(FileMoveError));
        public static Error DirectoryCopyError => Error.Failure(description: nameof(DirectoryCopyError));
        public static Error DirectoryMoveError => Error.Failure(description: nameof(DirectoryMoveError));
        public static Error FileNotFound => Error.NotFound(description: nameof(FileNotFound));
        public static Error InvalidPath => Error.Validation(description: nameof(InvalidPath));
        public static Error PathCannotBeEmpty => Error.Validation(description: nameof(PathCannotBeEmpty));
        public static Error ExtensionCannotBeEmpty => Error.Validation(description: nameof(ExtensionCannotBeEmpty));
        public static Error StreamIdCannotBeEmpty => Error.Validation(description: nameof(StreamIdCannotBeEmpty));
        public static Error CodecCannotBeEmpty => Error.Validation(description: nameof(CodecCannotBeEmpty));
        public static Error BitrateMustBeAPositiveNumber => Error.Validation(description: nameof(BitrateMustBeAPositiveNumber));
        public static Error CannotNavigateUp => Error.Failure(description: nameof(CannotNavigateUp));
        public static Error NameCannotBeEmpty => Error.Validation(description: nameof(NameCannotBeEmpty));
        public static Error FileAlreadyExists => Error.Conflict(description: nameof(FileAlreadyExists));
        public static Error DirectoryNotFound => Error.NotFound(description: nameof(DirectoryNotFound));
        public static Error DirectoryAlreadyExists => Error.Conflict(description: nameof(DirectoryAlreadyExists));
    }
}
