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
    /// <returns>The created <see cref="OpenLibrarySettingsDto"/>.</returns>
    public OpenLibrarySettingsDto Create(
        string? userAgent = null,
        string? contactEmail = null,
        int? searchResultLimit = null,
        int? workEditionLimit = null,
        TimeSpan? minimumRequestInterval = null)
    {
        return new OpenLibrarySettingsDto
        {
            UserAgent = userAgent ?? _faker.Internet.UserAgent(),
            ContactEmail = contactEmail,
            SearchResultLimit = searchResultLimit ?? _faker.Random.Number(1, 50),
            WorkEditionLimit = workEditionLimit ?? _faker.Random.Number(1, 200),
            MinimumRequestInterval = minimumRequestInterval ?? TimeSpan.FromMilliseconds(_faker.Random.Number(100, 2000))
        };
    }
}
