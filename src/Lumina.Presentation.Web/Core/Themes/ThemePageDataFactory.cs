#region ========================================================================= USING =====================================================================================
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
#endregion

namespace Lumina.Presentation.Web.Core.Themes;

/// <summary>
/// Builds the page data entries that the themed endpoints expose to their templates.
/// </summary>
public static class ThemePageDataFactory
{
    /// <summary>
    /// Creates the localized strings of a page from the provided localizer, keyed by their resource names.
    /// </summary>
    /// <param name="localizer">The localizer of the page resources.</param>
    /// <returns>The localized strings keyed by their resource names.</returns>
    public static Dictionary<string, object?> CreateLocalizedStrings(IStringLocalizer localizer)
    {
        // expose the page's full resource set, so templates can pull any localized string without the page enumerating keys
        Dictionary<string, object?> strings = [];
        foreach (LocalizedString localizedString in localizer.GetAllStrings(includeParentCultures: true))
            strings[localizedString.Name] = localizedString.Value;

        return strings;
    }
}
