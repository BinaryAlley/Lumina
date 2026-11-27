#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.MediaConsumption.Commands.DeleteConsumptionItemHistory;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaConsumption.Commands.DeleteConsumptionItemHistory;

/// <summary>
/// Fixture class for the <see cref="DeleteConsumptionItemHistoryCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteConsumptionItemHistoryCommandFixture
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
    /// Creates a random valid <see cref="DeleteConsumptionItemHistoryCommand"/>.
    /// </summary>
    /// <param name="targetUserId">Optional. The Id of the user the records must belong to. Defaults to a random user Id.</param>
    /// <param name="mediaKind">Optional. The name of the kind of the consumed media item. Defaults to the name of a random supported kind.</param>
    /// <param name="mediaItemId">Optional. The Id of the consumed media item. Defaults to a random Id.</param>
    /// <param name="libraryId">Optional. The Id of the media library that contains the media item. Defaults to a random Id.</param>
    /// <param name="includeTargetUserId">Whether the target user Id should be included, or forced to <see langword="null"/> so that the current user is targeted.</param>
    /// <param name="includeMediaKind">Whether the media kind name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMediaItemId">Whether the media item Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="DeleteConsumptionItemHistoryCommand"/>.</returns>
    public DeleteConsumptionItemHistoryCommand Create(
        string? targetUserId = null,
        string? mediaKind = null,
        string? mediaItemId = null,
        string? libraryId = null,
        bool includeTargetUserId = true,
        bool includeMediaKind = true,
        bool includeMediaItemId = true,
        bool includeLibraryId = true)
    {
        return new DeleteConsumptionItemHistoryCommand(
            includeTargetUserId ? targetUserId ?? _faker.Random.Guid().ToString() : null,
            includeMediaKind ? mediaKind ?? _faker.Random.ArrayElement(_supportedMediaKinds).ToString() : null,
            includeMediaItemId ? mediaItemId ?? _faker.Random.Guid().ToString() : null,
            includeLibraryId ? libraryId ?? _faker.Random.Guid().ToString() : null);
    }

    /// <summary>
    /// Creates a list of <see cref="DeleteConsumptionItemHistoryCommand"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="DeleteConsumptionItemHistoryCommand"/> instances.</returns>
    public List<DeleteConsumptionItemHistoryCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
