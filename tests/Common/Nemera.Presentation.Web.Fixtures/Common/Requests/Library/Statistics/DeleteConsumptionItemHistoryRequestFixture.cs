#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using Lumina.Presentation.Web.Common.Requests.Library.Statistics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Library.Statistics;

/// <summary>
/// Fixture class for the <see cref="DeleteConsumptionItemHistoryRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteConsumptionItemHistoryRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="DeleteConsumptionItemHistoryRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="userId">Optional Id of the user whose history is deleted.</param>
    /// <param name="mediaKind">Optional kind of the consumed media item.</param>
    /// <param name="mediaItemId">Optional Id of the consumed media item.</param>
    /// <param name="libraryId">Optional Id of the media library that contains the media item.</param>
    /// <param name="includeUserId">Whether the user Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="DeleteConsumptionItemHistoryRequest"/> instance.</returns>
    public DeleteConsumptionItemHistoryRequest Create(
        string? userId = null,
        MediaKind? mediaKind = null,
        Guid? mediaItemId = null,
        Guid? libraryId = null,
        bool includeUserId = true)
    {
        return new DeleteConsumptionItemHistoryRequest(
            UserId: includeUserId ? (userId ?? _faker.Random.Guid().ToString()) : null,
            MediaKind: mediaKind ?? _faker.PickRandom(MediaKind.MusicTrack, MediaKind.PodcastEpisode, MediaKind.Audiobook, MediaKind.Movie, MediaKind.TvEpisode, MediaKind.Book, MediaKind.Photo, MediaKind.Video),
            MediaItemId: mediaItemId ?? _faker.Random.Guid(),
            LibraryId: libraryId ?? _faker.Random.Guid()
        );
    }

    /// <summary>
    /// Creates multiple <see cref="DeleteConsumptionItemHistoryRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="DeleteConsumptionItemHistoryRequest"/> instances.</returns>
    public List<DeleteConsumptionItemHistoryRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
