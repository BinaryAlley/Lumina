#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Commands.DeleteConsumptionRecord;

/// <summary>
/// Command for deleting one consumption record of a user.
/// </summary>
/// <param name="ConsumptionRecordId">The Id of the consumption record, taken from the route.</param>
/// <param name="TargetUserId">The Id of the user the record must belong to, or <see langword="null"/> for the current user.</param>
[DebuggerDisplay("ConsumptionRecordId: {ConsumptionRecordId}; TargetUserId: {TargetUserId}")]
public record DeleteConsumptionRecordCommand(
    string? ConsumptionRecordId,
    string? TargetUserId
) : ICommand;
