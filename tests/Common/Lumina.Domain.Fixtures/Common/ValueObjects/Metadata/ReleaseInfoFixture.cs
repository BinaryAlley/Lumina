#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Common.ValueObjects.Metadata;

/// <summary>
/// Fixture class for the <see cref="ReleaseInfo"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReleaseInfoFixture
{
    private readonly Faker _faker = new();
    private readonly Random _random = new();

    /// <summary>
    /// Creates a random valid <see cref="ReleaseInfo"/>.
    /// </summary>
    /// <param name="originalReleaseDate">Optional. The original release date of the media item.</param>
    /// <param name="originalReleaseYear">Optional. The original release year of the media item.</param>
    /// <param name="reReleaseDate">Optional. The re-release date of the media item.</param>
    /// <param name="reReleaseYear">Optional. The re-release year of the media item.</param>
    /// <param name="releaseCountry">Optional. The country or region of release.</param>
    /// <param name="releaseVersion">Optional. The release version or edition.</param>
    /// <returns>The created <see cref="ReleaseInfo"/>.</returns>
    public ReleaseInfo Create(
        Optional<DateOnly>? originalReleaseDate = null,
        Optional<int>? originalReleaseYear = null,
        Optional<DateOnly>? reReleaseDate = null,
        Optional<int>? reReleaseYear = null,
        Optional<string>? releaseCountry = null,
        Optional<string>? releaseVersion = null)
    {
        int generatedYear = _random.Next(1900, 2000);

        Optional<DateOnly> resolvedOriginalDate = originalReleaseDate ?? Optional<DateOnly>.None();
        Optional<int> resolvedOriginalYear = originalReleaseYear ?? Optional<int>.None();
        if (!resolvedOriginalDate.HasValue && !resolvedOriginalYear.HasValue)
        {
            resolvedOriginalDate = Optional<DateOnly>.Some(new DateOnly(generatedYear, 1, 1));
            resolvedOriginalYear = Optional<int>.Some(generatedYear);
        }
        else if (!resolvedOriginalDate.HasValue)
        {
            resolvedOriginalDate = Optional<DateOnly>.Some(new DateOnly(resolvedOriginalYear.Value, 1, 1));
        }
        else if (!resolvedOriginalYear.HasValue)
        {
            resolvedOriginalYear = Optional<int>.Some(resolvedOriginalDate.Value.Year);
        }

        Optional<DateOnly> resolvedReReleaseDate = reReleaseDate ?? Optional<DateOnly>.None();
        Optional<int> resolvedReReleaseYear = reReleaseYear ?? Optional<int>.None();
        if (resolvedReReleaseDate.HasValue && !resolvedReReleaseYear.HasValue)
        {
            resolvedReReleaseYear = Optional<int>.Some(resolvedReReleaseDate.Value.Year);
        }
        else if (!resolvedReReleaseDate.HasValue && resolvedReReleaseYear.HasValue)
        {
            resolvedReReleaseDate = Optional<DateOnly>.Some(new DateOnly(resolvedReReleaseYear.Value, 1, 1));
        }

        Result<ReleaseInfo> releaseInfoResult = ReleaseInfo.Create(
            resolvedOriginalDate,
            resolvedOriginalYear,
            resolvedReReleaseDate,
            resolvedReReleaseYear,
            releaseCountry ?? Optional<string>.Some(_faker.Address.Country()),
            releaseVersion ?? Optional<string>.Some(_faker.Lorem.Word()));

        if (releaseInfoResult.IsFailure)
            throw new InvalidOperationException("Failed to create ReleaseInfo: " + string.Join(", ", releaseInfoResult.Errors));
        return releaseInfoResult.Value;
    }

    /// <summary>
    /// Creates multiple <see cref="ReleaseInfo"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ReleaseInfo"/> instances.</returns>
    public List<ReleaseInfo> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
