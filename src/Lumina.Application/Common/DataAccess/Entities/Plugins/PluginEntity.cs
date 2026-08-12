#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Domain.SharedKernel.Common.Enums.Plugins;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Common.DataAccess.Entities.Plugins;

/// <summary>
/// Repository entity for a plugin.
/// </summary>
[DebuggerDisplay("Name: {Name}")]
public class PluginEntity : IStorageEntity, IAuditableEntity
{
    /// <summary>
    /// Gets the Id of the plugin.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the display name of the plugin.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the author of the plugin.
    /// </summary>
    public required string Author { get; set; }

    /// <summary>
    /// Gets or sets the version of the plugin.
    /// </summary>
    public required string Version { get; set; }

    /// <summary>
    /// Gets or sets the description of the plugin.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Gets or sets the load status of the plugin.
    /// </summary>
    public PluginLoadStatus LoadStatus { get; set; }

    /// <summary>
    /// Gets or sets the error message when the plugin failed to load.
    /// </summary>
    public string? LoadError { get; set; }

    /// <summary>
    /// Gets or sets the serialized settings of the plugin.
    /// </summary>
    public string? SettingsJson { get; set; }

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
