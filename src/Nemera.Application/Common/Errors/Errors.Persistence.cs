#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Application.Common.Errors;

/// <summary>
/// Application persistence related error types.
/// </summary>
public static partial class Errors
{
    /// <summary>
    /// Error types for application persistence operations.
    /// </summary>
    public static class Persistence
    {
        /// <summary>
        /// Gets the error that reports that a media library could not be persisted.
        /// </summary>
        public static Error ErrorPersistingMediaLibrary => Error.Failure(description: nameof(ErrorPersistingMediaLibrary));

        /// <summary>
        /// Gets the error that reports that an authorization role could not be persisted.
        /// </summary>
        public static Error ErrorPersistingAuthorizationRole => Error.Failure(description: nameof(ErrorPersistingAuthorizationRole));

        /// <summary>
        /// Gets the error that reports that a unique constraint was violated.
        /// </summary>
        public static Error UniqueConstraintViolation => Error.Conflict(description: nameof(UniqueConstraintViolation));

        /// <summary>
        /// Gets the error that reports that a concurrency conflict occurred while persisting changes.
        /// </summary>
        public static Error ConcurrencyConflict => Error.Conflict(description: nameof(ConcurrencyConflict));
    }
}
