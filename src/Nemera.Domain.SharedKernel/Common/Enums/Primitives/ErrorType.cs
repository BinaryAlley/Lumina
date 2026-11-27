namespace Lumina.Domain.SharedKernel.Common.Enums.Primitives;

/// <summary>
/// Enumeration for the type of an error.
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// Represents a generic failure.
    /// </summary>
    Failure,

    /// <summary>
    /// Represents an unexpected error.
    /// </summary>
    Unexpected,

    /// <summary>
    /// Represents a validation error.
    /// </summary>
    Validation,

    /// <summary>
    /// Represents a conflict error.
    /// </summary>
    Conflict,

    /// <summary>
    /// Represents a not found error.
    /// </summary>
    NotFound,

    /// <summary>
    /// Represents an unauthorized error.
    /// </summary>
    Unauthorized,

    /// <summary>
    /// Represents a forbidden error.
    /// </summary>
    Forbidden,
}
