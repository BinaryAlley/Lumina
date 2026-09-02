#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.UsersManagement;

/// <summary>
/// Repository entity for the settings of a user.
/// </summary>
[DebuggerDisplay("Id: {Id}; UserId: {UserId}")]
public class UserSettingsEntity : IStorageEntity, IAuditableEntity
{
    /// <summary>
    /// Gets the Id of these user settings.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the Id of the user that owns these settings.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets or sets whether pagination is enabled for the user, or not.
    /// </summary>
    public bool IsPaginationEnabled { get; set; }

    /// <summary>
    /// Gets or sets the number of library items displayed per page when pagination is enabled.
    /// </summary>
    public int ItemsPerPage { get; set; }

    /// <summary>
    /// Gets or sets whether the "The" prefix of library item titles is ignored by the alpha picker, or not.
    /// </summary>
    public bool ShouldIgnoreThePrefixForAlphaPicker { get; set; }

    /// <summary>
    /// Gets or sets whether the theme data served to this user is cached, or not.
    /// </summary>
    public bool IsThemeCachingEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the metadata of the media library items is aggregated from multiple providers, when fields are missing, or not.
    /// </summary>
    public bool ShouldAggregateMetadataWhenMissing { get; set; }

    /// <summary>
    /// Gets or sets whether PDF books are rendered as page images for the user, or not. Rendering PDFs as images preserves the
    /// original layout, but the pages are not selectable or searchable, unlike their text layer.
    /// </summary>
    public bool ShouldRenderPdfAsImages { get; set; }

    /// <summary>
    /// Gets or sets whether the styles of the book content are preserved when it is rendered for the user, or not. Preserving them
    /// keeps the original look of a book (its fonts, colors, and alignment), but the styles of a book can load resources from
    /// external servers, which those servers could observe; stripping them removes that possibility, at the cost of the look of the book.
    /// </summary>
    public bool ShouldPreserveBookStyles { get; set; }

    /// <summary>
    /// Gets or sets the time and date when the entity was added.
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the Id of the user that created the entity.
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the optional time and date when the entity was updated.
    /// </summary>
    public DateTime? UpdatedOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the optional Id of the user that updated the entity.
    /// </summary>
    public Guid? UpdatedBy { get; set; }
}
