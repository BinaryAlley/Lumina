#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Progress;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Queries.GetRunningLibraryScans;

/// <summary>
/// Handler for the query to get the ongoing media library scans.
/// </summary>
public class GetRunningLibraryScansQueryHandler : IQueryHandler<GetRunningLibraryScansQuery, Result<IEnumerable<MediaLibraryScanProgressResponse>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IMediaLibrariesScanProgressTracker _mediaLibrariesScanProgressTracker;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRunningLibraryScansQueryHandler"/> class.
    /// </summary>
    /// <param name="mediaLibrariesScanProgressTracker">Injected service for tracking the progress of media library scans.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public GetRunningLibraryScansQueryHandler(
        IMediaLibrariesScanProgressTracker mediaLibrariesScanProgressTracker,
        IAuthorizationService authorizationService,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _mediaLibrariesScanProgressTracker = mediaLibrariesScanProgressTracker;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the query to get the ongoing media library scans.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="MediaLibraryScanProgress"/>, or an error message.
    /// </returns>
    public async Task<Result<IEnumerable<MediaLibraryScanProgressResponse>>> HandleAsync(GetRunningLibraryScansQuery query, CancellationToken cancellationToken)
    {
        // An authenticated request must always carry a user identity.
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return Errors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // Get the library scans that are executing or still waiting in the scan queue, from the repository.
        Result<IEnumerable<LibraryScanEntity>> getActiveScansResult = await _unitOfWork.LibraryScanRepository.GetActiveScansAsync(cancellationToken).ConfigureAwait(false);
        if (getActiveScansResult.IsFailure)
            return getActiveScansResult.Errors;

        // Filter the library scans by what the user is allowed to see: admins see all libraries, regular users only their own.
        LibraryScanEntity[] userActiveRepositoryLibraryScans = [];
        if (await _authorizationService.IsInRoleAsync(userId, AuthorizationRoles.ADMIN, cancellationToken).ConfigureAwait(false))
            userActiveRepositoryLibraryScans = [.. getActiveScansResult.Value];
        else
            userActiveRepositoryLibraryScans = [.. getActiveScansResult.Value.Where(libraryScan => libraryScan.UserId == userId)];

        // For each of the filtered library scans, get their progress.
        List<MediaLibraryScanProgressResponse> libraryScanProgresses = [];
        IEnumerable<Result<LibraryScan>> userActiveDomainLibraryScans = userActiveRepositoryLibraryScans.ToDomainEntities();
        foreach (Result<LibraryScan> userActiveDomainLibraryScan in userActiveDomainLibraryScans)
        {
            if (userActiveDomainLibraryScan.IsFailure)
                return userActiveDomainLibraryScan.Errors;

            Result<MediaLibraryScanProgress> getLibraryScanProgressResult = _mediaLibrariesScanProgressTracker.GetScanProgress(
                MediaLibraryScanCompositeId.Create(userActiveDomainLibraryScan.Value.Id, userActiveDomainLibraryScan.Value.UserId));
            // A scan whose progress was already released has ended, so it is not reported as active any more.
            if (getLibraryScanProgressResult.IsFailure)
                continue;

            libraryScanProgresses.Add(getLibraryScanProgressResult.Value.ToResponse());
        }

        return Result.From(libraryScanProgresses.AsEnumerable());
    }
}
