#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Fixtures.Common.Setup;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaContributors;

/// <summary>
/// Fixture class for the <see cref="MediaContributorEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaContributorEntityFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="MediaContributorEntity"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the media contributor.</param>
    /// <param name="displayName">Optional. The name by which the contributor is popularly known.</param>
    /// <param name="legalName">Optional. The legal name of the contributor.</param>
    /// <param name="biography">Optional. The biography of the contributor.</param>
    /// <param name="dateOfBirth">Optional. The date of birth of the contributor.</param>
    /// <param name="dateOfDeath">Optional. The date of death of the contributor.</param>
    /// <returns>The created <see cref="MediaContributorEntity"/>.</returns>
    public MediaContributorEntity Create(
        Guid? id = null,
        string? displayName = null,
        string? legalName = null,
        string? biography = null,
        DateOnly? dateOfBirth = null,
        DateOnly? dateOfDeath = null)
    {
        return new MediaContributorEntity
        {
            Id = id ?? Guid.NewGuid(),
            DisplayName = displayName ?? _faker.Person.FullName,
            LegalName = legalName ?? _faker.Person.FullName,
            Biography = biography ?? _faker.Lorem.Sentence(),
            DateOfBirth = dateOfBirth ?? _faker.DateOnlyBetween(new DateOnly(1900, 1, 1), new DateOnly(2000, 12, 31)),
            DateOfDeath = dateOfDeath,
            CreatedOnUtc = _faker.Date.Past(),
            CreatedBy = Guid.NewGuid(),
            UpdatedOnUtc = null,
            UpdatedBy = null
        };
    }

    /// <summary>
    /// Creates a list of <see cref="MediaContributorEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MediaContributorEntity"/> instances.</returns>
    public List<MediaContributorEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
