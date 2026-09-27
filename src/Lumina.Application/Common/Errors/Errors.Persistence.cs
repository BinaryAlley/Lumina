#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
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
        public static Error ErrorPersistingAuthorizationRole => Error.Failure(description: nameof(ErrorPersistingAuthorizationRole));
        public static Error UniqueConstraintViolation => Error.Conflict(description: nameof(UniqueConstraintViolation));
    }
}
