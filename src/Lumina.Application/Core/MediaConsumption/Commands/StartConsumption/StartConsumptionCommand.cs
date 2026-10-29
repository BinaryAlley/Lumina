#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Commands.StartConsumption;

/// <summary>
/// Command for starting a consumption session of a media item by the current user.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library that contains the media item, taken from the route.</param>
/// <param name="MediaKind">The kind of media that is consumed, taken from the route.</param>
/// <param name="MediaItemId">The unique identifier of the consumed media item, taken from the route.</param>
[DebuggerDisplay("MediaKind: {MediaKind}; MediaItemId: {MediaItemId}")]
public record StartConsumptionCommand(
    string? LibraryId,
    string? MediaKind,
    string? MediaItemId
) : ICommand;
