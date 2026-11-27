#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.MediaContributorBoundedContext.MediaContributorAggregate;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.MediaContributorBoundedContext.MediaContributorAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="MusicMediaContributor"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicMediaContributorFixture
{
    private readonly Faker _faker = new();
    private readonly MediaContributorIdFixture _mediaContributorIdFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="MusicMediaContributor"/>.
    /// </summary>
    /// <param name="contributorId">Optional. The unique identifier of the media contributor.</param>
    /// <param name="role">Optional. The role the contributor played.</param>
    /// <returns>The created <see cref="MusicMediaContributor"/>.</returns>
    public MusicMediaContributor Create(
        MediaContributorId? contributorId = null,
        MediaContributorRole? role = null)
    {
        Result<MusicMediaContributor> contributorResult = MusicMediaContributor.Create(
            contributorId ?? _mediaContributorIdFixture.Create(),
            role ?? _faker.PickRandom<MediaContributorRole>());

        if (contributorResult.IsFailure)
            throw new InvalidOperationException("Failed to create MusicMediaContributor: " + string.Join(", ", contributorResult.Errors));
        return contributorResult.Value;
    }

    /// <summary>
    /// Creates multiple <see cref="MusicMediaContributor"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicMediaContributor"/> instances.</returns>
    public List<MusicMediaContributor> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
