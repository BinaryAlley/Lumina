#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Commands.DeleteConsumptionItemHistory;

/// <summary>
/// Command for deleting every consumption record of a media item of a user.
/// </summary>
/// <param name="TargetUserId">The Id of the user the records must belong to, or <see langword="null"/> for the current user.</param>
/// <param name="MediaKind">The name of the kind of the consumed media item, taken from the route.</param>
/// <param name="MediaItemId">The Id of the consumed media item, taken from the route.</param>
/// <param name="LibraryId">The Id of the media library that contains the media item, taken from the route.</param>
[DebuggerDisplay("TargetUserId: {TargetUserId}; MediaKind: {MediaKind}; MediaItemId: {MediaItemId}")]
public record DeleteConsumptionItemHistoryCommand(
    string? TargetUserId,
    string? MediaKind,
    string? MediaItemId,
    string? LibraryId
) : ICommand;
