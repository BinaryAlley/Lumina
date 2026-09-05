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
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Scheduling.Commands.FireScheduledJob;

/// <summary>
/// Handler for the command to fire the task of a scheduled job once.
/// </summary>
public class FireScheduledJobCommandHandler : ICommandHandler<FireScheduledJobCommand, Result<ScheduledJobResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDomainEventsQueue _domainEventsQueue;
    private readonly IValidator<FireScheduledJobCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="FireScheduledJobCommandHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="domainEventsQueue">Injected service for the queue of domain events.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public FireScheduledJobCommandHandler(
        IAuthorizationService authorizationService,
        ICurrentUserService currentUserService,
        IDomainEventsQueue domainEventsQueue,
        IUnitOfWork unitOfWork,
        IValidator<FireScheduledJobCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _domainEventsQueue = domainEventsQueue;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to fire the task of a scheduled job once.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a <see cref="ScheduledJobResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<ScheduledJobResponse>> HandleAsync(FireScheduledJobCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // Only administrators can fire scheduled jobs.
        if (!await _authorizationService.IsInRoleAsync(userId, "Admin", cancellationToken).ConfigureAwait(false))
            return ApplicationErrors.Authorization.NotAuthorized;

        // Get the scheduled job from the storage medium.
        Result<ScheduledJobEntity?> getScheduledJobResult = await _unitOfWork.ScheduledJobRepository.GetByIdAsync(command.ScheduledJobId, cancellationToken).ConfigureAwait(false);
        if (getScheduledJobResult.IsFailure)
            return getScheduledJobResult.Errors;
        if (getScheduledJobResult.Value is null)
            return DomainErrors.Scheduling.ScheduledJobNotFound;

        // Convert the repository entity to a domain entity, and guard the fire against a currently running task.
        Result<ScheduledJob> scheduledJobDomainResult = getScheduledJobResult.Value.ToDomainEntity();
        if (scheduledJobDomainResult.IsFailure)
            return scheduledJobDomainResult.Errors;
        Result<Success> fireResult = scheduledJobDomainResult.Value.Fire();
        if (fireResult.IsFailure)
            return fireResult.Errors;

        // Queue the domain event that requests a one time execution of the task of the scheduled job.
        foreach (IDomainEvent domainEvent in scheduledJobDomainResult.Value.GetDomainEvents())
            _domainEventsQueue.Enqueue(domainEvent);

        return scheduledJobDomainResult.Value.ToResponse();
    }
}
