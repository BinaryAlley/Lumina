#region ========================================================================= USING =====================================================================================
using ErrorOr;
#endregion

namespace Lumina.Application.Common.Errors;

/// <summary>
/// Application authorization related error types.
/// </summary>
public static partial class Errors
{
    public static class Authorization
    {
        public static Error NotAuthorized => Error.Unauthorized(description: nameof(NotAuthorized));
        public static Error AdminAccountAlreadyCreated => Error.Unauthorized(description: nameof(AdminAccountAlreadyCreated));
    }
}
