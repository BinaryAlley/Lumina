namespace Lumina.Domain.Common.Primitives;

/// <summary>
/// Provides predefined success results for operations that don't return meaningful data, and factory methods for results.
/// </summary>
public static class Result
{
    /// <summary>
    /// Represents a successful operation.
    /// </summary>
    public static Success Success { get; } = new();

    /// <summary>
    /// Represents a successful creation operation.
    /// </summary>
    public static Created Created { get; } = new();

    /// <summary>
    /// Represents a successful update operation.
    /// </summary>
    public static Updated Updated { get; } = new();

    /// <summary>
    /// Represents a successful deletion operation.
    /// </summary>
    public static Deleted Deleted { get; } = new();

    /// <summary>
    /// Creates a successful result wrapping the provided value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to wrap.</typeparam>
    /// <param name="value">The value to wrap in a successful result.</param>
    /// <returns>A successful result containing the provided value.</returns>
    public static Result<TValue> From<TValue>(TValue value)
    {
        return Result<TValue>.Success(value);
    }
}
