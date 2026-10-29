#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.MediaConsumption.Queries.GetResumePoint;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaConsumption.Queries.GetResumePoint;

/// <summary>
/// Fixture class for the <see cref="GetResumePointQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetResumePointQueryFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="GetResumePointQuery"/>.
    /// </summary>
    /// <param name="mediaKind">Optional. The name of the kind of media to resume, taken from the route. Defaults to a resumable kind supported by the current catalogues.</param>
    /// <param name="mediaItemId">Optional. The Id of the media item to resume, taken from the route. Defaults to a random Id.</param>
    /// <param name="includeMediaKind">Whether the media kind should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMediaItemId">Whether the media item Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="GetResumePointQuery"/>.</returns>
    public GetResumePointQuery Create(
        string? mediaKind = null,
        string? mediaItemId = null,
        bool includeMediaKind = true,
        bool includeMediaItemId = true)
    {
        string? resolvedMediaKind = includeMediaKind ? mediaKind ?? nameof(MediaKind.MusicTrack) : null;
        string? resolvedMediaItemId = includeMediaItemId ? mediaItemId ?? _faker.Random.Guid().ToString() : null;

        return new GetResumePointQuery(resolvedMediaKind, resolvedMediaItemId);
    }

    /// <summary>
    /// Creates a list of <see cref="GetResumePointQuery"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetResumePointQuery"/> instances.</returns>
    public List<GetResumePointQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
