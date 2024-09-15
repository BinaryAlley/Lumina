#region ========================================================================= USING =====================================================================================
using ErrorOr;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// File management error types.
/// </summary>
public static partial class Errors
{
    public static class FileManagement
    {
        #region ==================================================================== PROPERTIES =================================================================================
        public static Error ParentNodeCannotBeNull => Error.Failure(nameof(ParentNodeCannotBeNull));
        public static Error FileCopyError => Error.Failure(nameof(FileCopyError));
        public static Error FileMoveError => Error.Failure(nameof(FileMoveError));
        public static Error DirectoryCopyError => Error.Failure(nameof(DirectoryCopyError));
        public static Error DirectoryMoveError => Error.Failure(nameof(DirectoryMoveError));
        public static Error FileNotFound => Error.NotFound(nameof(FileNotFound));
        public static Error InvalidPath => Error.Validation(nameof(InvalidPath));
        public static Error PathCannotBeEmpty => Error.Validation(nameof(PathCannotBeEmpty));
        public static Error ExtensionCannotBeEmpty => Error.Validation(nameof(ExtensionCannotBeEmpty));
        public static Error StreamIdCannotBeEmpty => Error.Validation(nameof(StreamIdCannotBeEmpty));
        public static Error CodecCannotBeEmpty => Error.Validation(nameof(CodecCannotBeEmpty));
        public static Error BitrateMustBeAPositiveNumber => Error.Validation(nameof(BitrateMustBeAPositiveNumber));
        public static Error CannotNavigateUp => Error.Failure(nameof(CannotNavigateUp));
        public static Error NameCannotBeEmpty => Error.Validation(nameof(NameCannotBeEmpty));
        public static Error FileAlreadyExists => Error.Conflict(nameof(FileAlreadyExists));
        public static Error DirectoryNotFound => Error.NotFound(nameof(DirectoryNotFound));
        public static Error DirectoryAlreadyExists => Error.Conflict(nameof(DirectoryAlreadyExists));
        #endregion
    }
}