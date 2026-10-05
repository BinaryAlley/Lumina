#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.MediaLibrary.Management;

/// <summary>
/// Represents a path template part response.
/// </summary>
/// <param name="Kind">The kind of the path part.</param>
/// <param name="Representation">The literal text or the value mask of the path part.</param>
/// <param name="IsOptional">Whether the path part can be absent from the path.</param>
[DebuggerDisplay("Kind: {Kind}; Representation: {Representation}")]
public record LibraryPathTemplatePartResponse(
    string Kind,
    string Representation,
    bool IsOptional
);
