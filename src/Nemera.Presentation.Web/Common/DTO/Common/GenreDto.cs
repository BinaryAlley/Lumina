#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.Common;

/// <summary>
/// Data transfer object for a request to get genre information.
/// </summary>
[DebuggerDisplay("{Name}")]
public class GenreDto
{
    /// <summary>
    /// Gets the name of the genre element of the media item.
    /// </summary>
    public string? Name { get; set; }
}
