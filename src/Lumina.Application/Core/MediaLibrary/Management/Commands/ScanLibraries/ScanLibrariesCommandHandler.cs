#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Application.Core.MediaLibrary.Management.Services.Scanning;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Mediator;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lumina.Application.Common.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.ScanLibraries;

/// <summary>
/// Handler for the command for initiating the scan of all media libraries.
/// </summary>
public class ScanLibrariesCommandHandler : IRequestHandler<ScanLibrariesCommand, ErrorOr<Success>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILibraryScanningService _libraryScanningService;
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
        ILibraryScanningService libraryScanningService,
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
    public async ValueTask<ErrorOr<Success>> Handle(ScanLibrariesCommand request, CancellationToken cancellationToken)
    {
        // if the user that made the request is not an Admin, they do not have the right to perform scans
        if (!await _authorizationService.IsInRoleAsync(_currentUserService.UserId!.Value, "Admin", cancellationToken).ConfigureAwait(false))
            return Errors.Authorization.NotAuthorized;

        // get all media libraries from the persistence medium
        ILibraryRepository libraryRepository = _unitOfWork.GetRepository<ILibraryRepository>();
        ErrorOr<IEnumerable<LibraryEntity>> getLibrariesResult = await libraryRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        if (getLibrariesResult.IsError)
            return getLibrariesResult.Errors;

        // convert persistence libraries to domain entities, and start scanning them
        IEnumerable<ErrorOr<Library>> domainEntitiesResult = getLibrariesResult.Value.ToDomainEntities();
        foreach (ErrorOr<Library> domainLibraryResult in domainEntitiesResult)
        {
            if (domainLibraryResult.IsError)
                return domainLibraryResult.Errors;
            await _libraryScanningService.StartScanAsync(domainLibraryResult.Value, cancellationToken).ConfigureAwait(false);
        }
        return Result.Success;
    }
}
