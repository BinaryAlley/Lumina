#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Security;
using Lumina.Contracts.Responses.Authentication;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.UsersManagement.Authentication.Commands.ChangePassword;

/// <summary>
/// Handler for the command to change the password of a user account.
/// </summary>
public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ErrorOr<ChangePasswordResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHashService _hashService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangePasswordCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="hashService">Injected service for password hashing functionality.</param>
    public ChangePasswordCommandHandler(
        IUnitOfWork unitOfWork,
        IHashService hashService)
    {
        _unitOfWork = unitOfWork;
        _hashService = hashService;
    }

    /// <summary>
    /// Handles the command to change the password of an account.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully created <see cref="ChangePasswordResponse"/>, or an error message.
    /// </returns>
    public async ValueTask<ErrorOr<ChangePasswordResponse>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        IUserRepository userRepository = _unitOfWork.GetRepository<IUserRepository>();
        ErrorOr<UserEntity?> getUserResult = await userRepository.GetByUsernameAsync(request.Username!, cancellationToken).ConfigureAwait(false);
        if (getUserResult.IsError)
            return getUserResult.Errors;
        else if (getUserResult.Value is null)
            return Errors.Authentication.UsernameDoesNotExist;
        // validate if the current password is correct
        if (!_hashService.CheckStringAgainstHash(request.CurrentPassword!, Uri.UnescapeDataString(getUserResult.Value.Password!)))
            return Errors.Authentication.InvalidCurrentPassword;
        getUserResult.Value.Password = Uri.EscapeDataString(_hashService.HashString(request.NewPassword!));
        // if the password change was initiated via a password reset, remote the temporary password that was generated in the process
        getUserResult.Value.TempPassword = null;
        getUserResult.Value.TempPasswordCreated = null;
        // update the user
        ErrorOr<Updated> updateUserResult = await userRepository.UpdateAsync(getUserResult.Value, cancellationToken).ConfigureAwait(false);
        if (updateUserResult.IsError)
            return updateUserResult.Errors;
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new ChangePasswordResponse(true);
    }
}
