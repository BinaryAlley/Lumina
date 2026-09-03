#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Scheduling.Commands.StartScheduledJob;

/// <summary>
/// Command for starting the execution cycle of a scheduled job.
/// </summary>
/// <param name="ScheduledJobId">The unique identifier of the scheduled job whose execution cycle is started.</param>
[DebuggerDisplay("ScheduledJobId: {ScheduledJobId}")]
public record StartScheduledJobCommand(
    Guid ScheduledJobId
) : ICommand;
