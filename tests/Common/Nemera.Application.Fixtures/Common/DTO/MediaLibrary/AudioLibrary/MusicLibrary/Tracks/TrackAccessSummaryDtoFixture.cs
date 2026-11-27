#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Fixture class for the <see cref="TrackAccessSummaryDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class TrackAccessSummaryDtoFixture
{
    /// <summary>
    /// Creates a random valid <see cref="TrackAccessSummaryDto"/>.
    /// </summary>
    /// <param name="trackCount">Optional. The number of distinct requested tracks that exist.</param>
    /// <param name="libraryIds">Optional. The Ids of the media libraries that contain those tracks. Defaults to a single random library.</param>
    /// <returns>The created <see cref="TrackAccessSummaryDto"/>.</returns>
    public TrackAccessSummaryDto Create(
        int? trackCount = null,
        IReadOnlyList<Guid>? libraryIds = null)
    {
        return new TrackAccessSummaryDto(
            trackCount ?? 1,
            libraryIds ?? [Guid.NewGuid()]);
    }

    /// <summary>
    /// Creates a list of <see cref="TrackAccessSummaryDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="TrackAccessSummaryDto"/> instances.</returns>
    public List<TrackAccessSummaryDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
