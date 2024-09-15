namespace Lumina.Contracts.Responses.FileManagement;

/// <summary>
/// Represents a response containing the result of splitting a file system path.
/// </summary>
/// <param name="PathSegments">The returned path segments.</param>
public record SplitPathResponse(
    string[] PathSegments
);