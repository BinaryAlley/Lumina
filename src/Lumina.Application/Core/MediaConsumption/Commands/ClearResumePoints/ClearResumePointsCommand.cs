#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Commands.ClearResumePoints;

/// <summary>
/// Command for deleting the resume points of a user.
/// </summary>
/// <param name="TargetUserId">The Id of the user whose resume points are deleted, or <see langword="null"/> for the current user.</param>
/// <param name="LibraryId">The Id of the media library to restrict the deletion to, or <see langword="null"/> for every library.</param>
[DebuggerDisplay("TargetUserId: {TargetUserId}; LibraryId: {LibraryId}")]
public record ClearResumePointsCommand(
    string? TargetUserId,
    string? LibraryId
) : ICommand;
