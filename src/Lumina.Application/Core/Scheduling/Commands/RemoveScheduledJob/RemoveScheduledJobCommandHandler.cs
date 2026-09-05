#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Scheduling;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.Scheduling;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Scheduling.Commands.RemoveScheduledJob;

/// <summary>
/// Handler for the command to remove a scheduled job.
/// </summary>
public class RemoveScheduledJobCommandHandler : ICommandHandler<RemoveScheduledJobCommand, Result<Success>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDomainEventsQueue _domainEventsQueue;
    private readonly IScheduledJobScheduler _scheduledJobScheduler;
    private readonly IValidator<RemoveScheduledJobCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveScheduledJobCommandHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="domainEventsQueue">Injected service for the queue of domain events.</param>
    /// <param name="scheduledJobScheduler">Injected service that schedules and executes the tasks of scheduled jobs.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public RemoveScheduledJobCommandHandler(
        IAuthorizationService authorizationService,
        ICurrentUserService currentUserService,
        IDomainEventsQueue domainEventsQueue,
        IScheduledJobScheduler scheduledJobScheduler,
        IUnitOfWork unitOfWork,
        IValidator<RemoveScheduledJobCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _domainEventsQueue = domainEventsQueue;
        _scheduledJobScheduler = scheduledJobScheduler;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to remove a scheduled job.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> representing either a successful operation, or an error.
    /// </returns>
    public async Task<Result<Success>> HandleAsync(RemoveScheduledJobCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // Only administrators can remove scheduled jobs.
        if (!await _authorizationService.IsInRoleAsync(userId, "Admin", cancellationToken).ConfigureAwait(false))
            return ApplicationErrors.Authorization.NotAuthorized;

        // Get the scheduled job from the storage medium.
        Result<ScheduledJobEntity?> getScheduledJobResult = await _unitOfWork.ScheduledJobRepository.GetByIdAsync(command.ScheduledJobId, cancellationToken).ConfigureAwait(false);
        if (getScheduledJobResult.IsFailure)
            return getScheduledJobResult.Errors;
        if (getScheduledJobResult.Value is null)
            return DomainErrors.Scheduling.ScheduledJobNotFound;

        // Stop the execution cycle of the scheduled job, if it has one.
        Result<ScheduledJob> scheduledJobDomainResult = getScheduledJobResult.Value.ToDomainEntity();
        if (scheduledJobDomainResult.IsFailure)
            return scheduledJobDomainResult.Errors;
        await _scheduledJobScheduler.StopCycleAsync(scheduledJobDomainResult.Value.Id, cancellationToken).ConfigureAwait(false);

        // Remove the executions and the scheduled job itself from the storage medium.
        Result<Success> deleteExecutionsResult = await _unitOfWork.ScheduledJobExecutionRepository.DeleteByScheduledJobIdAsync(command.ScheduledJobId, cancellationToken).ConfigureAwait(false);
        if (deleteExecutionsResult.IsFailure)
            return deleteExecutionsResult.Errors;
        Result<Deleted> deleteScheduledJobResult = await _unitOfWork.ScheduledJobRepository.DeleteByIdAsync(command.ScheduledJobId, cancellationToken).ConfigureAwait(false);
        if (deleteScheduledJobResult.IsFailure)
            return deleteScheduledJobResult.Errors;
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Queue any domain events.
        scheduledJobDomainResult.Value.Remove();
        foreach (IDomainEvent domainEvent in scheduledJobDomainResult.Value.GetDomainEvents())
            _domainEventsQueue.Enqueue(domainEvent);

        return Result.Success;
    }
}
