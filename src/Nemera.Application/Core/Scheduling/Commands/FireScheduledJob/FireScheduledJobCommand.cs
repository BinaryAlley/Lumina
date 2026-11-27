#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Scheduling.Commands.FireScheduledJob;

/// <summary>
/// Command for firing the task of a scheduled job once, without affecting its execution cycle.
/// </summary>
/// <param name="ScheduledJobId">The unique identifier of the scheduled job whose task is fired.</param>
[DebuggerDisplay("ScheduledJobId: {ScheduledJobId}")]
public record FireScheduledJobCommand(
    Guid ScheduledJobId
) : ICommand;
