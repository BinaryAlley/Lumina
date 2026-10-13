#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.Common;

/// <summary>
/// Data transfer object for a stored piece of artwork of a media item.
/// </summary>
/// <param name="Type">The type of the artwork, like the cover or the back of the media item.</param>
/// <param name="Ordinal">The ordinal of the artwork within its type, so that multiple artworks of the same type can be ordered.</param>
/// <param name="Path">The relative path of the stored artwork file.</param>
[DebuggerDisplay("Type: {Type}, Ordinal: {Ordinal}, Path: {Path}")]
public sealed record MediaArtworkDto(
    ArtworkType Type,
    int Ordinal,
    string Path
);
