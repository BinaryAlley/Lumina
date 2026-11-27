#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Data transfer object for the response of an update book cover operation.
/// </summary>
[DebuggerDisplay("CoverPath: {CoverPath}")]
public class UpdateBookCoverDto
{
    /// <summary>
    /// Gets or sets the relative path of the stored cover image.
    /// </summary>
    public string? CoverPath { get; set; }
}
