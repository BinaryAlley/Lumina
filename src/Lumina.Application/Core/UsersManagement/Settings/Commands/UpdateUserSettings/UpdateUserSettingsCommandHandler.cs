#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.UsersManagement.Users;
using Lumina.Domain.Common.Primitives;
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

        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return Errors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // Get the existing settings of the current user, if any.
        Result<UserSettingsEntity?> getSettingsResult = await _unitOfWork.UserSettingsRepository.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (getSettingsResult.IsFailure)
            return getSettingsResult.Errors;

        if (getSettingsResult.Value is null)
        {
            // Create the settings of the current user, since they are not stored yet. The playback state the player reports automatically is left
            // at its default, because this command only carries the settings the user edits by hand.
            Result<UserSettings> createSettingsResult = UserSettings.Create(UserId.Create(userId));
            if (createSettingsResult.IsFailure)
                return createSettingsResult.Errors;

            Result<Updated> applySettingsResult = ApplySettings(createSettingsResult.Value, command);
            if (applySettingsResult.IsFailure)
                return applySettingsResult.Errors;

            Result<Created> insertResult = await _unitOfWork.UserSettingsRepository.InsertAsync(createSettingsResult.Value.ToRepositoryEntity(), cancellationToken).ConfigureAwait(false);
            if (insertResult.IsFailure)
                return insertResult.Errors;
        }
        else
        {
            // Update the stored settings of the current user.
            Result<UserSettings> toDomainEntityResult = getSettingsResult.Value.ToDomainEntity();
            if (toDomainEntityResult.IsFailure)
                return toDomainEntityResult.Errors;

            Result<Updated> updateSettingsResult = ApplySettings(toDomainEntityResult.Value, command);
            if (updateSettingsResult.IsFailure)
                return updateSettingsResult.Errors;

            Result<Updated> updateResult = await _unitOfWork.UserSettingsRepository.UpdateAsync(toDomainEntityResult.Value.ToRepositoryEntity(), cancellationToken).ConfigureAwait(false);
            if (updateResult.IsFailure)
                return updateResult.Errors;
        }

        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Errors;

        return Result.Updated;
    }

    /// <summary>
    /// Applies the values carried by <paramref name="command"/> to the settings of the current user.
    /// </summary>
    /// <param name="settings">The settings of the current user to update.</param>
    /// <param name="command">The command carrying the new values of the settings.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful update, or an error.</returns>
    private static Result<Updated> ApplySettings(UserSettings settings, UpdateUserSettingsCommand command)
    {
        return settings.UpdateSettings(
            command.IsPaginationEnabled,
            command.ItemsPerPage,
            command.ShouldIgnoreThePrefixForAlphaPicker,
            command.IsThemeCachingEnabled,
            command.ShouldAggregateMetadataWhenMissing,
            command.ShouldAggregateArtworkWhenMissing,
            command.ShouldRenderPdfAsImages,
            command.ShouldPreserveBookStyles,
            command.ShouldKeepDownloadingWhilePaused!.Value,
            command.ShouldAutoplay!.Value,
            command.IsGaplessPlaybackEnabled!.Value,
            command.ReplayGainMode!.Value,
            command.StreamingQuality!.Value);
    }
}
