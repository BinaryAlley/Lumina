#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Security;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Contracts.Responses.Authentication;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.UsersManagement.Authentication.Commands.RegisterUser;

/// <summary>
/// Handler for the command to register a new user account.
/// </summary>
public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, Result<RegistrationResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHashService _hashService;
    private readonly ICryptographyService _cryptographyService;
    private readonly ITotpTokenGenerator _totpTokenGenerator;
    private readonly IQRCodeGenerator _qRCodeGenerator;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IValidator<RegisterUserCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterUserCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="hashService">Injected service for password hashing functionality.</param>
    /// <param name="cryptographyService">Injected service for cryptographic functionality.</param>
    /// <param name="totpTokenGenerator">Injected service for generating and validating TOTP tokens.</param>
    /// <param name="qRCodeGenerator">Injected service for generating QR codes.</param>
    /// <param name="dateTimeProvider">Injected service for time related concerns.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public RegisterUserCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHashService hashService,
        ICryptographyService cryptographyService,
        ITotpTokenGenerator totpTokenGenerator,
        IQRCodeGenerator qRCodeGenerator,
        IDateTimeProvider dateTimeProvider,
        IValidator<RegisterUserCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _hashService = hashService;
        _cryptographyService = cryptographyService;
        _totpTokenGenerator = totpTokenGenerator;
        _qRCodeGenerator = qRCodeGenerator;
        _dateTimeProvider = dateTimeProvider;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to register an account.
    /// </summary>
    /// <param name="command">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="RegistrationResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<RegistrationResponse>> HandleAsync(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        // Check if a user with this username already exists, since the admin account can only be created once.
        Result<UserEntity?> getUserResult = await _unitOfWork.UserRepository.GetByUsernameAsync(command.Username!, cancellationToken).ConfigureAwait(false);
        if (getUserResult.IsFailure)
            return getUserResult.Errors;
        else if (getUserResult.Value is not null)
            return Errors.Authentication.UsernameAlreadyExists;

        // Build the account, hashing the password and URI escaping it the same way as the stored form.
        string? totpSecret = null;
        Guid id = Guid.NewGuid();
        UserEntity user = new()
        {
            Id = id,
            Username = command.Username!,
            Password = Uri.EscapeDataString(_hashService.HashString(command.Password!)),
            CreatedOnUtc = _dateTimeProvider.UtcNow,
            Libraries = [],
            UserPermissions = [],
            UserRole = null,
            CreatedBy = id,
            LibraryScans = [],
        };

        // If the user enabled two factor authentication, include a QR code with the TOTP secret.
        if (command.Use2fa)
        {
            // Generate a TOTP secret.
            byte[] secret = _totpTokenGenerator.GenerateSecret();
            // Convert the secret into a QR code for the user to scan.
            totpSecret = _qRCodeGenerator.GenerateQrCodeDataUri(command.Username!, secret);
            // Store the TOTP secret in the repository, encrypted.
            user.TotpSecret = _cryptographyService.Encrypt(Convert.ToBase64String(secret));
        }

        // Insert the user and persist the change.
        Result<Created> insertUserResult = await _unitOfWork.UserRepository.InsertAsync(user, cancellationToken).ConfigureAwait(false);
        if (insertUserResult.IsFailure)
            return insertUserResult.Errors;

        Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Errors;

        // TODO: Insert the default admin profile preferences when they are implemented.
        // If 2FA was enabled, the TOTP secret needs to be delivered to the client unencrypted, so it can be displayed.
        return new RegistrationResponse(user.Id, user.Username, totpSecret);
    }
}
