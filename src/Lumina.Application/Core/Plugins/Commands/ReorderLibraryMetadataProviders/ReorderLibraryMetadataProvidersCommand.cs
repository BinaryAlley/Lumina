#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Mediator;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Core.Plugins.Commands.ReorderLibraryMetadataProviders;

/// <summary>
/// Command for reordering the metadata providers of a media library.
/// </summary>
/// <param name="LibraryId">The Id of the media library whose metadata providers are reordered.</param>
/// <param name="PluginIds">The plugin Ids in the new order, from highest to lowest rank.</param>
public record ReorderLibraryMetadataProvidersCommand(
    Guid LibraryId,
    IReadOnlyList<Guid> PluginIds
) : IRequest<ErrorOr<Success>>;
