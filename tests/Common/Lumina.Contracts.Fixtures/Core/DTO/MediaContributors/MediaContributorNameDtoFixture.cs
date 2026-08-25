#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.MediaContributors;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;

/// <summary>
/// Fixture class for the <see cref="MediaContributorNameDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaContributorNameDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="MediaContributorNameDto"/>.
    /// </summary>
    /// <param name="displayName">Optional. The display name of the media contributor.</param>
    /// <param name="legalName">Optional. The legal name of the media contributor.</param>
    /// <returns>The created <see cref="MediaContributorNameDto"/>.</returns>
    public MediaContributorNameDto Create(
        string? displayName = null, 
        string? legalName = null)
    {
        return new MediaContributorNameDto(
            displayName ?? _faker.Name.FullName(),
            legalName ?? _faker.Name.FullName());
    }

    /// <summary>
    /// Creates a list of <see cref="MediaContributorNameDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<MediaContributorNameDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
