#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Libraries;

/// <summary>
/// Data transfer object for one part of the path template of a media library.
/// </summary>
[DebuggerDisplay("Kind: {Kind}; Representation: {Representation}")]
public class LibraryPathTemplatePartDto
{
    /// <summary>
    /// Gets or sets the kind of the path part.
    /// </summary>
    public string Kind { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the literal text or the value mask of the path part.
    /// </summary>
    public string Representation { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the path part can be absent from the path.
    /// </summary>
    public bool IsOptional { get; set; }
}
