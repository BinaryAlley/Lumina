namespace Lumina.Application.Common.DTO.MediaLibrary.Management;

/// <summary>
/// Data transfer object for one part of the path template of a media library, carried by the commands that create or update a media library.
/// </summary>
/// <param name="Kind">The kind of the path part.</param>
/// <param name="Representation">The literal text or the value mask of the path part.</param>
/// <param name="IsOptional">Whether the path part can be absent from the path.</param>
public record LibraryPathTemplatePartDto(
    string? Kind,
    string? Representation,
    bool IsOptional
);
