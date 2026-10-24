#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.ConsumptionActivity;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaConsumption.Commands.ClearConsumptionHistory;

/// <summary>
/// Handler for the command to delete every consumption record of a user.
/// </summary>
public class ClearConsumptionHistoryCommandHandler : ICommandHandler<ClearConsumptionHistoryCommand, Result<Deleted>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IValidator<ClearConsumptionHistoryCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClearConsumptionHistoryCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public ClearConsumptionHistoryCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        IValidator<ClearConsumptionHistoryCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to delete every consumption record of a user.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful deletion, or an error message.</returns>
    public async Task<Result<Deleted>> HandleAsync(ClearConsumptionHistoryCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // The target defaults to the current user, and the access policy is the only place that decides whether the caller may act on it.
        Guid targetUserId = command.TargetUserId is null ? userId : Guid.Parse(command.TargetUserId);
        bool canAccessTarget = await _authorizationService.EvaluatePolicyAsync<IConsumptionActivityAccessPolicy>(
            userId, new ConsumptionActivityAccessPolicyContext(targetUserId), cancellationToken).ConfigureAwait(false);
        if (!canAccessTarget)
            return ApplicationErrors.Authorization.NotAuthorized;

        // Clearing the history does not clear the resume points: the two are separate user actions, so the continue watching list survives.
        Result<Deleted> deleteResult = await _unitOfWork.MediaConsumptionRecordRepository.DeleteAllForUserAsync(targetUserId, cancellationToken).ConfigureAwait(false);
        if (deleteResult.IsFailure)
            return deleteResult.Errors;

        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Errors;

        return Result.Deleted;
    }
}
