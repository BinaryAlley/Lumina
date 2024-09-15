#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.Common;

/// <summary>
/// Represents a request to get language information.
/// </summary>
[DebuggerDisplay("{LanguageName}")]
public class LanguageInfoModel
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the ISO 639-1 two-letter language code.
    /// </summary>
    public string? LanguageCode { get; set; }

    /// <summary>
    /// Gets the full name of the language in English.
    /// </summary>
    public string? LanguageName { get; set; }

    /// <summary>
    /// Gets an optional native name of the language.
    /// </summary>
    public string? NativeName { get; set; }
    #endregion
}