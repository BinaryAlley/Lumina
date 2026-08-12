#region ========================================================================= USING =====================================================================================
using Lumina.Plugins.OpenLibrary.Common.Models.DTO.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
#endregion

namespace Lumina.Plugins.OpenLibrary.Core.Settings;

/// <summary>
/// Applies the settings persisted by the host onto the runtime settings of the Open Library metadata plugin.
/// </summary>
internal static class OpenLibrarySettingsLoader
{
    /// <summary>
    /// Overlays the persisted <paramref name="storedSettings"/> onto the provided <paramref name="settings"/>, keeping the default values of the settings that were not persisted.
    /// </summary>
    /// <param name="settings">The runtime settings onto which the persisted settings are applied.</param>
    /// <param name="storedSettings">The settings persisted by the host, keyed by setting key.</param>
    public static void Apply(OpenLibrarySettingsDto settings, IReadOnlyDictionary<string, string> storedSettings)
    {
        if (storedSettings.TryGetValue(OpenLibrarySettingsKeys.CONTACT_EMAIL, out string? contactEmail))
            settings.ContactEmail = string.IsNullOrWhiteSpace(contactEmail) ? null : contactEmail.Trim();

        if (storedSettings.TryGetValue(OpenLibrarySettingsKeys.SEARCH_RESULT_LIMIT, out string? searchResultLimit) &&
            int.TryParse(searchResultLimit, NumberStyles.Integer, CultureInfo.InvariantCulture, out int searchResultLimitValue))
            settings.SearchResultLimit = Math.Max(1, searchResultLimitValue);

        if (storedSettings.TryGetValue(OpenLibrarySettingsKeys.WORK_EDITION_LIMIT, out string? workEditionLimit) &&
            int.TryParse(workEditionLimit, NumberStyles.Integer, CultureInfo.InvariantCulture, out int workEditionLimitValue))
            settings.WorkEditionLimit = Math.Max(1, workEditionLimitValue);

        if (storedSettings.TryGetValue(OpenLibrarySettingsKeys.MINIMUM_REQUEST_INTERVAL_SECONDS, out string? minimumRequestInterval) &&
            double.TryParse(minimumRequestInterval, NumberStyles.Float, CultureInfo.InvariantCulture, out double minimumRequestIntervalValue))
            settings.MinimumRequestInterval = TimeSpan.FromSeconds(Math.Max(0, minimumRequestIntervalValue));
    }
}
