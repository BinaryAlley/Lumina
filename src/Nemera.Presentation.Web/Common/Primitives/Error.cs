#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Primitives;
#endregion

namespace Lumina.Presentation.Web.Common.Primitives;

/// <summary>
/// Represents an error that can be implicitly converted to a Result.
/// </summary>
public abstract record Error
{
    /// <summary>
    /// Gets the type of this error.
    /// </summary>
    public abstract ErrorType Type { get; }

    /// <summary>
    /// Gets the error code that uniquely identifies this error type.
    /// </summary>
    public abstract string Code { get; }

    /// <summary>
    /// Gets the human-readable description of the error.
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// Returns a string representation of the error.
    /// </summary>
    /// <returns>A formatted string containing the error code and description.</returns>
    public override string ToString()
    {
        return $"{Code}: {Description}";
    }

    /// <summary>
    /// Creates a new NotFoundError.
    /// </summary>
    /// <param name="code">The error code, defaults to "General.NotFound" when null.</param>
    /// <param name="description">The human-readable description of the error.</param>
    /// <returns>A new NotFoundError instance.</returns>
    public static NotFoundError NotFound(string? code = null, string? description = null)
    {
        return new(code ?? "General.NotFound", description ?? "A 'Not Found' error has occurred.");
    }

    /// <summary>
    /// Creates a new InvalidOperationError.
    /// </summary>
    /// <param name="code">The error code, defaults to "General.InvalidOperation" when null.</param>
    /// <param name="description">The human-readable description of the error.</param>
    /// <returns>A new InvalidOperationError instance.</returns>
    public static InvalidOperationError InvalidOperation(string? code = null, string? description = null)
    {
        return new(code ?? "General.InvalidOperation", description ?? "An invalid operation error has occurred.");
    }

    /// <summary>
    /// Creates a new ForbiddenError.
    /// </summary>
    /// <param name="code">The error code, defaults to "General.Forbidden" when null.</param>
    /// <param name="description">The human-readable description of the error.</param>
    /// <returns>A new ForbiddenError instance.</returns>
    public static ForbiddenError Forbidden(string? code = null, string? description = null)
    {
        return new(code ?? "General.Forbidden", description ?? "A 'Forbidden' error has occurred.");
    }

    /// <summary>
    /// Creates a new FailureError.
    /// </summary>
    /// <param name="code">The error code, defaults to "General.Failure" when null.</param>
    /// <param name="description">The human-readable description of the error.</param>
    /// <returns>A new FailureError instance.</returns>
    public static FailureError Failure(string? code = null, string? description = null)
    {
        return new(code ?? "General.Failure", description ?? "A failure has occurred.");
    }

    /// <summary>
    /// Creates a new ValidationError.
    /// </summary>
    /// <param name="code">The error code, defaults to "General.Validation" when null.</param>
    /// <param name="description">The human-readable description of the error.</param>
    /// <returns>A new ValidationError instance.</returns>
    public static ValidationError Validation(string? code = null, string? description = null)
    {
        return new(code ?? "General.Validation", description ?? "A validation error has occurred.");
    }

    /// <summary>
    /// Creates a new ConflictError.
    /// </summary>
    /// <param name="code">The error code, defaults to "General.Conflict" when null.</param>
    /// <param name="description">The human-readable description of the error.</param>
    /// <returns>A new ConflictError instance.</returns>
    public static ConflictError Conflict(string? code = null, string? description = null)
    {
        return new(code ?? "General.Conflict", description ?? "A conflict error has occurred.");
    }

    /// <summary>
    /// Creates a new TimeoutError.
    /// </summary>
    /// <param name="code">The error code, defaults to "General.Timeout" when null.</param>
    /// <param name="description">The human-readable description of the error.</param>
    /// <returns>A new TimeoutError instance.</returns>
    public static TimeoutError Timeout(string? code = null, string? description = null)
    {
        return new(code ?? "General.Timeout", description ?? "The operation timed out.");
    }

    /// <summary>
    /// Creates a new UnauthorizedError.
    /// </summary>
    /// <param name="code">The error code, defaults to "General.Unauthorized" when null.</param>
    /// <param name="description">The human-readable description of the error.</param>
    /// <returns>A new UnauthorizedError instance.</returns>
    public static UnauthorizedError Unauthorized(string? code = null, string? description = null)
    {
        return new(code ?? "General.Unauthorized", description ?? "An 'Unauthorized' error has occurred.");
    }

    /// <summary>
    /// Creates a new UnexpectedError.
    /// </summary>
    /// <param name="code">The error code, defaults to "General.Unexpected" when null.</param>
    /// <param name="description">The human-readable description of the error.</param>
    /// <returns>A new UnexpectedError instance.</returns>
    public static UnexpectedError Unexpected(string? code = null, string? description = null)
    {
        return new(code ?? "General.Unexpected", description ?? "An unexpected error has occurred.");
    }

    /// <summary>
    /// Creates a new InternalError.
    /// </summary>
    /// <param name="code">The error code, defaults to "General.Internal" when null.</param>
    /// <param name="description">The human-readable description of the error.</param>
    /// <returns>A new InternalError instance.</returns>
    public static InternalError Internal(string? code = null, string? description = null)
    {
        return new(code ?? "General.Internal", description ?? "An internal error has occurred.");
    }

    /// <summary>
    /// Creates a new ResourceUnavailableError.
    /// </summary>
    /// <param name="code">The error code, defaults to "General.ResourceUnavailable" when null.</param>
    /// <param name="description">The human-readable description of the error.</param>
    /// <returns>A new ResourceUnavailableError instance.</returns>
    public static ResourceUnavailableError ResourceUnavailable(string? code = null, string? description = null)
    {
        return new(code ?? "General.ResourceUnavailable", description ?? "The required resource is currently unavailable.");
    }

    /// <summary>
    /// Creates a new NotImplementedError.
    /// </summary>
    /// <param name="code">The error code, defaults to "General.NotImplemented" when null.</param>
    /// <param name="description">The human-readable description of the error.</param>
    /// <returns>A new NotImplementedError instance.</returns>
    public static NotImplementedError NotImplemented(string? code = null, string? description = null)
    {
        return new(code ?? "General.NotImplemented", description ?? "The requested functionality is not implemented.");
    }
}
