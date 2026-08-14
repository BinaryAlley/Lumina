#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.ScanLibrary;

/// <summary>
/// Handler for the command for initiating the scan of a media library.
/// </summary>
public class ScanLibraryCommandHandler : ICommandHandler<ScanLibraryCommand, Result<MediaLibraryScanResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDomainEventsQueue _domainEventsQueue;
    private readonly IValidator<ScanLibraryCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanLibraryCommandHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="domainEventsQueue">Injected service for the queue of domain events.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public ScanLibraryCommandHandler(
        IAuthorizationService authorizationService,
        ICurrentUserService currentUserService,
        IDomainEventsQueue domainEventsQueue,
        IUnitOfWork unitOfWork,
        IValidator<ScanLibraryCommand> validator)
    {
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _domainEventsQueue = domainEventsQueue;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command for initiating the scan of a media library.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<MediaLibraryScanResponse>> HandleAsync(ScanLibraryCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        // get the media library from the persistence medium
        Result<LibraryEntity?> getLibraryResult = await _unitOfWork.LibraryRepository.GetByIdAsync(command.Id, cancellationToken).ConfigureAwait(false);
        if (getLibraryResult.IsFailure)
            return getLibraryResult.Errors;

        if (getLibraryResult.Value is null)
            return DomainErrors.Library.LibraryNotFound;

        // if the user that wants to scan the library is not an Admin or is not the owner of the library, they do not have the right to scan it
        if (getLibraryResult.Value.UserId != _currentUserService.UserId ||
            !await _authorizationService.IsInRoleAsync(_currentUserService.UserId!.Value, "Admin", cancellationToken).ConfigureAwait(false))
            return ApplicationErrors.Authorization.NotAuthorized;

        // check if the library is enabled and unlocked, before scanning it
        if (!getLibraryResult.Value.IsEnabled)
            return DomainErrors.Library.CannotScanDisabledLibrary;

        if (getLibraryResult.Value.IsLocked)
            return DomainErrors.Library.CannotScanLockedLibrary;

        // convert the persistence library to a domain entity
        Result<Library> domainLibraryResult = getLibraryResult.Value.ToDomainEntity();
        if (domainLibraryResult.IsFailure)
            return domainLibraryResult.Errors;

        // get the past month's scans for this library
        Result<IEnumerable<LibraryScanEntity>> pastLibraryScansResult = await _unitOfWork.LibraryScanRepository.GetPastMonthScansByLibraryIdAsync(command.Id, cancellationToken).ConfigureAwait(false);
        if (pastLibraryScansResult.IsFailure)
            return pastLibraryScansResult.Errors;
        
        // convert the repository scans history to domain objects
        IEnumerable<Result<LibraryScan>> pastLibraryScansDomainResult = pastLibraryScansResult.Value.ToDomainEntities();
        foreach (Result<LibraryScan> pastLibraryScanDomainResult in pastLibraryScansDomainResult)
            if (pastLibraryScanDomainResult.IsFailure)
                return pastLibraryScanDomainResult.Errors;

        // queue the media library scan
        Result<LibraryScan> libraryScanResult = LibraryScan.Create(
            LibraryId.Create(command.Id), 
            UserId.Create(_currentUserService.UserId!.Value),
            [.. pastLibraryScansDomainResult.Select(pastLibraryScanDomainResult => pastLibraryScanDomainResult.Value)]
        );
        if (libraryScanResult.IsFailure)
            return libraryScanResult.Errors;
        
        Result<Success> queueScanResult = libraryScanResult.Value.QueueScan();
        if (queueScanResult.IsFailure)
            return queueScanResult.Errors;

        // add the library scan to the persistence medium
        Result<Created> insertLibraryScanResult = await _unitOfWork.LibraryScanRepository.InsertAsync(libraryScanResult.Value.ToRepositoryEntity(), cancellationToken).ConfigureAwait(false);
        if (insertLibraryScanResult.IsFailure)
            return insertLibraryScanResult.Errors;

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        
        // queue any domain events
        foreach (IDomainEvent domainEvent in libraryScanResult.Value.GetDomainEvents())
            _domainEventsQueue.Enqueue(domainEvent);

        return new MediaLibraryScanResponse(libraryScanResult.Value.Id.Value, domainLibraryResult.Value.Id.Value);
    }
}
