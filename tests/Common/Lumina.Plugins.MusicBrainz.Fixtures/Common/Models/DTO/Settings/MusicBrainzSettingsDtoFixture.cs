#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Plugins.MusicBrainz.Common.Models.DTO.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Plugins.MusicBrainz.Fixtures.Common.Models.DTO.Settings;

/// <summary>
/// Fixture class for the <see cref="MusicBrainzSettingsDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class MusicBrainzSettingsDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="MusicBrainzSettingsDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="baseUrl">Optional. The base URL of the MusicBrainz web service.</param>
    /// <param name="doesAllowPrivateBaseUrl">Optional. Whether the base URL may point at a private or local host.</param>
    /// <param name="userAgent">Optional. The user agent sent with every request.</param>
    /// <param name="contactEmail">Optional. The contact email sent to the MusicBrainz API.</param>
    /// <param name="includeContactEmail">Whether the contact email should be included, or forced to <see langword="null"/>.</param>
    /// <param name="searchResultLimit">Optional. The maximum number of results returned by a single search.</param>
    /// <param name="releaseLookupLimit">Optional. The maximum number of releases fetched for a single release group.</param>
    /// <param name="minimumRequestInterval">Optional. The minimum interval between consecutive requests.</param>
    /// <returns>A configured <see cref="MusicBrainzSettingsDto"/> instance.</returns>
    public MusicBrainzSettingsDto Create(
        string? baseUrl = null,
        bool? doesAllowPrivateBaseUrl = null,
        string? userAgent = null,
        string? contactEmail = null,
        bool includeContactEmail = true,
        int? searchResultLimit = null,
        int? releaseLookupLimit = null,
        TimeSpan? minimumRequestInterval = null)
    {
        return new MusicBrainzSettingsDto
        {
            BaseUrl = baseUrl ?? _faker.Internet.Url(),
            DoesAllowPrivateBaseUrl = doesAllowPrivateBaseUrl ?? _faker.Random.Bool(),
            UserAgent = userAgent ?? $"Lumina-MusicBrainz/{_faker.Random.Int(1, 9)}.{_faker.Random.Int(0, 9)}",
            ContactEmail = includeContactEmail ? contactEmail ?? _faker.Internet.Email() : null,
            SearchResultLimit = searchResultLimit ?? _faker.Random.Int(1, 100),
            ReleaseLookupLimit = releaseLookupLimit ?? _faker.Random.Int(1, 100),
            MinimumRequestInterval = minimumRequestInterval ?? TimeSpan.FromSeconds(_faker.Random.Double(0, 5))
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicBrainzSettingsDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicBrainzSettingsDto"/> instances.</returns>
    public List<MusicBrainzSettingsDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
