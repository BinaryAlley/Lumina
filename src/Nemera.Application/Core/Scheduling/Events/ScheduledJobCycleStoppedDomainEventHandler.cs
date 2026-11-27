#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.Scheduling;
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.Application.Common.DTO.Filtering;
using Nemera.Application.Common.DTO.Pagination;
using Nemera.Application.Common.Mapping.Scheduling;
using Nemera.Application.Core.Scheduling.Notifications;
using Nemera.Contracts.Responses.Scheduling;
using Nemera.Domain.Common.Events;
using Nemera.Domain.Common.Exceptions;
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.Events;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Application.Core.Scheduling.Events;

/// <summary>
/// Handler for the domain event raised when the execution cycle of a scheduled job is stopped.
/// </summary>
public class ScheduledJobCycleStoppedDomainEventHandler : IDomainEventHandler<ScheduledJobCycleStoppedDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IScheduledJobNotifier _scheduledJobNotifier;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledJobCycleStoppedDomainEventHandler"/> class.
    /// </summary>
    /// <param name="scheduledJobNotifier">Injected service for notifying the scheduled job changes to SignalR clients.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public ScheduledJobCycleStoppedDomainEventHandler(IScheduledJobNotifier scheduledJobNotifier, IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _scheduledJobNotifier = scheduledJobNotifier;
    }

    /// <summary>
    /// Handles the event raised when the execution cycle of a scheduled job is stopped.
    /// </summary>
    /// <param name="domainEvent">The domain event to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public async ValueTask HandleAsync(ScheduledJobCycleStoppedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        Result<PaginatedResultDto<ScheduledJobEntity>> getScheduledJobsResult = await _unitOfWork.ScheduledJobRepository.GetAllAsync<BaseFilterDto>(cancellationToken: cancellationToken).ConfigureAwait(false);
        if (getScheduledJobsResult.IsFailure)
            throw new EventualConsistencyException(getScheduledJobsResult.FirstError, getScheduledJobsResult.Errors);
        IReadOnlyList<ScheduledJobResponse> scheduledJobResponses = [.. getScheduledJobsResult.Value.Data.Select(scheduledJob => scheduledJob.ToResponse())];
        await _scheduledJobNotifier.SendScheduledJobsAsync(scheduledJobResponses, cancellationToken).ConfigureAwait(false);
    }
}
