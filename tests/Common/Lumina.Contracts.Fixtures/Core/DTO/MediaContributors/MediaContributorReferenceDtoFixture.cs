#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;

/// <summary>
/// Fixture class for the <see cref="MediaContributorReferenceDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaContributorReferenceDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="MediaContributorReferenceDto"/>.
    /// </summary>
    /// <param name="contributorId">Optional. The Id of the media contributor.</param>
    /// <param name="role">Optional. The role the contributor played.</param>
    /// <returns>The created <see cref="MediaContributorReferenceDto"/>.</returns>
    public MediaContributorReferenceDto Create(Guid? contributorId = null, MediaContributorRole? role = null)
    {
        return new MediaContributorReferenceDto(
            ContributorId: contributorId ?? Guid.NewGuid(),
            Role: role ?? _faker.PickRandom<MediaContributorRole>());
    }

    /// <summary>
    /// Creates a list of <see cref="MediaContributorReferenceDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<MediaContributorReferenceDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
