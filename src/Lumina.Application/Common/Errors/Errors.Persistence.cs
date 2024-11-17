#region ========================================================================= USING =====================================================================================
using ErrorOr;
#endregion

namespace Lumina.Application.Common.Errors;

/// <summary>
/// Application persistence related error types.
/// </summary>
public static partial class Errors
{
    public static class Persistence
    {
        public static Error ErrorPersistingMediaLibrary => Error.Failure(description: nameof(ErrorPersistingMediaLibrary));
    }
}
