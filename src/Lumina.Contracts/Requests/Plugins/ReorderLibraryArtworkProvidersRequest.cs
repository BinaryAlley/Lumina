#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Plugins;

/// <summary>
/// Represents a request to reorder the artwork providers of a media library.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library whose artwork providers are reordered. Required.</param>
/// <param name="PluginIds">The plugin Ids in the new order, from highest to lowest rank. Required.</param>
[DebuggerDisplay("LibraryId: {LibraryId}")]
public sealed record ReorderLibraryArtworkProvidersRequest(
    Guid LibraryId,
    IReadOnlyList<Guid> PluginIds
);
