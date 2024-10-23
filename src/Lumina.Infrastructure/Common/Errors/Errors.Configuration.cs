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
        public static Error ApplicationThemeCannotBeEmpty => Error.Validation(description: nameof(ApplicationThemeCannotBeEmpty));
        public static Error DatabaseConnectionStringCannotBeEmpty => Error.Validation(description: nameof(DatabaseConnectionStringCannotBeEmpty));
    }
}
