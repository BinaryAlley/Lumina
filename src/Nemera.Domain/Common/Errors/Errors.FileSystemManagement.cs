#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Primitives;
#endregion

namespace Nemera.Domain.Common.Errors;

/// <summary>
/// File management error types.
/// </summary>
public static partial class Errors
{
    /// <summary>
    /// Groups the errors that report file system navigation, validation and operation failures.
    /// </summary>
    public static class FileSystemManagement
    {
        /// <summary>
        /// Gets the error that reports that a file system operation failed because its parent node is <see langword="null"/>.
        /// </summary>
        public static Error ParentNodeCannotBeNull => Error.Failure(description: nameof(ParentNodeCannotBeNull));

        /// <summary>
        /// Gets the error that reports that a file system operation failed because the path exceeds the maximum length of 260 characters.
        /// </summary>
        public static Error PathMustBeMaximum260CharactersLong => Error.Failure(description: nameof(PathMustBeMaximum260CharactersLong));

        /// <summary>
        /// Gets the error that reports that copying a file failed.
        /// </summary>
        public static Error FileCopyError => Error.Failure(description: nameof(FileCopyError));

        /// <summary>
        /// Gets the error that reports that moving a file failed.
        /// </summary>
        public static Error FileMoveError => Error.Failure(description: nameof(FileMoveError));

        /// <summary>
        /// Gets the error that reports that deleting a file failed.
        /// </summary>
        public static Error FileDeleteError => Error.Failure(description: nameof(FileDeleteError));

        /// <summary>
        /// Gets the error that reports that copying a directory failed.
        /// </summary>
        public static Error DirectoryCopyError => Error.Failure(description: nameof(DirectoryCopyError));

        /// <summary>
        /// Gets the error that reports that moving a directory failed.
        /// </summary>
        public static Error DirectoryMoveError => Error.Failure(description: nameof(DirectoryMoveError));

        /// <summary>
        /// Gets the error that reports that the requested file was not found.
        /// </summary>
        public static Error FileNotFound => Error.NotFound(description: nameof(FileNotFound));

        /// <summary>
        /// Gets the error that reports that the file system path is not valid.
        /// </summary>
        public static Error InvalidPath => Error.Validation(description: nameof(InvalidPath));

        /// <summary>
        /// Gets the error that reports that the file system path cannot be empty.
        /// </summary>
        public static Error PathCannotBeEmpty => Error.Validation(description: nameof(PathCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the file extension cannot be empty.
        /// </summary>
        public static Error ExtensionCannotBeEmpty => Error.Validation(description: nameof(ExtensionCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the stream identifier cannot be empty.
        /// </summary>
        public static Error StreamIdCannotBeEmpty => Error.Validation(description: nameof(StreamIdCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the codec name cannot be empty.
        /// </summary>
        public static Error CodecCannotBeEmpty => Error.Validation(description: nameof(CodecCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the bitrate must be a positive number.
        /// </summary>
        public static Error BitrateMustBeAPositiveNumber => Error.Validation(description: nameof(BitrateMustBeAPositiveNumber));

        /// <summary>
        /// Gets the error that reports that the file system browser cannot navigate above the root directory.
        /// </summary>
        public static Error CannotNavigateUp => Error.Failure(description: nameof(CannotNavigateUp));

        /// <summary>
        /// Gets the error that reports that the name of the file system node cannot be empty.
        /// </summary>
        public static Error NameCannotBeEmpty => Error.Validation(description: nameof(NameCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the file name cannot be empty.
        /// </summary>
        public static Error FileNameCannotBeEmpty => Error.Validation(description: nameof(FileNameCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that a file with the same name already exists in the target directory.
        /// </summary>
        public static Error FileAlreadyExists => Error.Conflict(description: nameof(FileAlreadyExists));

        /// <summary>
        /// Gets the error that reports that the requested directory was not found.
        /// </summary>
        public static Error DirectoryNotFound => Error.NotFound(description: nameof(DirectoryNotFound));

        /// <summary>
        /// Gets the error that reports that a directory with the same name already exists in the target location.
        /// </summary>
        public static Error DirectoryAlreadyExists => Error.Conflict(description: nameof(DirectoryAlreadyExists));

        /// <summary>
        /// Gets the error that reports that the file exceeds the maximum allowed size.
        /// </summary>
        public static Error FileTooLarge => Error.Validation(description: nameof(FileTooLarge));
    }
}
