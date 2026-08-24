#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.DTO.Common;

/// <summary>
/// Data transfer object for the artwork of a media item, provided by an artwork provider.
/// </summary>
/// <param name="LocalPath">The local file system path of the artwork, when the artwork is a local file.</param>
/// <param name="RemoteUrl">The remote URL of the artwork, when the artwork is fetched over the web.</param>
[DebuggerDisplay("LocalPath: {LocalPath}, RemoteUrl: {RemoteUrl}")]
public sealed record ArtworkDto(
    string? LocalPath,
    string? RemoteUrl
);
