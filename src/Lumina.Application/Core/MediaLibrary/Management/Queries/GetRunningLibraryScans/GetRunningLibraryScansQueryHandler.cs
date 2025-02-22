#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Progress;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Mediator;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Queries.GetRunningLibraryScans;

/// <summary>
/// Handler for the query to get the ongoing media library scans.
/// </summary>
public class GetRunningLibraryScansQueryHandler : IRequestHandler<GetRunningLibraryScansQuery, ErrorOr<IEnumerable<MediaLibraryScanProgressResponse>>>
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
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfuly created <see cref="MediaLibraryScanProgress"/>, or an error message.
    /// </returns>
    public async ValueTask<ErrorOr<IEnumerable<MediaLibraryScanProgressResponse>>> Handle(GetRunningLibraryScansQuery request, CancellationToken cancellationToken)
    {
        // get the ongoing library scans from the repository
        ILibraryScanRepository libraryScanRepository = _unitOfWork.GetRepository<ILibraryScanRepository>();
        ErrorOr<IEnumerable<LibraryScanEntity>> getRunningScansResult = await libraryScanRepository.GetRunningScansAsync(cancellationToken).ConfigureAwait(false);
        if (getRunningScansResult.IsError)
            return getRunningScansResult.Errors;

        // filter the library scans by what the user is allowed to see
        LibraryScanEntity[] userRunningRepositoryLibraryScans = [];
        // admins can see all libraries
        if (!await _authorizationService.IsInRoleAsync(_currentUserService.UserId!.Value, "Admin", cancellationToken).ConfigureAwait(false))
            userRunningRepositoryLibraryScans = getRunningScansResult.Value.ToArray();
        else // for regular users, only take the libraries that belong to them
            userRunningRepositoryLibraryScans = getRunningScansResult.Value
                .Where(libraryScan => libraryScan.UserId == _currentUserService.UserId).ToArray();

        // for each of the filtered library scans, get their progress
        List<MediaLibraryScanProgressResponse> libraryScanProgresses = [];
        IEnumerable<ErrorOr<LibraryScan>> userRunningDomainLibraryScans = userRunningRepositoryLibraryScans.ToDomainEntities();
        foreach (ErrorOr<LibraryScan> userRunningDomainLibraryScan in userRunningDomainLibraryScans)
        {
            if (userRunningDomainLibraryScan.IsError)
                return userRunningDomainLibraryScan.Errors;
            else
            {
                ErrorOr<MediaLibraryScanProgress> getLibraryScanProgressResult = _mediaLibrariesScanProgressTracker.GetScanProgress(
                    MediaLibraryScanCompositeId.Create(userRunningDomainLibraryScan.Value.Id, userRunningDomainLibraryScan.Value.UserId));
                if (getLibraryScanProgressResult.IsError)
                    return getLibraryScanProgressResult.Errors;
                libraryScanProgresses.Add(getLibraryScanProgressResult.Value.ToResponse());
            }
        }

        return ErrorOrFactory.From(libraryScanProgresses.AsEnumerable());
    }
}
