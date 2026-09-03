#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.Scheduling;
using Lumina.Contracts.Responses.Scheduling;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Scheduling;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Scheduling.Commands.AddScheduledJob;

/// <summary>
/// Handler for the command to add a scheduled job.
/// </summary>
public class AddScheduledJobCommandHandler : ICommandHandler<AddScheduledJobCommand, Result<ScheduledJobResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDomainEventsQueue _domainEventsQueue;
    private readonly IValidator<AddScheduledJobCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddScheduledJobCommandHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="domainEventsQueue">Injected service for the queue of domain events.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public AddScheduledJobCommandHandler(
        IAuthorizationService authorizationService,
        ICurrentUserService currentUserService,
        IDomainEventsQueue domainEventsQueue,
        IUnitOfWork unitOfWork,
        IValidator<AddScheduledJobCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _domainEventsQueue = domainEventsQueue;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to add a scheduled job.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="ScheduledJobResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<ScheduledJobResponse>> HandleAsync(AddScheduledJobCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // Only administrators can schedule jobs.
        if (!await _authorizationService.IsInRoleAsync(userId, "Admin", cancellationToken).ConfigureAwait(false))
            return ApplicationErrors.Authorization.NotAuthorized;

        // Create the schedule of the scheduled job, based on its type.
        Result<Schedule> scheduleResult;
        if (command.ScheduleType == ScheduleType.WithIntervalInMinutes)
        {
            Result<IntervalSchedule> intervalScheduleResult = IntervalSchedule.Create(command.IntervalMinutes ?? 0);
            if (intervalScheduleResult.IsFailure)
                return intervalScheduleResult.Errors;
            scheduleResult = Result.From<Schedule>(intervalScheduleResult.Value);
        }
        else if (command.ScheduleType == ScheduleType.DailyAtHourAndMinute)
        {
            Result<DailySchedule> dailyScheduleResult = DailySchedule.Create(command.Hour ?? 0, command.Minute ?? 0);
            if (dailyScheduleResult.IsFailure)
                return dailyScheduleResult.Errors;
            scheduleResult = Result.From<Schedule>(dailyScheduleResult.Value);
        }
        else
            return DomainErrors.Scheduling.InvalidScheduleType;

        // Create a domain scheduled job object.
        Result<ScheduledJob> createScheduledJobResult = ScheduledJob.Create(command.Name, command.TaskType, scheduleResult.Value, UserId.Create(userId));
        if (createScheduledJobResult.IsFailure)
            return createScheduledJobResult.Errors;
        createScheduledJobResult.Value.Add();

        // Convert the domain scheduled job to a repository entity, insert it, and save the changes.
        ScheduledJobEntity persistenceScheduledJob = createScheduledJobResult.Value.ToRepositoryEntity();
        Result<Created> insertScheduledJobResult = await _unitOfWork.ScheduledJobRepository.InsertAsync(persistenceScheduledJob, cancellationToken).ConfigureAwait(false);
        if (insertScheduledJobResult.IsFailure)
            return insertScheduledJobResult.Errors;
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Queue any domain events.
        foreach (IDomainEvent domainEvent in createScheduledJobResult.Value.GetDomainEvents())
            _domainEventsQueue.Enqueue(domainEvent);

        return createScheduledJobResult.Value.ToResponse();
    }
}
