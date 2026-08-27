#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaContributors;

/// <summary>
/// Fixture class for the <see cref="BookContributorEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookContributorEntityFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="BookContributorEntity"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the participation.</param>
    /// <param name="bookId">Optional. The Id of the book the contributor participated in.</param>
    /// <param name="mediaContributorId">Optional. The Id of the media contributor.</param>
    /// <param name="roleName">Optional. The display name of the role the contributor played in the book.</param>
    /// <param name="roleCategory">Optional. The canonical category of the role the contributor played in the book.</param>
    /// <returns>The created <see cref="BookContributorEntity"/>.</returns>
    public BookContributorEntity Create(
        Guid? id = null,
        Guid? bookId = null,
        Guid? mediaContributorId = null,
        string? roleName = null,
        MediaContributorRoleCategory? roleCategory = null)
    {
        return new BookContributorEntity
        {
            Id = id ?? Guid.NewGuid(),
            BookId = bookId ?? Guid.NewGuid(),
            MediaContributorId = mediaContributorId ?? Guid.NewGuid(),
            RoleName = roleName ?? _faker.Lorem.Word(),
            RoleCategory = roleCategory ?? _faker.PickRandom<MediaContributorRoleCategory>(),
            CreatedOnUtc = _faker.Date.Past(),
            CreatedBy = Guid.NewGuid(),
            UpdatedOnUtc = null,
            UpdatedBy = null
        };
    }

    /// <summary>
    /// Creates a list of <see cref="BookContributorEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="BookContributorEntity"/> instances.</returns>
    public List<BookContributorEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
