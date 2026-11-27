#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Primitives;
#endregion

namespace Nemera.Domain.Common.Errors;

/// <summary>
/// User settings error types.
/// </summary>
public static partial class Errors
{
    /// <summary>
    /// Defines the errors that report failures raised while working with user settings.
    /// </summary>
    public static class UserSettings
    {
        /// <summary>
        /// Gets the error that reports that settings already exist for the user.
        /// </summary>
        public static Error UserSettingsAlreadyExists => Error.Conflict(description: nameof(UserSettingsAlreadyExists));

        /// <summary>
        /// Gets the error that reports that no settings exist for the requested user.
        /// </summary>
        public static Error UserSettingsNotFound => Error.NotFound(description: nameof(UserSettingsNotFound));
    }
}
