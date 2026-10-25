#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Commands.DeleteConsumptionItemHistory;

/// <summary>
/// Command for deleting every consumption record of a media item of a user.
/// </summary>
/// <param name="TargetUserId">The Id of the user the records must belong to, or <see langword="null"/> for the current user.</param>
/// <param name="MediaKind">The kind of the consumed media item.</param>
/// <param name="MediaItemId">The Id of the consumed media item.</param>
/// <param name="LibraryId">The Id of the media library that contains the media item.</param>
[DebuggerDisplay("TargetUserId: {TargetUserId}; MediaKind: {MediaKind}; MediaItemId: {MediaItemId}")]
public record DeleteConsumptionItemHistoryCommand(
    string? TargetUserId,
    MediaKind MediaKind,
    Guid MediaItemId,
    Guid LibraryId
) : ICommand;
