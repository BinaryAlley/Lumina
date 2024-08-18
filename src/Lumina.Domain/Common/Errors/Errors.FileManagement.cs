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
        public static Error PathCannotBeEmpty => Error.Validation(nameof(PathCannotBeEmpty));
        public static Error ExtensionCannotBeEmpty => Error.Validation(nameof(ExtensionCannotBeEmpty));
        public static Error StreamIdCannotBeEmpty => Error.Validation(nameof(StreamIdCannotBeEmpty));
        public static Error CodecCannotBeEmpty => Error.Validation(nameof(CodecCannotBeEmpty));
        public static Error BitrateMustBeAPositiveNumber => Error.Validation(nameof(BitrateMustBeAPositiveNumber));
        #endregion
    }
}