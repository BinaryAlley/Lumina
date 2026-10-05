#region ========================================================================= USING =====================================================================================
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Libraries;

/// <summary>
/// Data transfer object for the path template catalog of a media library type.
/// </summary>
[DebuggerDisplay("Parts: {Parts.Count}; DefaultTemplateParts: {DefaultTemplateParts.Count}")]
public class LibraryPathTemplateCatalogDto
{
    /// <summary>
    /// Gets or sets the path parts that can be used in the path template of the library type.
    /// </summary>
    public List<LibraryPathPartDefinitionDto> Parts { get; set; } = [];

    /// <summary>
    /// Gets or sets the ordered parts of the default path template of the library type.
    /// </summary>
    public List<LibraryPathTemplatePartDto> DefaultTemplateParts { get; set; } = [];
}
