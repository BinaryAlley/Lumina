#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Domain permission related error types.
/// </summary>
public static partial class Errors
{
    public static class Permission
    {
        public static Error UnauthorizedAccess => Error.Failure(description: nameof(UnauthorizedAccess));
    }
}
