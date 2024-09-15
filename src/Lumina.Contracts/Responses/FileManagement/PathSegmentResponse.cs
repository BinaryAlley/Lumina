namespace Lumina.Contracts.Responses.FileManagement;

/// <summary>
/// Represents a response containing a file system path.
/// </summary>
/// <param name="Path">The returned path.</param>
public record PathSegmentResponse(
    string Path
);