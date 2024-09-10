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
        public static Error FileCopyError => Error.Failure(nameof(FileCopyError));
        public static Error FileMoveError => Error.Failure(nameof(FileMoveError));
        public static Error DirectoryCopyError => Error.Failure(nameof(DirectoryCopyError));
        public static Error DirectoryMoveError => Error.Failure(nameof(DirectoryMoveError));
        public static Error FileNotFoundError => Error.NotFound(nameof(FileNotFoundError));
        public static Error InvalidPathError => Error.Validation(nameof(InvalidPathError));
        public static Error PathCannotBeEmpty => Error.Validation(nameof(PathCannotBeEmpty));
        public static Error ExtensionCannotBeEmpty => Error.Validation(nameof(ExtensionCannotBeEmpty));
        public static Error StreamIdCannotBeEmpty => Error.Validation(nameof(StreamIdCannotBeEmpty));
        public static Error CodecCannotBeEmpty => Error.Validation(nameof(CodecCannotBeEmpty));
        public static Error BitrateMustBeAPositiveNumber => Error.Validation(nameof(BitrateMustBeAPositiveNumber));
        public static Error CannotNavigateUpError => Error.Failure(nameof(CannotNavigateUpError));
        public static Error NameCannotBeEmptyError => Error.Validation(nameof(NameCannotBeEmptyError));
        public static Error FileAlreadyExistsError => Error.Conflict(nameof(FileAlreadyExistsError));
        public static Error DirectoryNotFoundError => Error.NotFound(nameof(DirectoryNotFoundError));
        public static Error DirectoryAlreadyExistsError => Error.Conflict(nameof(DirectoryAlreadyExistsError));
        #endregion
    }
}