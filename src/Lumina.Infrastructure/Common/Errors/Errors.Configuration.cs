#region ========================================================================= USING =====================================================================================
using ErrorOr;
#endregion

namespace Lumina.Infrastructure.Common.Errors;

/// <summary>
/// Application configuration error types.
/// </summary>
public static partial class Errors
{
    public static class Configuration
    {
        #region ==================================================================== PROPERTIES =================================================================================
        public static Error ApplicationThemeCannotBeEmpty => Error.Validation(nameof(ApplicationThemeCannotBeEmpty));
        public static Error DatabaseConnectionStringCannotBeEmpty => Error.Validation(nameof(DatabaseConnectionStringCannotBeEmpty));
        #endregion
    }
}