#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Scheduling.Commands.UpdateSchedulerDisplayPreferences;

/// <summary>
/// Handler for the command to update the display preferences of the scheduler page of the current user.
/// </summary>
public class UpdateSchedulerDisplayPreferencesCommandHandler : ICommandHandler<UpdateSchedulerDisplayPreferencesCommand, Result<Updated>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdateSchedulerDisplayPreferencesCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateSchedulerDisplayPreferencesCommandHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public UpdateSchedulerDisplayPreferencesCommandHandler(IAuthorizationService authorizationService, ICurrentUserService currentUserService, IUnitOfWork unitOfWork, IValidator<UpdateSchedulerDisplayPreferencesCommand> validator)
    {
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to update the display preferences of the scheduler page of the current user.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> representing either a successful update, or an error.
    /// </returns>
    public async Task<Result<Updated>> HandleAsync(UpdateSchedulerDisplayPreferencesCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // Only administrators can use the scheduler page.
        if (!await _authorizationService.IsInRoleAsync(userId, "Admin", cancellationToken).ConfigureAwait(false))
            return ApplicationErrors.Authorization.NotAuthorized;

        // Upsert the display preferences of the scheduler page of the current user.
        SchedulerDisplayPreferencesEntity displayPreferences = new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            JobTypeFilter = command.JobTypeFilter,
            DisplayTimeSpan = command.DisplayTimeSpan,
            DisplayTimeUnit = command.DisplayTimeUnit,
            CreatedOnUtc = default,
            CreatedBy = default,
            UpdatedOnUtc = null,
            UpdatedBy = null
        };
        Result<Updated> upsertResult = await _unitOfWork.SchedulerDisplayPreferencesRepository.UpsertAsync(displayPreferences, cancellationToken).ConfigureAwait(false);
        if (upsertResult.IsFailure)
            return upsertResult.Errors;
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Updated;
    }
}
