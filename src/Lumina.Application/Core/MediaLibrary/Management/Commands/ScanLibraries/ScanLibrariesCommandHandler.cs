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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.ScanLibraries;

/// <summary>
/// Handler for the command for initiating the scan of all media libraries.
/// </summary>
public class ScanLibrariesCommandHandler : IRequestHandler<ScanLibrariesCommand, ErrorOr<IEnumerable<ScanLibraryResponse>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediaLibraryScanningService _libraryScanningService;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanLibrariesCommandHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="libraryScanningService">Injected service for scanning media libraries.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public ScanLibrariesCommandHandler(
        IAuthorizationService authorizationService,
        ICurrentUserService currentUserService,
        IMediaLibraryScanningService libraryScanningService,
        IUnitOfWork unitOfWork)
    {
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _libraryScanningService = libraryScanningService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the command for initiating the scan of all media libraries.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public async ValueTask<ErrorOr<IEnumerable<ScanLibraryResponse>>> Handle(ScanLibrariesCommand request, CancellationToken cancellationToken)
    {
        // get all media libraries that are enabled and unlocked from the persistence medium
        ILibraryRepository libraryRepository = _unitOfWork.GetRepository<ILibraryRepository>();
        ErrorOr<IEnumerable<LibraryEntity>> getLibrariesResult = await libraryRepository.GetAllEnabledAndUnlockedAsync(cancellationToken).ConfigureAwait(false);
        if (getLibrariesResult.IsError)
            return getLibrariesResult.Errors;

        // convert persistence libraries to domain entities
        IEnumerable<ErrorOr<Library>> domainEntitiesResult = getLibrariesResult.Value.ToDomainEntities();

        // if the current user is not an admin, they can only scan the libraries that belong to them
        if (!await _authorizationService.IsInRoleAsync(_currentUserService.UserId!.Value, "Admin", cancellationToken).ConfigureAwait(false))
            domainEntitiesResult = domainEntitiesResult.Where(libraryResult => libraryResult.Value.UserId.Value == _currentUserService.UserId!.Value);

        List<ScanLibraryResponse> responses = [];
        // for each library, start the scan
        foreach (ErrorOr<Library> domainLibraryResult in domainEntitiesResult)
        {
            if (domainLibraryResult.IsError)
                return domainLibraryResult.Errors;
            Guid scanId = await _libraryScanningService.StartScanAsync(domainLibraryResult.Value, cancellationToken).ConfigureAwait(false);
            responses.Add(new ScanLibraryResponse(scanId, domainLibraryResult.Value.Id.Value));
        }
        return responses;
    }
}
