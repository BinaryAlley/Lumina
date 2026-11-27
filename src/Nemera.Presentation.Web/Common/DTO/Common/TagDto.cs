#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Nemera.Presentation.Web.Common.DTO.Common;

/// <summary>
/// Data transfer object for a request to get tag information.
/// </summary>
[DebuggerDisplay("Name: {Name}")]
public class TagDto
{
    /// <summary>
    /// Gets the name of the tag element of the media item.
    /// </summary>
    public string? Name { get; set; }
}
