#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Plugins.OpenLibrary.Common.Models.DTO.Settings;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Plugins.OpenLibrary.Fixtures.Common.Models.DTO.Settings;

/// <summary>
/// Fixture class for the <see cref="OpenLibrarySettingsDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
internal class OpenLibrarySettingsDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="OpenLibrarySettingsDto"/>.
    /// </summary>
    /// <param name="userAgent">Optional. The user agent sent with every request.</param>
    /// <param name="contactEmail">Optional. The contact email sent with every request.</param>
    /// <param name="searchResultLimit">Optional. The maximum number of search results.</param>
    /// <param name="workEditionLimit">Optional. The maximum number of editions fetched per work.</param>
    /// <param name="minimumRequestInterval">Optional. The minimum interval between consecutive requests.</param>
    /// <param name="includeUserAgent">Whether the user agent should be included, or left at its default value.</param>
    /// <param name="includeSearchResultLimit">Whether the search result limit should be included, or left at its default value.</param>
    /// <param name="includeWorkEditionLimit">Whether the work edition limit should be included, or left at its default value.</param>
    /// <param name="includeMinimumRequestInterval">Whether the minimum request interval should be included, or left at its default value.</param>
    /// <returns>The created <see cref="OpenLibrarySettingsDto"/>.</returns>
    public OpenLibrarySettingsDto Create(
        string? userAgent = null,
        string? contactEmail = null,
        int? searchResultLimit = null,
        int? workEditionLimit = null,
        TimeSpan? minimumRequestInterval = null,
        bool includeUserAgent = true,
        bool includeSearchResultLimit = true,
        bool includeWorkEditionLimit = true,
        bool includeMinimumRequestInterval = true)
    {
        OpenLibrarySettingsDto settings = new()
        {
            ContactEmail = contactEmail
        };
        if (includeUserAgent)
            settings.UserAgent = userAgent ?? _faker.Internet.UserAgent();
        if (includeSearchResultLimit)
            settings.SearchResultLimit = searchResultLimit ?? _faker.Random.Number(1, 50);
        if (includeWorkEditionLimit)
            settings.WorkEditionLimit = workEditionLimit ?? _faker.Random.Number(1, 200);
        if (includeMinimumRequestInterval)
            settings.MinimumRequestInterval = minimumRequestInterval ?? TimeSpan.FromMilliseconds(_faker.Random.Number(100, 2000));
        return settings;
    }
}
