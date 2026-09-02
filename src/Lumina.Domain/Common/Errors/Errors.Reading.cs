#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Domain reading related error types.
/// </summary>
public static partial class Errors
{
    public static class Reading
    {
        public static Error BookIdCannotBeEmpty => Error.Validation(description: nameof(BookIdCannotBeEmpty));
        public static Error LocationRefCannotBeEmpty => Error.Validation(description: nameof(LocationRefCannotBeEmpty));
        public static Error ResourceKeyCannotBeEmpty => Error.Validation(description: nameof(ResourceKeyCannotBeEmpty));
        public static Error BookNotFound => Error.NotFound(description: nameof(BookNotFound));
        public static Error NoReaderAvailable => Error.NotFound(description: nameof(NoReaderAvailable));
        public static Error ReaderDisabled => Error.NotFound(description: nameof(ReaderDisabled));
        public static Error BookFileNotFound => Error.NotFound(description: nameof(BookFileNotFound));
        public static Error SectionNotFound => Error.NotFound(description: nameof(SectionNotFound));
        public static Error ResourceNotFound => Error.NotFound(description: nameof(ResourceNotFound));
    }
}
