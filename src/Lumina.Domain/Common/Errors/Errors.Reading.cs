#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Domain reading related error types.
/// </summary>
public static partial class Errors
{
    /// <summary>
    /// Groups the errors that report reading rule violations and missing reading resources.
    /// </summary>
    public static class Reading
    {
        /// <summary>
        /// Gets the error that reports that the reading location reference cannot be empty.
        /// </summary>
        public static Error LocationRefCannotBeEmpty => Error.Validation(description: nameof(LocationRefCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the reading resource key cannot be empty.
        /// </summary>
        public static Error ResourceKeyCannotBeEmpty => Error.Validation(description: nameof(ResourceKeyCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that the requested book was not found.
        /// </summary>
        public static Error BookNotFound => Error.NotFound(description: nameof(BookNotFound));

        /// <summary>
        /// Gets the error that reports that no book reader is available.
        /// </summary>
        public static Error NoReaderAvailable => Error.NotFound(description: nameof(NoReaderAvailable));

        /// <summary>
        /// Gets the error that reports that the requested book reader was not found because it is disabled.
        /// </summary>
        public static Error ReaderDisabled => Error.NotFound(description: nameof(ReaderDisabled));

        /// <summary>
        /// Gets the error that reports that the file of the requested book was not found.
        /// </summary>
        public static Error BookFileNotFound => Error.NotFound(description: nameof(BookFileNotFound));

        /// <summary>
        /// Gets the error that reports that the requested book section was not found.
        /// </summary>
        public static Error SectionNotFound => Error.NotFound(description: nameof(SectionNotFound));

        /// <summary>
        /// Gets the error that reports that the requested reading resource was not found.
        /// </summary>
        public static Error ResourceNotFound => Error.NotFound(description: nameof(ResourceNotFound));
    }
}
