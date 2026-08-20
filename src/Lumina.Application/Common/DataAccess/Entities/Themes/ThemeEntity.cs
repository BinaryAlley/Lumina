#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Domain.SharedKernel.Common.Enums.Themes;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.Themes;

/// <summary>
/// Repository entity for a theme.
/// </summary>
[DebuggerDisplay("ThemeId: {ThemeId}, Name: {Name}")]
public class ThemeEntity : IStorageEntity, IAuditableEntity
{
    /// <summary>
    /// Gets the Id of the theme.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the unique identifier of the theme, taken from its manifest and used by clients to reference it.
    /// </summary>
    public required string ThemeId { get; set; }

    /// <summary>
    /// Gets or sets the display name of the theme.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the theme.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Gets or sets the author of the theme.
    /// </summary>
    public required string Author { get; set; }

    /// <summary>
    /// Gets or sets the version of the theme, using semantic version form.
    /// </summary>
    public required string Version { get; set; }

    /// <summary>
    /// Gets or sets the path of the theme preview image, relative to the theme pack root, or <see langword="null"/> when the theme has no preview.
    /// </summary>
    public string? PreviewPath { get; set; }

    /// <summary>
    /// Gets or sets the source the theme was installed from.
    /// </summary>
    public required ThemeInstallSource InstallSource { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is the currently active theme. At most one theme can be current.
    /// </summary>
    public bool? IsCurrent { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the theme was soft deleted, so it is not shown and not reinstalled automatically.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp at which the theme was installed.
    /// </summary>
    public required DateTime InstalledAtUtc { get; set; }

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
