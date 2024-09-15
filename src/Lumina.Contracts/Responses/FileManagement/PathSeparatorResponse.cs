namespace Lumina.Contracts.Responses.FileManagement;

/// <summary>
/// Represents a response to the inquiry about the system's path separator.
/// </summary>
/// <param name="Separator">The system's path separator.</param>
public record PathSeparatorResponse(
    string Separator
);