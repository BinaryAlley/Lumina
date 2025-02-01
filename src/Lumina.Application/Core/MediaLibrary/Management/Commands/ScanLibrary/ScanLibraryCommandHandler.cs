#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Application.Core.MediaLibrary.Management.Services.Scanning;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.ScanLibrary;

/// <summary>
/// Handler for the command for initiating the scan of a media library.
/// </summary>
public class ScanLibraryCommandHandler : IRequestHandler<ScanLibraryCommand, ErrorOr<ScanLibraryResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediaLibraryScanningService _mediaLibraryScanningService;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanLibraryCommandHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="mediaLibraryScanningService">Injected service for scanning media libraries.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public ScanLibraryCommandHandler(
        IAuthorizationService authorizationService,
        ICurrentUserService currentUserService,
        IMediaLibraryScanningService mediaLibraryScanningService,
        IUnitOfWork unitOfWork)
    {
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _mediaLibraryScanningService = mediaLibraryScanningService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the command for initiating the scan of a media library.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public async ValueTask<ErrorOr<ScanLibraryResponse>> Handle(ScanLibraryCommand request, CancellationToken cancellationToken)
    {
        // get the media library from the persistence medium
        ILibraryRepository libraryRepository = _unitOfWork.GetRepository<ILibraryRepository>();
        ErrorOr<LibraryEntity?> getLibraryResult = await libraryRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (getLibraryResult.IsError)
            return getLibraryResult.Errors;

        if (getLibraryResult.Value is null)
            return DomainErrors.Library.LibraryNotFound;

        // if the user that wants to delete the library is not an Admin or is not the owner of the library, they do not have the right to scan it
        if (getLibraryResult.Value.UserId != _currentUserService.UserId ||
            !await _authorizationService.IsInRoleAsync(_currentUserService.UserId!.Value, "Admin", cancellationToken).ConfigureAwait(false))
            return ApplicationErrors.Authorization.NotAuthorized;

        // check if the library is enabled and unlocked, before scanning it
        if (!getLibraryResult.Value.IsEnabled)
            return DomainErrors.Library.CannotScanDisabledLibrary;

        if (getLibraryResult.Value.IsLocked)
            return DomainErrors.Library.CannotScanLockedLibrary;

        // convert persistence libraries to domain entities
        ErrorOr<Library> domainLibraryResult = getLibraryResult.Value.ToDomainEntity();

        if (domainLibraryResult.IsError)
            return domainLibraryResult.Errors;

        // start the media library scan
        Guid scanId = await _mediaLibraryScanningService.StartScanAsync(domainLibraryResult.Value, cancellationToken).ConfigureAwait(false);
        return new ScanLibraryResponse(scanId, domainLibraryResult.Value.Id.Value);
    }
}
