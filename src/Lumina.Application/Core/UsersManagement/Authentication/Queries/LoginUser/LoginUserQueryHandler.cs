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

namespace Lumina.Application.Core.UsersManagement.Authentication.Queries.LoginUser;

/// <summary>
/// Handler for the query to authenticate an account.
/// </summary>
public class LoginUserQueryHandler : IQueryHandler<LoginUserQuery, Result<LoginResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHashService _hashService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ITotpTokenGenerator _totpTokenGenerator;
    private readonly ICryptographyService _cryptographyService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IValidator<LoginUserQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginUserQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="hashService">Injected service for password hashing functionality.</param>
    /// <param name="jwtTokenGenerator">Injected service for generating JWT tokens.</param>
    /// <param name="totpTokenGenerator">Injected service for generating and validating TOTP tokens.</param>
    /// <param name="cryptographyService">Injected service for cryptographic functionality.</param>
    /// <param name="dateTimeProvider">Injected service for time related concerns.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public LoginUserQueryHandler(
        IUnitOfWork unitOfWork, 
        IPasswordHashService hashService, 
        IJwtTokenGenerator jwtTokenGenerator, 
        ITotpTokenGenerator totpTokenGenerator, 
        ICryptographyService cryptographyService,
        IDateTimeProvider dateTimeProvider,
        IValidator<LoginUserQuery> validator)
    {
        _unitOfWork = unitOfWork;
        _hashService = hashService;
        _jwtTokenGenerator = jwtTokenGenerator;
        _totpTokenGenerator = totpTokenGenerator;
        _cryptographyService = cryptographyService;
        _dateTimeProvider = dateTimeProvider;
        _validator = validator;
    }

    /// <summary>
    /// Handles the query to authenticate an account.
    /// </summary>
    /// <param name="query">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="LoginResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<LoginResponse>> HandleAsync(LoginUserQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return validationResult;

        // Load the account by its username, so the password and two factor state can be verified.
        Result<UserEntity?> getUserResult = await _unitOfWork.UserRepository.GetByUsernameAsync(query.Username!, cancellationToken).ConfigureAwait(false);
        if (getUserResult.IsFailure)
            return getUserResult.Errors;
        else if (getUserResult.Value is null)
            return Errors.Authentication.InvalidUsernameOrPassword;

        // Validate that the password matches the stored hash.
        if (!_hashService.CheckStringAgainstHash(query.Password!, Uri.UnescapeDataString(getUserResult.Value.Password!)))
        {
            // If a password reset was requested, a temporary password is in use; when the check against the regular password failed, try the temporary one too.
            if (getUserResult.Value.TempPassword is not null)
            {
                // The temporary password is only valid for 15 minutes; when it is obsolete, remove it and return an error.
                if (getUserResult.Value.TempPasswordCreated!.Value.AddMinutes(15) < _dateTimeProvider.UtcNow)
                {
                    getUserResult.Value.TempPassword = null;
                    getUserResult.Value.TempPasswordCreated = null;
                    await _unitOfWork.UserRepository.UpdateAsync(getUserResult.Value, cancellationToken);
                    Result<Success> saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    if (saveChangesResult.IsFailure)
                        return saveChangesResult.Errors;
                    return Errors.Authentication.TempPasswordExpired;
                }
                else // The temporary password is still valid, so validate the password against it.
                {
                    if (!_hashService.CheckStringAgainstHash(query.Password!, Uri.UnescapeDataString(getUserResult.Value.TempPassword!)))
                        return Errors.Authentication.InvalidUsernameOrPassword;
                }
            }
            else
                return Errors.Authentication.InvalidUsernameOrPassword;
        }

        // Create the JWT token.
        string token = _jwtTokenGenerator.GenerateToken(getUserResult.Value.Id.ToString(), getUserResult.Value.Username);

        // Check if the user uses TOTP.
        bool doesUseTotp = !string.IsNullOrEmpty(getUserResult.Value.TotpSecret);
        if (doesUseTotp && string.IsNullOrEmpty(query.TotpCode))
            return Errors.Authentication.InvalidTotpCode;
        else if (doesUseTotp && !string.IsNullOrEmpty(query.TotpCode)) // When the user uses TOTP, validate the provided code.
            if (!_totpTokenGenerator.ValidateToken(Convert.FromBase64String(_cryptographyService.Decrypt(getUserResult.Value.TotpSecret!)), query.TotpCode))
                return Errors.Authentication.InvalidTotpCode;
        return new LoginResponse(getUserResult.Value.Id, getUserResult.Value.Username, token, doesUseTotp);
    }
}
