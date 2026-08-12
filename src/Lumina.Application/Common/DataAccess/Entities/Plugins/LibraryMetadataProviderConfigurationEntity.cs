#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.Plugins;

/// <summary>
/// Repository entity for the configuration of a metadata provider of a media library.
/// </summary>
[DebuggerDisplay("LibraryId: {LibraryId}, PluginId: {PluginId}")]
public class LibraryMetadataProviderConfigurationEntity : IStorageEntity, IAuditableEntity
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
    /// Gets or sets the Id of the plugin providing the metadata.
    /// </summary>
    public required Guid PluginId { get; set; }

    /// <summary>
    /// Gets or sets whether the metadata provider is enabled for the media library.
    /// </summary>
    public required bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets the rank of the metadata provider, determining the order in which providers are tried.
    /// </summary>
    public required int Rank { get; set; }

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
