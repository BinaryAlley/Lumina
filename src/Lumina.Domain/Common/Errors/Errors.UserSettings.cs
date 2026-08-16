#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// User settings error types.
/// </summary>
public static partial class Errors
{
    public static class UserSettings
    {
        public static Error UserSettingsAlreadyExists => Error.Conflict(description: nameof(UserSettingsAlreadyExists));
        public static Error UserSettingsNotFound => Error.NotFound(description: nameof(UserSettingsNotFound));
        public static Error ItemsPerPageMustBeGreaterThanZero => Error.Validation(description: nameof(ItemsPerPageMustBeGreaterThanZero));
    }
}
