#region ========================================================================= USING =====================================================================================
using System.Text.RegularExpressions;
#endregion

namespace Lumina.Presentation.Web.Common.Utilities;

/// <summary>
/// Extension methods for strings.
/// </summary>
public static class StringUtilities
{
    #region ================================================================== METHODS ===================================================================================
    /// <summary>
    /// Splits a camel case string into separate words.
    /// </summary>
    /// <param name="input">The string to be split.</param>
    /// <returns>The split string.</returns>
    public static string SplitCamelCase(this string input)
    {
        return Regex.Replace(input, "([A-Z])", " $1", RegexOptions.Compiled).Trim();
    }
    #endregion
}