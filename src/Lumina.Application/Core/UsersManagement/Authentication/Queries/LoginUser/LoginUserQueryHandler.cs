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

namespace Lumina.Application.Core.UsersManagement.Authentication.Queries.LoginUser;

/// <summary>
/// Handler for the query to authenticate an account.
/// </summary>
public class LoginUserQueryHandler : IRequestHandler<LoginUserQuery, ErrorOr<LoginResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHashService _hashService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ITotpTokenGenerator _totpTokenGenerator;
    private readonly ICryptographyService _cryptographyService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginUserQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="hashService">Injected service for password hashing functionality.</param>
    /// <param name="jwtTokenGenerator">Injected service for generating JWT tokens.</param>
    /// <param name="totpTokenGenerator">Injected service for generating and validating TOTP tokens.</param>
    /// <param name="cryptographyService">Injected service for cryptographic functionality.</param>
    public LoginUserQueryHandler(
        IUnitOfWork unitOfWork, 
        IHashService hashService, 
        IJwtTokenGenerator jwtTokenGenerator, 
        ITotpTokenGenerator totpTokenGenerator, 
        ICryptographyService cryptographyService)
    {
        _unitOfWork = unitOfWork;
        _hashService = hashService;
        _jwtTokenGenerator = jwtTokenGenerator;
        _totpTokenGenerator = totpTokenGenerator;
        _cryptographyService = cryptographyService;
    }

    /// <summary>
    /// Handles the query to authenticate an account.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully created <see cref="LoginResponse"/>, or an error message.
    /// </returns>
    public async ValueTask<ErrorOr<LoginResponse>> Handle(LoginUserQuery request, CancellationToken cancellationToken)
    {
        // check if any users already exists
        IUserRepository userRepository = _unitOfWork.GetRepository<IUserRepository>();
        ErrorOr<UserEntity?> resultSelectUser = await userRepository.GetByUsernameAsync(request.Username!, cancellationToken);
        if (resultSelectUser.IsError)
            return resultSelectUser.Errors;
        else if (resultSelectUser.Value is null)
            return Errors.Authentication.InvalidUsernameOrPassword;
        // validate that the password is correct
        if (!_hashService.CheckStringAgainstHash(request.Password!, Uri.UnescapeDataString(resultSelectUser.Value.Password!)))
        {
            // if a password reset was requested, a temp password is being used - if the hash check failed against the regular password, try against the temp one too
            if (resultSelectUser.Value.TempPassword is not null)
            {
                // the temporary password should only be valid for 15 minutes, if its obsolete, remove it and return error
                if (resultSelectUser.Value.TempPasswordCreated!.Value.AddMinutes(15) < DateTime.UtcNow)
                {
                    resultSelectUser.Value.TempPassword = null;
                    resultSelectUser.Value.TempPasswordCreated = null;
                    await userRepository.UpdateAsync(resultSelectUser.Value, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    return Errors.Authentication.TempPasswordExpired;
                }
                else // temporary password is still valid, validate password against it
                {
                    if (!_hashService.CheckStringAgainstHash(request.Password!, Uri.UnescapeDataString(resultSelectUser.Value.TempPassword!)))
                        return Errors.Authentication.InvalidUsernameOrPassword;
                }
            }
            else
                return Errors.Authentication.InvalidUsernameOrPassword;
        }
        // create the JWT token
        string token = _jwtTokenGenerator.GenerateToken(resultSelectUser.Value.Id.ToString(), resultSelectUser.Value.Username);
        // check if the user uses TOTP
        bool usesTotp = !string.IsNullOrEmpty(resultSelectUser.Value.TotpSecret);
        if (usesTotp && string.IsNullOrEmpty(request.TotpCode)) 
            return Errors.Authentication.InvalidTotpCode;
        else if (usesTotp && !string.IsNullOrEmpty(request.TotpCode)) // and if they do, validate it
            if (!_totpTokenGenerator.ValidateToken(Convert.FromBase64String(_cryptographyService.Decrypt(resultSelectUser.Value.TotpSecret!)), request.TotpCode))
                return Errors.Authentication.InvalidTotpCode;
        return new LoginResponse(resultSelectUser.Value.Id, resultSelectUser.Value.Username, token, usesTotp);
    }
}
