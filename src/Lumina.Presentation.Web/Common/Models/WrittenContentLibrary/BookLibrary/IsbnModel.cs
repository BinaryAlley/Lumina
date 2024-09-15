#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.BookLibrary;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Represents a request to get ISBN information.
/// </summary>
[DebuggerDisplay("{Value} ({Format})")]
public class IsbnModel
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets or sets the ISBN value.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the format of the ISBN (e.g., ISBN-10 or ISBN-13).
    /// </summary>
    public IsbnFormat? Format { get; set; }
    #endregion
}