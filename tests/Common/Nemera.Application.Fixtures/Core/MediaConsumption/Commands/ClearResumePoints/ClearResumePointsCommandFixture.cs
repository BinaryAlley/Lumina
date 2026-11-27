#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.MediaConsumption.Commands.ClearResumePoints;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaConsumption.Commands.ClearResumePoints;

/// <summary>
/// Fixture class for the <see cref="ClearResumePointsCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ClearResumePointsCommandFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="ClearResumePointsCommand"/>.
    /// </summary>
    /// <param name="targetUserId">Optional. The Id of the user whose resume points are cleared. Defaults to a random user Id.</param>
    /// <param name="libraryId">Optional. The Id of the media library the deletion is restricted to. Defaults to a random library Id.</param>
    /// <param name="includeTargetUserId">Whether the target user Id should be included, or forced to <see langword="null"/> so that the current user is targeted.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/> so that every library is cleared.</param>
    /// <returns>The created <see cref="ClearResumePointsCommand"/>.</returns>
    public ClearResumePointsCommand Create(
        string? targetUserId = null,
        string? libraryId = null,
        bool includeTargetUserId = true,
        bool includeLibraryId = true)
    {
        string? resolvedTargetUserId = includeTargetUserId ? targetUserId ?? _faker.Random.Guid().ToString() : null;
        string? resolvedLibraryId = includeLibraryId ? libraryId ?? _faker.Random.Guid().ToString() : null;

        return new ClearResumePointsCommand(resolvedTargetUserId, resolvedLibraryId);
    }

    /// <summary>
    /// Creates a list of <see cref="ClearResumePointsCommand"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ClearResumePointsCommand"/> instances.</returns>
    public List<ClearResumePointsCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
