#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibraries;

/// <summary>
/// Handler for the query to get the media libraries.
/// </summary>
public class GetLibrariesQueryHandler : IQueryHandler<GetLibrariesQuery, Result<LibraryResponse[]>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibrariesQueryHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public GetLibrariesQueryHandler(
        IAuthorizationService authorizationService,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the query to get the media libraries.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="LibraryResponse"/>, or an error message.
    /// </returns>
    public async Task<Result<LibraryResponse[]>> HandleAsync(GetLibrariesQuery query, CancellationToken cancellationToken)
    {
        // an authenticated request must always carry a user identity
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return Errors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // get the libraries from the repository
        Result<IEnumerable<LibraryEntity>> getLibrariesResult = await _unitOfWork.LibraryRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        if (getLibrariesResult.IsFailure)
            return getLibrariesResult.Errors;

        // admins can see all libraries
        if (await _authorizationService.IsInRoleAsync(userId, "Admin", cancellationToken).ConfigureAwait(false))
            return Result.From(getLibrariesResult.Value.Select(library => library.ToResponse()).ToArray());
        else
        {
            // for regular users, only take the libraries that belong to them
            LibraryResponse[] userLibraries = [.. getLibrariesResult.Value
                .Where(library => library.UserId == userId)
                .Select(library => library.ToResponse())];

            return Result.From(userLibraries);
        }
    }
}
