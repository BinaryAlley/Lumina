#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Libraries;

/// <summary>
/// Data transfer object for a path part available for a media library type.
/// </summary>
[DebuggerDisplay("Kind: {Kind}")]
public class LibraryPathPartDefinitionDto
{
    /// <summary>
    /// Gets or sets the kind of the path part.
    /// </summary>
    public string Kind { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value type captured by the path part.
    /// </summary>
    public string ValueType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default representation of the path part.
    /// </summary>
    public string DefaultRepresentation { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the path part is optional by default.
    /// </summary>
    public bool IsOptionalByDefault { get; set; }
}
