#region ========================================================================= USING =====================================================================================
using ErrorOr;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Written content error types.
/// </summary>
public static partial class Errors
{
    public static class WrittenContent
    {
        #region ==================================================================== PROPERTIES =================================================================================
        public static Error IsbnCannotBeEmpty => Error.Validation(nameof(IsbnCannotBeEmpty));
        public static Error InvalidIsbn10Format => Error.Validation(nameof(InvalidIsbn10Format));
        public static Error InvalidIsbn13Format => Error.Validation(nameof(InvalidIsbn13Format));
        public static Error UnknownIsbnFormat => Error.Unexpected(nameof(UnknownIsbnFormat));
        #endregion
    }
}