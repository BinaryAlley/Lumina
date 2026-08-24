#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Core.Plugins.Commands.ReorderLibraryArtworkProviders;

/// <summary>
/// Command for reordering the artwork providers of a media library.
/// </summary>
/// <param name="LibraryId">The Id of the media library whose artwork providers are reordered.</param>
/// <param name="PluginIds">The plugin Ids in the new order, from highest to lowest rank.</param>
public record ReorderLibraryArtworkProvidersCommand(
    Guid LibraryId,
    IReadOnlyList<Guid> PluginIds
) : ICommand;
