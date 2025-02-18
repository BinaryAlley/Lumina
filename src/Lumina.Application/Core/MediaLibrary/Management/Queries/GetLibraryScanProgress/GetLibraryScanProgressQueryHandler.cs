#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Enums.Authorization;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Progress;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Mediator;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibraryScanProgress;

/// <summary>
/// Handler for the query to get the progress of a library scan.
/// </summary>
public class GetLibraryScanProgressQueryHandler : IRequestHandler<GetLibraryScanProgressQuery, ErrorOr<MediaLibraryScanProgressResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediaLibrariesScanProgressTracker _mediaLibrariesScanProgressTracker;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryScanProgressQueryHandler"/> class.
    /// </summary>
    /// <param name="mediaLibrariesScanProgressTracker">Injected service for tracking the progress of media library scans.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public GetLibraryScanProgressQueryHandler(
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
    /// Handles the query to get the progress of a library scan.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a <see cref="MediaLibraryScanProgressResponse"/>, or an error message.
    /// </returns>
    public async ValueTask<ErrorOr<MediaLibraryScanProgressResponse>> Handle(GetLibraryScanProgressQuery request, CancellationToken cancellationToken)
    {
        // get the library with the specified id from the repository
        ILibraryRepository libraryRepository = _unitOfWork.GetRepository<ILibraryRepository>();
        ErrorOr<LibraryEntity?> getLibraryResult = await libraryRepository.GetByIdAsync(request.LibraryId, cancellationToken).ConfigureAwait(false);
        if (getLibraryResult.IsError)
            return getLibraryResult.Errors;
        else if (getLibraryResult.Value is null)
            return DomainErrors.Library.LibraryNotFound;

        // if the user that made the request is not an Admin or is not the owner of the library, they do not have the right to view its scan progress
        if (getLibraryResult.Value.UserId != _currentUserService.UserId ||
            !await _authorizationService.IsInRoleAsync(_currentUserService.UserId!.Value, "Admin", cancellationToken).ConfigureAwait(false))
            return ApplicationErrors.Authorization.NotAuthorized;

        ErrorOr<MediaLibraryScanProgress> getProgressResult = _mediaLibrariesScanProgressTracker.GetScanProgress(
            MediaLibraryScanCompositeId.Create(ScanId.Create(request.ScanId), UserId.Create(_currentUserService.UserId!.Value)));
        if (getProgressResult.IsError)
            return getProgressResult.Errors;

        return getProgressResult.Value.ToResponse();
    }
}
