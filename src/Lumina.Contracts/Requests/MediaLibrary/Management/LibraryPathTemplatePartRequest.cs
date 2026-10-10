#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.Management;

/// <summary>
/// Represents one part of the path template of a media library.
/// </summary>
/// <param name="Kind">The kind of the path part. Required.</param>
/// <param name="Representation">The literal text or the value mask of the path part. Optional for separators, required otherwise.</param>
/// <param name="IsOptional">Whether the path part can be absent from the path. Optional.</param>
[DebuggerDisplay("Kind: {Kind}; Representation: {Representation}")]
public record LibraryPathTemplatePartRequest(
    string? Kind,
    string? Representation,
    bool IsOptional
);
