#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums;
#endregion

namespace Lumina.Presentation.Web.Common.Models.Books;

/// <summary>
/// Represents a request to get ISBN information.
/// </summary>
public class IsbnDto
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the value of the ISBN.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Gets the format of the ISBN.
    /// </summary>
    public IsbnFormat? Format { get; set; }
    #endregion
}