#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.MediaContributors;

/// <summary>
/// Repository entity for a media contributor, the person that contributed to one or more media items.
/// The contributor is a person, unique by name, and agnostic of the kind of media it contributed to.
/// </summary>
[DebuggerDisplay("DisplayName: {DisplayName}")]
public class MediaContributorEntity : IStorageEntity, IAuditableEntity
{
    /// <summary>
    /// Gets the Id of the media contributor.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the name by which the contributor is popularly known.
    /// </summary>
    public required string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the legal name of the contributor.
    /// </summary>
    public string? LegalName { get; set; }

    /// <summary>
    /// Gets or sets the biography of the contributor.
    /// </summary>
    public string? Biography { get; set; }

    /// <summary>
    /// Gets or sets the date of birth of the contributor.
    /// </summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>
    /// Gets or sets the date of death of the contributor.
    /// </summary>
    public DateOnly? DateOfDeath { get; set; }

    /// <summary>
    /// Gets or sets the time and date when the entity was added.
    /// </summary>
    public required DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the Id of the user that created the entity.
    /// </summary>
    public required Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the optional time and date when the entity was updated.
    /// </summary>
    public DateTime? UpdatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the optional Id of the user that updated the entity.
    /// </summary>
    public required Guid? UpdatedBy { get; set; }
}
