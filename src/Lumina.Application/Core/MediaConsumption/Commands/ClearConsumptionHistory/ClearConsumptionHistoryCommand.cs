#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Commands.ClearConsumptionHistory;

/// <summary>
/// Command for deleting every consumption record of a user.
/// </summary>
/// <param name="TargetUserId">The Id of the user whose history is deleted, or <see langword="null"/> for the current user.</param>
[DebuggerDisplay("TargetUserId: {TargetUserId}")]
public record ClearConsumptionHistoryCommand(
    string? TargetUserId
) : ICommand;
