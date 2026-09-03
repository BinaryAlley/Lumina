#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Scheduling.Commands.RemoveScheduledJob;

/// <summary>
/// Command for removing a scheduled job.
/// </summary>
/// <param name="ScheduledJobId">The unique identifier of the scheduled job to remove.</param>
[DebuggerDisplay("ScheduledJobId: {ScheduledJobId}")]
public record RemoveScheduledJobCommand(
    Guid ScheduledJobId
) : ICommand;
