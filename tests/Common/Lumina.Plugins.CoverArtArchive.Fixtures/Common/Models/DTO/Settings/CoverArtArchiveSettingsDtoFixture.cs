#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Plugins.CoverArtArchive.Common.Models.DTO.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Plugins.CoverArtArchive.Fixtures.Common.Models.DTO.Settings;

/// <summary>
/// Fixture class for the <see cref="CoverArtArchiveSettingsDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class CoverArtArchiveSettingsDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="CoverArtArchiveSettingsDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="userAgent">Optional. The user agent sent with every request.</param>
    /// <param name="contactEmail">Optional. The contact email sent to the Cover Art Archive API.</param>
    /// <param name="includeContactEmail">Whether the contact email should be included, or forced to <see langword="null"/>.</param>
    /// <param name="minimumRequestInterval">Optional. The minimum interval between consecutive requests.</param>
    /// <returns>A configured <see cref="CoverArtArchiveSettingsDto"/> instance.</returns>
    public CoverArtArchiveSettingsDto Create(
        string? userAgent = null,
        string? contactEmail = null,
        bool includeContactEmail = true,
        TimeSpan? minimumRequestInterval = null)
    {
        return new CoverArtArchiveSettingsDto
        {
            UserAgent = userAgent ?? $"Lumina-CoverArtArchive/{_faker.Random.Int(1, 9)}.{_faker.Random.Int(0, 9)}",
            ContactEmail = includeContactEmail ? contactEmail ?? _faker.Internet.Email() : null,
            MinimumRequestInterval = minimumRequestInterval ?? TimeSpan.FromSeconds(_faker.Random.Double(0, 5))
        };
    }

    /// <summary>
    /// Creates multiple <see cref="CoverArtArchiveSettingsDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="CoverArtArchiveSettingsDto"/> instances.</returns>
    public List<CoverArtArchiveSettingsDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
