#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.Common.Base;
using Lumina.Application.Common.Infrastructure.Authorization.Policies.LibraryOwnership;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
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
    // The policy is registered per request, so this is a per-request cache keyed by the user that was loaded. It is not shared between requests, and the key prevents a reused policy instance from answering for a different user.
    private Guid? _loadedUserId;
    private bool _canAccessAllLibraries;
    private HashSet<Guid> _ownedLibraryIds = [];

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

        // The policy is scoped to one request, so the user graph, including the libraries the user owns, is read once and every library of that request is answered from the same set.
        if (_loadedUserId != userId)
        {
            Result<UserEntity?> getUserResult = await _unitOfWork.UserRepository.GetByIdAsync(userId, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (getUserResult.IsFailure || getUserResult.Value is null)
            {
                _loadedUserId = userId;
                _canAccessAllLibraries = false;
                _ownedLibraryIds = [];
                return false;
            }
            // Admins can access any media library.
            _canAccessAllLibraries = getUserResult.Value.UserRole?.Role.RoleName == "Admin";
            // The user graph already carries the libraries owned by this user, so answering the policy needs no second library read.
            _ownedLibraryIds = [.. getUserResult.Value.Libraries.Select(library => library.Id)];
            _loadedUserId = userId;
        }

        return _canAccessAllLibraries || _ownedLibraryIds.Contains(libraryContext.LibraryId);
    }
}
