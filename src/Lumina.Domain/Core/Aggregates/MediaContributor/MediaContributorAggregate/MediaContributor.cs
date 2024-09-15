#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.Aggregates.MediaContributor.MediaContributorAggregate.ValueObjects;
using System;
using System.Diagnostics;
#endregion


namespace Lumina.Domain.Core.Aggregates.MediaContributor.MediaContributorAggregate;

/// <summary>
/// Aggregate Root for a media contributor.
/// </summary>
[DebuggerDisplay("{Id}: {Name}")]
public class MediaContributor : AggregateRoot<MediaContributorId>
{
    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets the name of the contributor.
    /// </summary>
    public MediaContributorName Name { get; private set; }

    /// <summary>
    /// Gets the role of the contributor.
    /// </summary>
    public MediaContributorRole Role { get; private set; }

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
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaContributor"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the contributor.</param>
    /// <param name="name">The name of the contributor.</param>
    /// <param name="role">The role of the contributor.</param>
    /// <param name="dateOfBirth">The optional date of birth of the contributor.</param>
    /// <param name="dateOfBirth">The optional date of death of the contributor.</param>
    private MediaContributor(
        MediaContributorId id, 
        MediaContributorName name, 
        MediaContributorRole role, 
        Optional<DateOnly> dateOfBirth, 
        Optional<DateOnly> dateOfDeath)
    {
        Id = id;
        Name = name;
        Role = role;
        DateOfBirth = dateOfBirth;
        DateOfDeath = dateOfDeath;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a new instance of the <see cref="MediaContributor"/> class.
    /// </summary>
    /// <param name="name">The name of the contributor.</param>
    /// <returns>The created <see cref="MediaContributor"/> instance.</returns>
    public static ErrorOr<MediaContributor> Create(MediaContributorName name)
    {
        // TODO: enforce invariants
        throw new NotImplementedException();
        //return new MediaContributor(id, name);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MediaContributor"/> class, from a pre-existing <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The unique identifier of the contributor.</param>
    /// <param name="name">The name of the contributor.</param>
    /// <returns>The created <see cref="MediaContributor"/> instance.</returns>
    public static ErrorOr<MediaContributor> Create(
        MediaContributorId id, 
        MediaContributorName name, 
        MediaContributorRole role, 
        Optional<DateOnly> dateOfBirth, 
        Optional<DateOnly> dateOfDeath)
    {
        return new MediaContributor(id, name, role, dateOfBirth, dateOfDeath);
    }
    #endregion
}