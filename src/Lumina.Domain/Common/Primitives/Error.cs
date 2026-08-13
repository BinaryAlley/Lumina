namespace Lumina.Domain.Common.Primitives;

/// <summary>
/// Represents an error that can be implicitly converted to a Results.
/// </summary>
public abstract record Error
{
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
    /// Creates a new NotFoundError with the specified description.
    /// </summary>
    /// <param name="description">The human-readable description of the error.</param>
    /// <returns>A new NotFoundError instance with code "NotFound".</returns>
    public static NotFoundError NotFound(string description)
    {
        return new("NotFound", description);
    }

    /// <summary>
    /// Creates a new InvalidOperationError with the specified description.
    /// </summary>
    /// <param name="description">The human-readable description of the error.</param>
    /// <returns>A new InvalidOperationError instance with code "InvalidOperation".</returns>
    public static InvalidOperationError InvalidOperation(string description)
    {
        return new("InvalidOperation", description);
    }

    /// <summary>
    /// Creates a new ForbiddenError with the specified description.
    /// </summary>
    /// <param name="description">The human-readable description of the error.</param>
    /// <returns>A new ForbiddenError instance with code "Forbidden".</returns>
    public static ForbiddenError Forbidden(string description)
    {
        return new("Forbidden", description);
    }

    /// <summary>
    /// Creates a new FailureError with the specified description.
    /// </summary>
    /// <param name="description">The human-readable description of the error.</param>
    /// <returns>A new FailureError instance with code "Failure".</returns>
    public static FailureError Failure(string description)
    {
        return new("Failure", description);
    }

    /// <summary>
    /// Creates a new ValidationError with the specified description.
    /// </summary>
    /// <param name="description">The human-readable description of the error.</param>
    /// <returns>A new ValidationError instance with code "Validation".</returns>
    public static ValidationError Validation(string description)
    {
        return new("Validation", description);
    }

    /// <summary>
    /// Creates a new ConflictError with the specified description.
    /// </summary>
    /// <param name="description">The human-readable description of the error.</param>
    /// <returns>A new ConflictError instance with code "Conflict".</returns>
    public static ConflictError Conflict(string description)
    {
        return new("Conflict", description);
    }

    /// <summary>
    /// Creates a new TimeoutError with the specified description.
    /// </summary>
    /// <param name="description">The human-readable description of the error.</param>
    /// <returns>A new TimeoutError instance with code "Timeout".</returns>
    public static TimeoutError Timeout(string description)
    {
        return new("Timeout", description);
    }

    /// <summary>
    /// Creates a new UnauthorizedError with the specified description.
    /// </summary>
    /// <param name="description">The human-readable description of the error.</param>
    /// <returns>A new UnauthorizedError instance with code "Unauthorized".</returns>
    public static UnauthorizedError Unauthorized(string description)
    {
        return new("Unauthorized", description);
    }

    /// <summary>
    /// Creates a new InternalError with the specified description.
    /// </summary>
    /// <param name="description">The human-readable description of the error.</param>
    /// <returns>A new InternalError instance with code "Internal".</returns>
    public static InternalError Internal(string description)
    {
        return new("Internal", description);
    }

    /// <summary>
    /// Creates a new ResourceUnavailableError with the specified description.
    /// </summary>
    /// <param name="description">The human-readable description of the error.</param>
    /// <returns>A new ResourceUnavailableError instance with code "ResourceUnavailable".</returns>
    public static ResourceUnavailableError ResourceUnavailable(string description)
    {
        return new("ResourceUnavailable", description);
    }

    /// <summary>
    /// Creates a new NotImplementedError with the specified description.
    /// </summary>
    /// <param name="description">The human-readable description of the error.</param>
    /// <returns>A new NotImplementedError instance with code "NotImplemented".</returns>
    public static NotImplementedError NotImplemented(string description)
    {
        return new("NotImplemented", description);
    }
}
