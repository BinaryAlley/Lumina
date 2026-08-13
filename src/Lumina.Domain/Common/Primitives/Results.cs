namespace Lumina.Domain.Common.Primitives;

/// <summary>
/// Provides predefined success results for operations that don't return meaningful data.
/// </summary>
public static class Results
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
}
