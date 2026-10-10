#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.Common;

/// <summary>
/// Data transfer object for the artwork of a media item, provided by an artwork provider.
/// </summary>
/// <param name="Type">The type of the artwork, like the cover or the back of the media item.</param>
/// <param name="Ordinal">The ordinal of the artwork within its type, so that multiple artworks of the same type can be ordered.</param>
/// <param name="LocalPath">The local file system path of the artwork, when the artwork is a local file.</param>
/// <param name="RemoteUrl">The remote URL of the artwork, when the artwork is fetched over the web.</param>
/// <param name="IsTemporary">Whether a local artwork is a temporary file produced by the provider, which is deleted after the artwork is stored.</param>
[DebuggerDisplay("Type: {Type}, Ordinal: {Ordinal}, LocalPath: {LocalPath}, RemoteUrl: {RemoteUrl}")]
public sealed record ArtworkDto(
    ArtworkType Type,
    int Ordinal,
    string? LocalPath,
    string? RemoteUrl,
    bool IsTemporary = false
);
