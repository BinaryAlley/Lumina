#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.DomainEvents;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Events;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Mediator;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.DeleteLibrary;

/// <summary>
/// Handler for the command to delete a library by its Id.
/// </summary>
public class DeleteLibraryCommandHandler : IRequestHandler<DeleteLibraryCommand, ErrorOr<Deleted>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDomainEventsQueue _domainEventsQueue;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteLibraryCommandHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="domainEventsQueue">Injected service for the queue of domain events.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public DeleteLibraryCommandHandler(
        IAuthorizationService authorizationService,
        ICurrentUserService currentUserService,
        IDomainEventsQueue domainEventsQueue,
        IUnitOfWork unitOfWork)
    {
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _domainEventsQueue = domainEventsQueue;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the command to delete a library by its Id.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfuly created <see cref="LibraryResponse"/>, or an error message.
    /// </returns>
    public async ValueTask<ErrorOr<Deleted>> Handle(DeleteLibraryCommand request, CancellationToken cancellationToken)
    {
        // get the library with the specified id from the repository
        ILibraryRepository libraryRepository = _unitOfWork.GetRepository<ILibraryRepository>();
        ErrorOr<LibraryEntity?> getLibraryResult = await libraryRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (getLibraryResult.IsError)
            return getLibraryResult.Errors;
        else if (getLibraryResult.Value is null)
            return DomainErrors.Library.LibraryNotFound;

        // if the user that wants to delete the library is not an Admin or is not the owner of the library, they do not have the right to delete it
        if (getLibraryResult.Value.UserId != _currentUserService.UserId ||
            !await _authorizationService.IsInRoleAsync(_currentUserService.UserId!.Value, "Admin", cancellationToken).ConfigureAwait(false))
            return ApplicationErrors.Authorization.NotAuthorized;

        // create a domain library object
        ErrorOr<Library> createLibraryResult = getLibraryResult.Value.ToDomainEntity();
        if (createLibraryResult.IsError)
            return createLibraryResult.Errors;

        // delete the domain aggregate
        ErrorOr<Deleted> deleteDomainLibraryResult = createLibraryResult.Value.Delete();
        if (deleteDomainLibraryResult.IsError)
            return deleteDomainLibraryResult.Errors;

        // queue any domain events
        foreach (IDomainEvent domainEvent in createLibraryResult.Value.PopDomainEvents())
            _domainEventsQueue.Enqueue(domainEvent);

        // perform the deletion
        ErrorOr<Deleted> deletePersistenceLibraryResult = await libraryRepository.DeleteByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (deletePersistenceLibraryResult.IsError)
            return deletePersistenceLibraryResult.Errors;
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return deletePersistenceLibraryResult;
    }
}
