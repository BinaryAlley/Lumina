#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.Common.Base;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Domain.Common.Primitives;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.Authorization.Policies.LibraryOwnership;

/// <summary>
/// Authorization policy that checks whether a user can access a media library: admins can access any library, while other users can only access the libraries they own.
/// </summary>
public class LibraryOwnershipPolicy : ILibraryOwnershipPolicy
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryOwnershipPolicy"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public LibraryOwnershipPolicy(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Evaluates the policy for the user identified by <paramref name="userId"/> against the media library identified in <paramref name="context"/>.
    /// </summary>
    /// <param name="userId">The unique identifier of the user for which to evaluate the policy.</param>
    /// <param name="context">The context carrying the Id of the media library whose access is being evaluated.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns><see langword="true"/> if the user is an admin or owns the media library, <see langword="false"/> otherwise.</returns>
    public async Task<bool> EvaluateAsync(Guid userId, PolicyContext? context, CancellationToken cancellationToken)
    {
        if (context is not LibraryOwnershipPolicyContext libraryContext)
            return false;

        // admins can access any media library
        Result<UserEntity?> getUserResult = await _unitOfWork.UserRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (getUserResult.IsFailure || getUserResult.Value is null)
            return false;
        if (getUserResult.Value.UserRole?.Role.RoleName == "Admin")
            return true;

        // regular users can only access the media libraries they own
        Result<LibraryEntity?> getLibraryResult = await _unitOfWork.LibraryRepository.GetByIdAsync(libraryContext.LibraryId, cancellationToken).ConfigureAwait(false);
        if (getLibraryResult.IsFailure || getLibraryResult.Value is null)
            return false;
        return getLibraryResult.Value.UserId == userId;
    }
}
