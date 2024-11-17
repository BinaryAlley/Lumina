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

namespace Lumina.Application.Core.UsersManagement.Authentication.Commands.RegisterUser;

/// <summary>
/// Handler for the command to register a new user account.
/// </summary>
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, ErrorOr<RegistrationResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHashService _hashService;
    private readonly ICryptographyService _cryptographyService;
    private readonly ITotpTokenGenerator _totpTokenGenerator;
    private readonly IQRCodeGenerator _qRCodeGenerator;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterUserCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="hashService">Injected service for password hashing functionality.</param>
    /// <param name="cryptographyService">Injected service for cryptographic functionality.</param>
    /// <param name="totpTokenGenerator">Injected service for generating and validating TOTP tokens.</param>
    /// <param name="qRCodeGenerator">Injected service for generating QR codes.</param>
    public RegisterUserCommandHandler(
        IUnitOfWork unitOfWork,
        IHashService hashService,
        ICryptographyService cryptographyService,
        ITotpTokenGenerator totpTokenGenerator,
        IQRCodeGenerator qRCodeGenerator)
    {
        _unitOfWork = unitOfWork;
        _hashService = hashService;
        _cryptographyService = cryptographyService;
        _totpTokenGenerator = totpTokenGenerator;
        _qRCodeGenerator = qRCodeGenerator;
    }

    /// <summary>
    /// Handles the command to register an account.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully created <see cref="RegistrationResponse"/>, or an error message.
    /// </returns>
    public async ValueTask<ErrorOr<RegistrationResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // check if any users already exists (admin account is only set once!)
        IUserRepository userRepository = _unitOfWork.GetRepository<IUserRepository>();
        ErrorOr<UserEntity?> resultSelectUser = await userRepository.GetByUsernameAsync(request.Username!, cancellationToken).ConfigureAwait(false);
        if (resultSelectUser.IsError)
            return resultSelectUser.Errors;
        else if (resultSelectUser.Value is not null)
            return Errors.Authentication.UsernameAlreadyExists;
        string? totpSecret = null;
        UserEntity user = new()
        {
            Id = Guid.NewGuid(),
            Username = request.Username!,
            Password = Uri.EscapeDataString(_hashService.HashString(request.Password!)),
            Created = DateTime.UtcNow,
            Libraries = []
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
        ErrorOr<Created> resultInsertUser = await userRepository.InsertAsync(user, cancellationToken).ConfigureAwait(false);
        if (resultInsertUser.IsError)
            return resultInsertUser.Errors;
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        // TODO: insert the default admin profile preferences when they are implemented
        // if 2FA was enabled, the TOTP secret needs to be delivered to the client unhashed, so it can be displayed 
        return new RegistrationResponse(user.Id, user.Username, totpSecret);
    }
}
