#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Security;
using Lumina.Application.Common.Infrastructure.Time;
using Lumina.Contracts.Responses.Authentication;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.UsersManagement.Authentication.Commands.RegisterUser;

/// <summary>
/// Handler for the command to register a new user account.
/// </summary>
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, ErrorOr<RegistrationResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHashService _hashService;
    private readonly ICryptographyService _cryptographyService;
    private readonly ITotpTokenGenerator _totpTokenGenerator;
    private readonly IQRCodeGenerator _qRCodeGenerator;
    private readonly IDateTimeProvider _dateTimeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterUserCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="hashService">Injected service for password hashing functionality.</param>
    /// <param name="cryptographyService">Injected service for cryptographic functionality.</param>
    /// <param name="totpTokenGenerator">Injected service for generating and validating TOTP tokens.</param>
    /// <param name="qRCodeGenerator">Injected service for generating QR codes.</param>
    /// <param name="dateTimeProvider">Injected service for time related concerns.</param>
    public RegisterUserCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHashService hashService,
        ICryptographyService cryptographyService,
        ITotpTokenGenerator totpTokenGenerator,
        IQRCodeGenerator qRCodeGenerator,
        IDateTimeProvider dateTimeProvider)
    {
        _unitOfWork = unitOfWork;
        _hashService = hashService;
        _cryptographyService = cryptographyService;
        _totpTokenGenerator = totpTokenGenerator;
        _qRCodeGenerator = qRCodeGenerator;
        _dateTimeProvider = dateTimeProvider;
    }

    /// <summary>
    /// Handles the command to register an account.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfuly created <see cref="RegistrationResponse"/>, or an error message.
    /// </returns>
    public async ValueTask<ErrorOr<RegistrationResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // check if any users already exists (admin account is only set once!)
        IUserRepository userRepository = _unitOfWork.GetRepository<IUserRepository>();
        ErrorOr<UserEntity?> getUserResult = await userRepository.GetByUsernameAsync(request.Username!, cancellationToken).ConfigureAwait(false);
        if (getUserResult.IsError)
            return getUserResult.Errors;
        else if (getUserResult.Value is not null)
            return Errors.Authentication.UsernameAlreadyExists;
        string? totpSecret = null;
        Guid id = Guid.NewGuid();
        UserEntity user = new()
        {
            Id = id,
            Username = request.Username!,
            Password = Uri.EscapeDataString(_hashService.HashString(request.Password!)),
            CreatedOnUtc = _dateTimeProvider.UtcNow,
            Libraries = [],
            UserPermissions = [],
            UserRole = null,
            CreatedBy = id,
            LibraryScans = [],
        };
        // if the user enabled two factor auth, include a QR with the totp secret
        if (request.Use2fa)
        {
            // generate a TOTP secret
            byte[] secret = _totpTokenGenerator.GenerateSecret();
            // convert the secret into a QR code for the user to scan
            totpSecret = _qRCodeGenerator.GenerateQrCodeDataUri(request.Username!, secret);
            // store the TOTP secret in the repository, encrypted
            user.TotpSecret = _cryptographyService.Encrypt(Convert.ToBase64String(secret));
        }
        // insert the user
        ErrorOr<Created> insertUserResult = await userRepository.InsertAsync(user, cancellationToken).ConfigureAwait(false);
        if (insertUserResult.IsError)
            return insertUserResult.Errors;
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        // TODO: insert the default admin profile preferences when they are implemented
        // if 2FA was enabled, the TOTP secret needs to be delivered to the client unhashed, so it can be displayed 
        return new RegistrationResponse(user.Id, user.Username, totpSecret);
    }
}
