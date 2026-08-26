#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Plugins;

/// <summary>
/// Represents a request to reorder the artwork providers of a media library.
/// </summary>
[DebuggerDisplay("LibraryId: {LibraryId}")]
public class ReorderLibraryArtworkProvidersRequest
{
    /// <summary>
    /// Gets or sets the Id of the media library whose artwork providers are reordered.
    /// </summary>
    public Guid LibraryId { get; set; }

    /// <summary>
    /// Gets or sets the plugin Ids in the new order, from highest to lowest rank.
    /// </summary>
    public List<Guid> PluginIds { get; set; } = [];
}
