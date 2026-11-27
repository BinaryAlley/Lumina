#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Nemera.Application.Core.Scheduling.Commands.StopScheduledJob;

/// <summary>
/// Command for stopping the execution cycle of a scheduled job.
/// </summary>
/// <param name="ScheduledJobId">The unique identifier of the scheduled job whose execution cycle is stopped.</param>
[DebuggerDisplay("ScheduledJobId: {ScheduledJobId}")]
public record StopScheduledJobCommand(
    Guid ScheduledJobId
) : ICommand;
