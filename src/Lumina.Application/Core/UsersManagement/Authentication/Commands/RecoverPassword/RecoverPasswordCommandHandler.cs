#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Security;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Contracts.Responses.Authentication;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.UsersManagement.Authentication.Commands.RecoverPassword;

/// <summary>
/// Handler for the command to recover the password of an account.
/// </summary>
public class RecoverPasswordCommandHandler : ICommandHandler<RecoverPasswordCommand, Result<RecoverPasswordResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHashService _hashService;
    private readonly ITotpTokenGenerator _totpTokenGenerator;
    private readonly ICryptographyService _cryptographyService;
    private readonly IValidator<RecoverPasswordCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecoverPasswordCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="hashService">Injected service for password hashing functionality.</param>
    /// <param name="totpTokenGenerator">Injected service for generating and validating TOTP tokens.</param>
    /// <param name="cryptographyService">Injected service for cryptographic functionality.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public RecoverPasswordCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHashService hashService,
        ITotpTokenGenerator totpTokenGenerator,
        ICryptographyService cryptographyService,
        IValidator<RecoverPasswordCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _hashService = hashService;
        _totpTokenGenerator = totpTokenGenerator;
        _cryptographyService = cryptographyService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to recover the password of an account.
    /// </summary>
    /// <param name="command">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="RecoverPasswordResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<RecoverPasswordResponse>> HandleAsync(RecoverPasswordCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        // Load the account by its username, so its existing recovery state and TOTP configuration can be checked.
        Result<UserEntity?> getUserResult = await _unitOfWork.UserRepository.GetByUsernameAsync(command.Username!, cancellationToken).ConfigureAwait(false);
        if (getUserResult.IsFailure)
            return getUserResult.Errors;
        else if (getUserResult.Value is null)
            return Errors.Authentication.UsernameDoesNotExist;
        else if (getUserResult.Value.TempPassword is not null) // If a temporary password is present, a password reset was already requested.
            return Errors.Authentication.PasswordResetAlreadyRequested;

        // Check if the user uses TOTP.
        bool doesUseTotp = !string.IsNullOrEmpty(getUserResult.Value.TotpSecret);
        if (!doesUseTotp)
            return Errors.Authentication.InvalidTotpCode;
        if (doesUseTotp && string.IsNullOrEmpty(command.TotpCode))
            return Errors.Authentication.InvalidTotpCode;
        else if (doesUseTotp && !string.IsNullOrEmpty(command.TotpCode)) // When the user uses TOTP, validate the provided code before allowing the reset.
            if (!_totpTokenGenerator.ValidateToken(Convert.FromBase64String(_cryptographyService.Decrypt(getUserResult.Value.TotpSecret!)), command.TotpCode))
                return Errors.Authentication.InvalidTotpCode;

        // Generate the hash of a temporary password and record when it was created, so that the reset flow can expire it after 15 minutes.
        getUserResult.Value.TempPassword = Uri.EscapeDataString(_hashService.HashString("Abcd123$")); // TODO: Replace the hardcoded password with a randomly generated one.
        getUserResult.Value.TempPasswordCreated = DateTime.UtcNow;

        // Update the user and persist the temporary password.
        Result<Updated> updateUserResult = await _unitOfWork.UserRepository.UpdateAsync(getUserResult.Value, cancellationToken).ConfigureAwait(false);
        if (updateUserResult.IsFailure)
            return updateUserResult.Errors;

        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Errors;
        return new RecoverPasswordResponse(true);
    }
}
