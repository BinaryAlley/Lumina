#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Security;
using Lumina.Contracts.Responses.Authentication;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.UsersManagement.Authentication.Commands.RecoverPassword;

/// <summary>
/// Handler for the command to recover the password of an account.
/// </summary>
public class RecoverPasswordCommandHandler : IRequestHandler<RecoverPasswordCommand, ErrorOr<RecoverPasswordResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHashService _hashService;
    private readonly ITotpTokenGenerator _totpTokenGenerator;
    private readonly ICryptographyService _cryptographyService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecoverPasswordCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="hashService">Injected service for password hashing functionality.</param>
    /// <param name="totpTokenGenerator">Injected service for generating and validating TOTP tokens.</param>
    /// <param name="cryptographyService">Injected service for cryptographic functionality.</param>
    public RecoverPasswordCommandHandler(
        IUnitOfWork unitOfWork,
        IHashService hashService,
        ITotpTokenGenerator totpTokenGenerator,
        ICryptographyService cryptographyService)
    {
        _unitOfWork = unitOfWork;
        _hashService = hashService;
        _totpTokenGenerator = totpTokenGenerator;
        _cryptographyService = cryptographyService;
    }

    /// <summary>
    /// Handles the command to recover the password of an account.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully created <see cref="RecoverPasswordResponse"/>, or an error message.
    /// </returns>
    public async ValueTask<ErrorOr<RecoverPasswordResponse>> Handle(RecoverPasswordCommand request, CancellationToken cancellationToken)
    {
        // check if any users already exists
        IUserRepository userRepository = _unitOfWork.GetRepository<IUserRepository>();
        ErrorOr<UserEntity?> resultSelectUser = await userRepository.GetByUsernameAsync(request.Username!, cancellationToken);
        if (resultSelectUser.IsError)
            return resultSelectUser.Errors;
        else if (resultSelectUser.Value is null)
            return Errors.Authentication.UsernameDoesNotExist;
        else if (resultSelectUser.Value.TempPassword is not null) // if a temp password is present, a password request was already requested
            return Errors.Authentication.PasswordResetAlreadyRequested;
        // check if the user uses TOTP
        bool usesTotp = !string.IsNullOrEmpty(resultSelectUser.Value.TotpSecret);
        if (!usesTotp)
            return Errors.Authentication.InvalidTotpCode;
        if (usesTotp && string.IsNullOrEmpty(request.TotpCode))
            return Errors.Authentication.InvalidTotpCode;
        else if (usesTotp && !string.IsNullOrEmpty(request.TotpCode)) // and if they do, validate it
            if (!_totpTokenGenerator.ValidateToken(Convert.FromBase64String(_cryptographyService.Decrypt(resultSelectUser.Value.TotpSecret!)), request.TotpCode))
                return Errors.Authentication.InvalidTotpCode;
        // hash the new password and assign it to a temporary password that will be valid 15 minutes
        resultSelectUser.Value.TempPassword = Uri.EscapeDataString(_hashService.HashString("Abcd123$")); // TODO: replace with random password generator
        resultSelectUser.Value.TempPasswordCreated = DateTime.UtcNow;
        // update the user
        ErrorOr<Updated> resultUpdateUser = await userRepository.UpdateAsync(resultSelectUser.Value, cancellationToken);
        if (resultUpdateUser.IsError)
            return resultUpdateUser.Errors;
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new RecoverPasswordResponse(true);
    }
}
