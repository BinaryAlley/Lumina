#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate;

/// <summary>
/// Aggregate root for a media contributor, the person that contributed to a media item.
/// A media contributor is a person, unique by name, agnostic of the kind of media it contributed to, and of
/// the roles it played in the media items. The roles a contributor plays are tracked per media item, so that a
/// single contributor that is both an actor and a writer is never duplicated.
/// </summary>
[DebuggerDisplay("{Id}: {Name}")]
public class MediaContributor : AggregateRoot<MediaContributorId>
{
    /// <summary>
    /// Gets the name of the contributor.
    /// </summary>
    public MediaContributorName Name { get; private set; }

    /// <summary>
    /// Gets the biography of the contributor.
    /// </summary>
    public Optional<string> Biography { get; private set; }

    /// <summary>
    /// Gets the date of birth of the contributor.
    /// </summary>
    public Optional<DateOnly> DateOfBirth { get; private set; }

    /// <summary>
    /// Gets the date of death of the contributor.
    /// </summary>
    public Optional<DateOnly> DateOfDeath { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaContributor"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the contributor.</param>
    /// <param name="name">The name of the contributor.</param>
    /// <param name="biography">The optional biography of the contributor.</param>
    /// <param name="dateOfBirth">The optional date of birth of the contributor.</param>
    /// <param name="dateOfDeath">The optional date of death of the contributor.</param>
    private MediaContributor(
        MediaContributorId id,
        MediaContributorName name,
        Optional<string> biography,
        Optional<DateOnly> dateOfBirth,
        Optional<DateOnly> dateOfDeath) : base(id)
    {
        Id = id;
        Name = name;
        Biography = biography;
        DateOfBirth = dateOfBirth;
        DateOfDeath = dateOfDeath;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MediaContributor"/> class.
    /// </summary>
    /// <param name="name">The name of the contributor.</param>
    /// <returns>The created <see cref="MediaContributor"/> instance.</returns>
    public static Result<MediaContributor> Create(MediaContributorName name)
    {
        return new MediaContributor(
            MediaContributorId.CreateUnique(),
            name,
            Optional<string>.None(),
            Optional<DateOnly>.None(),
            Optional<DateOnly>.None());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MediaContributor"/> class, from a pre-existing <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The unique identifier of the contributor.</param>
    /// <param name="name">The name of the contributor.</param>
    /// <param name="biography">The optional biography of the contributor.</param>
    /// <param name="dateOfBirth">The optional date of birth of the contributor.</param>
    /// <param name="dateOfDeath">The optional date of death of the contributor.</param>
    /// <returns>The created <see cref="MediaContributor"/> instance.</returns>
    public static Result<MediaContributor> Create(
        MediaContributorId id,
        MediaContributorName name,
        Optional<string> biography,
        Optional<DateOnly> dateOfBirth,
        Optional<DateOnly> dateOfDeath)
    {
        return new MediaContributor(id, name, biography, dateOfBirth, dateOfDeath);
    }
}
