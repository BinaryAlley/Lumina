#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.Plugins;

/// <summary>
/// Repository entity for the configuration of a book reader of a media library.
/// </summary>
[DebuggerDisplay("LibraryId: {LibraryId}, PluginId: {PluginId}")]
public class LibraryBookReaderConfigurationEntity : IStorageEntity, IAuditableEntity
{
    /// <summary>
    /// Gets or sets the Id of the configuration.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the Id of the media library the configuration belongs to.
    /// </summary>
    public required Guid LibraryId { get; set; }

    /// <summary>
    /// Gets or sets the Id of the plugin providing the book reader.
    /// </summary>
    public required Guid PluginId { get; set; }

    /// <summary>
    /// Gets or sets whether the book reader is enabled for the media library.
    /// </summary>
    public required bool IsEnabled { get; set; }

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
