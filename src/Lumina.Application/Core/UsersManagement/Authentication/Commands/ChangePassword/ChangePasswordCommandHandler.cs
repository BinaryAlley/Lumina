#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Security;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Contracts.Responses.Authentication;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.UsersManagement.Authentication.Commands.ChangePassword;

/// <summary>
/// Handler for the command to change the password of a user account.
/// </summary>
public class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand, Result<ChangePasswordResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHashService _hashService;
    private readonly IValidator<ChangePasswordCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangePasswordCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="hashService">Injected service for password hashing functionality.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public ChangePasswordCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHashService hashService,
        IValidator<ChangePasswordCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _hashService = hashService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to change the password of an account.
    /// </summary>
    /// <param name="command">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="ChangePasswordResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<ChangePasswordResponse>> HandleAsync(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        Result<UserEntity?> getUserResult = await _unitOfWork.UserRepository.GetByUsernameAsync(command.Username!, cancellationToken).ConfigureAwait(false);
        if (getUserResult.IsFailure)
            return getUserResult.Errors;
        else if (getUserResult.Value is null)
            return Errors.Authentication.UsernameDoesNotExist;
        // validate if the current password is correct
        if (!_hashService.CheckStringAgainstHash(command.CurrentPassword!, Uri.UnescapeDataString(getUserResult.Value.Password!)))
            return Errors.Authentication.InvalidCurrentPassword;
        getUserResult.Value.Password = Uri.EscapeDataString(_hashService.HashString(command.NewPassword!));
        // if the password change was initiated via a password reset, remote the temporary password that was generated in the process
        getUserResult.Value.TempPassword = null;
        getUserResult.Value.TempPasswordCreated = null;
        // update the user
        Result<Updated> updateUserResult = await _unitOfWork.UserRepository.UpdateAsync(getUserResult.Value, cancellationToken).ConfigureAwait(false);
        if (updateUserResult.IsFailure)
            return updateUserResult.Errors;
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new ChangePasswordResponse(true);
    }
}
