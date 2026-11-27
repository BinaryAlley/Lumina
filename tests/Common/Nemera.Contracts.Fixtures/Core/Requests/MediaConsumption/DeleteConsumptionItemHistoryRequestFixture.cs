#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.MediaConsumption;

/// <summary>
/// Fixture class for the <see cref="DeleteConsumptionItemHistoryRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteConsumptionItemHistoryRequestFixture
{
    private readonly Faker _faker = new();
    private readonly MediaKind[] _supportedMediaKinds =
    [
        MediaKind.MusicTrack,
        MediaKind.PodcastEpisode,
        MediaKind.Audiobook,
        MediaKind.Movie,
        MediaKind.TvEpisode,
        MediaKind.Book,
        MediaKind.Photo,
        MediaKind.Video
    ];

    /// <summary>
    /// Creates a random valid <see cref="DeleteConsumptionItemHistoryRequest"/>.
    /// </summary>
    /// <param name="mediaKind">Optional. The name of the kind of the consumed media item. Defaults to the name of a random supported kind.</param>
    /// <param name="mediaItemId">Optional. The Id of the consumed media item. Defaults to a random Id.</param>
    /// <param name="libraryId">Optional. The Id of the media library that contains the media item. Defaults to a random Id.</param>
    /// <param name="includeMediaKind">Whether the media kind name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMediaItemId">Whether the media item Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="DeleteConsumptionItemHistoryRequest"/>.</returns>
    public DeleteConsumptionItemHistoryRequest Create(
        string? mediaKind = null,
        string? mediaItemId = null,
        string? libraryId = null,
        bool includeMediaKind = true,
        bool includeMediaItemId = true,
        bool includeLibraryId = true)
    {
        return new DeleteConsumptionItemHistoryRequest(
            includeMediaKind ? mediaKind ?? _faker.Random.ArrayElement(_supportedMediaKinds).ToString() : null,
            includeMediaItemId ? mediaItemId ?? _faker.Random.Guid().ToString() : null,
            includeLibraryId ? libraryId ?? _faker.Random.Guid().ToString() : null);
    }

    /// <summary>
    /// Creates a list of <see cref="DeleteConsumptionItemHistoryRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="DeleteConsumptionItemHistoryRequest"/> instances.</returns>
    public List<DeleteConsumptionItemHistoryRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
