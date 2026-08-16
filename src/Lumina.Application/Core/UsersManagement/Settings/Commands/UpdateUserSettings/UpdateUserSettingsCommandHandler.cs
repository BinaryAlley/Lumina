#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.UsersManagement.Users;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserSettingsAggregate;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.UsersManagement.Settings.Commands.UpdateUserSettings;

/// <summary>
/// Handler for the command to update the settings of the current user.
/// </summary>
public class UpdateUserSettingsCommandHandler : ICommandHandler<UpdateUserSettingsCommand, Result<Updated>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<UpdateUserSettingsCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserSettingsCommandHandler"/> class.
    /// </summary>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public UpdateUserSettingsCommandHandler(ICurrentUserService currentUserService, IUnitOfWork unitOfWork, IValidator<UpdateUserSettingsCommand> validator)
    {
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to update the settings of the current user.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> representing either a successful update, or an error message.
    /// </returns>
    public async Task<Result<Updated>> HandleAsync(UpdateUserSettingsCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        // an authenticated request must always carry a user identity
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return Errors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // get the existing settings of the current user, if any
        Result<UserSettingsEntity?> getSettingsResult = await _unitOfWork.UserSettingsRepository.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (getSettingsResult.IsFailure)
            return getSettingsResult.Errors;

        if (getSettingsResult.Value is null)
        {
            // create the settings of the current user, since they are not stored yet
            Result<UserSettings> createSettingsResult = UserSettings.Create(
                UserId.Create(userId),
                command.IsPaginationEnabled,
                command.ItemsPerPage,
                command.IgnoreThePrefixForAlphaPicker);
            if (createSettingsResult.IsFailure)
                return createSettingsResult.Errors;

            Result<Created> insertResult = await _unitOfWork.UserSettingsRepository.InsertAsync(createSettingsResult.Value.ToRepositoryEntity(), cancellationToken).ConfigureAwait(false);
            if (insertResult.IsFailure)
                return insertResult.Errors;
        }
        else
        {
            // update the stored settings of the current user
            Result<UserSettings> toDomainEntityResult = getSettingsResult.Value.ToDomainEntity();
            if (toDomainEntityResult.IsFailure)
                return toDomainEntityResult.Errors;

            Result<Updated> updateSettingsResult = toDomainEntityResult.Value.UpdateSettings(
                command.IsPaginationEnabled,
                command.ItemsPerPage,
                command.IgnoreThePrefixForAlphaPicker);
            if (updateSettingsResult.IsFailure)
                return updateSettingsResult.Errors;

            Result<Updated> updateResult = await _unitOfWork.UserSettingsRepository.UpdateAsync(toDomainEntityResult.Value.ToRepositoryEntity(), cancellationToken).ConfigureAwait(false);
            if (updateResult.IsFailure)
                return updateResult.Errors;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Updated;
    }
}
