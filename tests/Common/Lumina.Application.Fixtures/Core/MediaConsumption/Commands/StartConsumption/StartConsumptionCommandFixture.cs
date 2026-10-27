#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.MediaConsumption.Commands.StartConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaConsumption.Commands.StartConsumption;

/// <summary>
/// Fixture class for the <see cref="StartConsumptionCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StartConsumptionCommandFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="StartConsumptionCommand"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library that contains the media item, taken from the route.</param>
    /// <param name="mediaKind">Optional. The kind of media that is consumed, taken from the route. Defaults to the name of a kind supported by the current catalogues.</param>
    /// <param name="mediaItemId">Optional. The Id of the consumed media item, taken from the route.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMediaKind">Whether the media kind should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMediaItemId">Whether the media item Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="StartConsumptionCommand"/>.</returns>
    public StartConsumptionCommand Create(
        string? libraryId = null,
        string? mediaKind = null,
        string? mediaItemId = null,
        bool includeLibraryId = true,
        bool includeMediaKind = true,
        bool includeMediaItemId = true)
    {
        string? resolvedLibraryId = includeLibraryId ? libraryId ?? _faker.Random.Guid().ToString() : null;
        string? resolvedMediaKind = includeMediaKind ? mediaKind ?? nameof(MediaKind.MusicTrack) : null;
        string? resolvedMediaItemId = includeMediaItemId ? mediaItemId ?? _faker.Random.Guid().ToString() : null;

        return new StartConsumptionCommand(resolvedLibraryId, resolvedMediaKind, resolvedMediaItemId);
    }

    /// <summary>
    /// Creates a list of <see cref="StartConsumptionCommand"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="StartConsumptionCommand"/> instances.</returns>
    public List<StartConsumptionCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
