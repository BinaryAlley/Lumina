#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="AlbumContributorEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AlbumContributorEntityFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="AlbumContributorEntity"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the participation.</param>
    /// <param name="albumId">Optional. The Id of the album the contributor participated in.</param>
    /// <param name="mediaContributorId">Optional. The Id of the media contributor.</param>
    /// <param name="role">Optional. The role the contributor played in the album.</param>
    /// <param name="createdBy">Optional. The Id of the user that created the participation.</param>
    /// <returns>The created <see cref="AlbumContributorEntity"/>.</returns>
    public AlbumContributorEntity Create(
        Guid? id = null,
        Guid? albumId = null,
        Guid? mediaContributorId = null,
        MediaContributorRole? role = null,
        Guid? createdBy = null)
    {
        return new AlbumContributorEntity
        {
            Id = id ?? Guid.NewGuid(),
            AlbumId = albumId ?? Guid.NewGuid(),
            MediaContributorId = mediaContributorId ?? Guid.NewGuid(),
            Role = role ?? _faker.PickRandom<MediaContributorRole>(),
            CreatedOnUtc = _faker.Date.Past(),
            CreatedBy = createdBy ?? Guid.NewGuid(),
            UpdatedOnUtc = _faker.Random.Bool() ? _faker.Date.Recent() : null,
            UpdatedBy = _faker.Random.Bool() ? Guid.NewGuid() : null
        };
    }

    /// <summary>
    /// Creates a list of <see cref="AlbumContributorEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <param name="albumId">Optional. The Id of the album the contributors participated in.</param>
    /// <returns>List of configured <see cref="AlbumContributorEntity"/> instances.</returns>
    public List<AlbumContributorEntity> CreateMany(int count = 3, Guid? albumId = null)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create(albumId: albumId))];
    }
}
