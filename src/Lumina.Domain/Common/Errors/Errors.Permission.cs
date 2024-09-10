#region ========================================================================= USING =====================================================================================
using ErrorOr;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Domain permission related error types.
/// </summary>
public static partial class Errors
{
    public static class Permission
    {
        #region ==================================================================== PROPERTIES =================================================================================
        public static Error UnauthorizedAccess => Error.Failure(nameof(UnauthorizedAccess));
        #endregion
    }
}