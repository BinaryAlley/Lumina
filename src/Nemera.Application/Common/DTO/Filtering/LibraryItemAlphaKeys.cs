#region ========================================================================= USING =====================================================================================
#endregion

namespace Lumina.Application.Common.DTO.Filtering;

/// <summary>
/// Collection of the special alpha keys that can be used for alpha filtering, in addition to the single letters.
/// </summary>
public static class LibraryItemAlphaKeys
{
    /// <summary>
    /// The alpha key that filters the items by titles whose first character is a digit.
    /// </summary>
    public const string NUMBER = "#";

    /// <summary>
    /// The alpha key that filters the items by titles whose first character is neither a letter nor a digit.
    /// </summary>
    public const string SYMBOL = "*";
}
