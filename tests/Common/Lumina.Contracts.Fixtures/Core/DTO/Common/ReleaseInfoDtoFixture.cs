#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.Common;

/// <summary>
/// Fixture class for the <see cref="ReleaseInfoDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReleaseInfoDtoFixture
{
    /// <summary>
    /// Creates an <see cref="ReleaseInfoDto"/>, preserving the values that are not provided as <see langword="null"/>.
    /// </summary>
    /// <param name="originalReleaseDate">Optional. The original release date of the content.</param>
    /// <param name="originalReleaseYear">Optional. The original release year of the content.</param>
    /// <param name="reReleaseDate">Optional. The re-release date of the content.</param>
    /// <param name="reReleaseYear">Optional. The re-release year of the content.</param>
    /// <param name="releaseCountry">Optional. The country where the content was released.</param>
    /// <param name="releaseVersion">Optional. The version or edition of the content's release.</param>
    /// <returns>The created <see cref="ReleaseInfoDto"/>.</returns>
    public ReleaseInfoDto Create(
        DateOnly? originalReleaseDate = null,
        int? originalReleaseYear = null,
        DateOnly? reReleaseDate = null,
        int? reReleaseYear = null,
        string? releaseCountry = null,
        string? releaseVersion = null)
    {
        return new ReleaseInfoDto(
            originalReleaseDate,
            originalReleaseYear,
            reReleaseDate,
            reReleaseYear,
            releaseCountry,
            releaseVersion);
    }

    /// <summary>
    /// Creates a list of <see cref="ReleaseInfoDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ReleaseInfoDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
