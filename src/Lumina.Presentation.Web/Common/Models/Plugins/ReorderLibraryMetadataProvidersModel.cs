#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Presentation.Web.Common.Models.Plugins;

/// <summary>
/// Represents a request to reorder the metadata providers of a media library.
/// </summary>
public class ReorderLibraryMetadataProvidersModel
{
    /// <summary>
    /// Gets or sets the Id of the media library whose metadata providers are reordered.
    /// </summary>
    public Guid LibraryId { get; set; }

    /// <summary>
    /// Gets or sets the plugin Ids in the new order, from highest to lowest rank.
    /// </summary>
    public List<Guid> PluginIds { get; set; } = [];
}
