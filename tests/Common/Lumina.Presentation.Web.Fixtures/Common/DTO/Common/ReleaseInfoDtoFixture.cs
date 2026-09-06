#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Common;

/// <summary>
/// Fixture class for generating <see cref="ReleaseInfoDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReleaseInfoDtoFixture
{
    private readonly Faker _faker = new();
    private readonly Random _random = new();

    /// <summary>
    /// Creates a new <see cref="ReleaseInfoDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="originalReleaseDate">Optional. The original release date of the content.</param>
    /// <param name="originalReleaseYear">Optional. The original release year of the content.</param>
    /// <param name="reReleaseDate">Optional. The re-release date of the content.</param>
    /// <param name="reReleaseYear">Optional. The re-release year of the content.</param>
    /// <param name="releaseCountry">Optional. The country where the content was released.</param>
    /// <param name="releaseVersion">Optional. The version or edition of the content's release.</param>
    /// <returns>A configured <see cref="ReleaseInfoDto"/> instance.</returns>
    public ReleaseInfoDto Create(
        DateOnly? originalReleaseDate = null,
        int? originalReleaseYear = null,
        DateOnly? reReleaseDate = null,
        int? reReleaseYear = null,
        string? releaseCountry = null,
        string? releaseVersion = null)
    {
        int resolvedOriginalReleaseYear = originalReleaseYear ?? (originalReleaseDate?.Year ?? _random.Next(1900, 2026));
        DateOnly resolvedOriginalReleaseDate = originalReleaseDate ?? new DateOnly(resolvedOriginalReleaseYear, 1, 1);
        int resolvedReReleaseYear = reReleaseYear ?? (reReleaseDate?.Year ?? _random.Next(resolvedOriginalReleaseYear, resolvedOriginalReleaseYear + 100));
        DateOnly resolvedReReleaseDate = reReleaseDate ?? new DateOnly(resolvedReReleaseYear, 1, 1);

        return new ReleaseInfoDto
        {
            OriginalReleaseDate = resolvedOriginalReleaseDate,
            OriginalReleaseYear = resolvedOriginalReleaseYear,
            ReReleaseDate = resolvedReReleaseDate,
            ReReleaseYear = resolvedReReleaseYear,
            ReleaseCountry = releaseCountry ?? _faker.Address.CountryCode(),
            ReleaseVersion = releaseVersion ?? _faker.Random.String2(_faker.Random.Number(1, 50))
        };
    }

    /// <summary>
    /// Creates multiple <see cref="ReleaseInfoDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ReleaseInfoDto"/> instances.</returns>
    public List<ReleaseInfoDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
