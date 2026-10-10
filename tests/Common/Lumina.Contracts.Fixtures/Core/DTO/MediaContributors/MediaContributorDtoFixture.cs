#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;

/// <summary>
/// Fixture class for the <see cref="MediaContributorDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaContributorDtoFixture
{
    private readonly Faker _faker = new();
    private readonly MediaContributorNameDtoFixture _mediaContributorNameDtoFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="MediaContributorDto"/>.
    /// </summary>
    /// <param name="displayName">Optional. The display name of the media contributor.</param>
    /// <param name="role">Optional. The canonical role assigned to the media contributor.</param>
    /// <param name="legalName">Optional. The legal name of the media contributor.</param>
    /// <param name="includeName">Whether the name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeDisplayName">Whether the display name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLegalName">Whether the legal name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeRole">Whether the role should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="MediaContributorDto"/>.</returns>
    public MediaContributorDto Create(
        string? displayName = null,
        MediaContributorRole? role = null,
        string? legalName = null,
        bool includeName = true,
        bool includeDisplayName = true,
        bool includeLegalName = true,
        bool includeRole = true)
    {
        return new MediaContributorDto(
            Name: includeName ? _mediaContributorNameDtoFixture.Create(
                displayName: displayName,
                legalName: legalName,
                includeDisplayName: includeDisplayName,
                // this fixture historically left the legal name null unless one was explicitly supplied, so the name fixture is only asked for one when a value is passed
                includeLegalName: includeLegalName && legalName is not null) : null,
            Role: includeRole ? role ?? _faker.PickRandom<MediaContributorRole>() : null);
    }

    /// <summary>
    /// Creates a list of <see cref="MediaContributorDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<MediaContributorDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
