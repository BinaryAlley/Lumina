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
        // an authenticated request must always carry a user identity
        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return Errors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // get the ongoing library scans from the repository
        Result<IEnumerable<LibraryScanEntity>> getRunningScansResult = await _unitOfWork.LibraryScanRepository.GetRunningScansAsync(cancellationToken).ConfigureAwait(false);
        if (getRunningScansResult.IsFailure)
            return getRunningScansResult.Errors;

        // filter the library scans by what the user is allowed to see: admins see all libraries, regular users only their own
        LibraryScanEntity[] userRunningRepositoryLibraryScans = [];
        if (await _authorizationService.IsInRoleAsync(userId, "Admin", cancellationToken).ConfigureAwait(false))
            userRunningRepositoryLibraryScans = [.. getRunningScansResult.Value];
        else
            userRunningRepositoryLibraryScans = getRunningScansResult.Value
                .Where(libraryScan => libraryScan.UserId == userId).ToArray();

        // for each of the filtered library scans, get their progress
        List<MediaLibraryScanProgressResponse> libraryScanProgresses = [];
        IEnumerable<Result<LibraryScan>> userRunningDomainLibraryScans = userRunningRepositoryLibraryScans.ToDomainEntities();
        foreach (Result<LibraryScan> userRunningDomainLibraryScan in userRunningDomainLibraryScans)
        {
            if (userRunningDomainLibraryScan.IsFailure)
                return userRunningDomainLibraryScan.Errors;
            else
            {
                Result<MediaLibraryScanProgress> getLibraryScanProgressResult = _mediaLibrariesScanProgressTracker.GetScanProgress(
                    MediaLibraryScanCompositeId.Create(userRunningDomainLibraryScan.Value.Id, userRunningDomainLibraryScan.Value.UserId));
                if (getLibraryScanProgressResult.IsFailure)
                    return getLibraryScanProgressResult.Errors;
                libraryScanProgresses.Add(getLibraryScanProgressResult.Value.ToResponse());
            }
        }

        return Result.From(libraryScanProgresses.AsEnumerable());
    }
}
