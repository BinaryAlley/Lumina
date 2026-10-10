#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;

/// <summary>
/// Repository entity for one part of the path template of a media library.
/// </summary>
[DebuggerDisplay("Kind: {Kind}; Position: {Position}")]
public class LibraryPathTemplatePartEntity
{
    /// <summary>
    /// Gets or sets the zero based position of the part in the path template.
    /// </summary>
    public required int Position { get; set; }

    /// <summary>
    /// Gets or sets the kind of the path part.
    /// </summary>
    public required LibraryPathPartKind Kind { get; set; }

    /// <summary>
    /// Gets or sets the literal text or the value mask of the path part.
    /// </summary>
    public required string Representation { get; set; }

    /// <summary>
    /// Gets or sets whether the path part can be absent from the path.
    /// </summary>
    public required bool IsOptional { get; set; }
}
