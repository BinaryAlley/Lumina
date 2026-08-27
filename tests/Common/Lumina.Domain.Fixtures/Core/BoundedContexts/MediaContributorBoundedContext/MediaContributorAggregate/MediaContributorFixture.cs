#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate;
using Lumina.Domain.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate;

/// <summary>
/// Fixture class for the <see cref="MediaContributor"/> domain aggregate.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaContributorFixture
{
    private readonly MediaContributorIdFixture _mediaContributorIdFixture = new();
    private readonly MediaContributorNameFixture _mediaContributorNameFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="MediaContributor"/> domain aggregate.
    /// </summary>
    /// <param name="id">Optional. The Id of the contributor.</param>
    /// <param name="name">Optional. The name of the contributor.</param>
    /// <param name="biography">Optional. The biography of the contributor.</param>
    /// <param name="dateOfBirth">Optional. The date of birth of the contributor.</param>
    /// <param name="dateOfDeath">Optional. The date of death of the contributor.</param>
    /// <returns>The created <see cref="MediaContributor"/> domain aggregate.</returns>
    public MediaContributor Create(
        MediaContributorId? id = null,
        MediaContributorName? name = null,
        Optional<string>? biography = null,
        Optional<DateOnly>? dateOfBirth = null,
        Optional<DateOnly>? dateOfDeath = null)
    {
        Result<MediaContributor> result = MediaContributor.Create(
            id ?? _mediaContributorIdFixture.Create(),
            name ?? _mediaContributorNameFixture.Create(),
            biography ?? Optional<string>.None(),
            dateOfBirth ?? Optional<DateOnly>.None(),
            dateOfDeath ?? Optional<DateOnly>.None());

        if (result.IsFailure)
            throw new InvalidOperationException("Failed to create MediaContributor: " + string.Join(", ", result.Errors));
        return result.Value;
    }

    /// <summary>
    /// Creates multiple <see cref="MediaContributor"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MediaContributor"/> instances.</returns>
    public List<MediaContributor> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
