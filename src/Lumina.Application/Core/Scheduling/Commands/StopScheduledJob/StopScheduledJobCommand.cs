#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Scheduling.Commands.StopScheduledJob;

/// <summary>
/// Command for stopping the execution cycle of a scheduled job.
/// </summary>
/// <param name="ScheduledJobId">The unique identifier of the scheduled job whose execution cycle is stopped.</param>
[DebuggerDisplay("ScheduledJobId: {ScheduledJobId}")]
public record StopScheduledJobCommand(
    Guid ScheduledJobId
) : ICommand;
