#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Plugins;

/// <summary>
/// Represents a request to reorder the metadata providers of a media library.
/// </summary>
/// <param name="LibraryId">The Id of the media library whose metadata providers are reordered. Required.</param>
/// <param name="PluginIds">The plugin Ids in the new order, from highest to lowest rank. Required.</param>
[DebuggerDisplay("LibraryId: {LibraryId}")]
public record ReorderLibraryMetadataProvidersRequest(
    Guid LibraryId,
    List<Guid> PluginIds
);
