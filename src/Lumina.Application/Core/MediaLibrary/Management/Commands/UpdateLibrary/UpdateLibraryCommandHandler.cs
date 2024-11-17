#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.UpdateLibrary;

/// <summary>
/// Handler for the command to update a media library.
/// </summary>
public class UpdateLibraryCommandHandler : IRequestHandler<UpdateLibraryCommand, ErrorOr<LibraryResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateLibraryCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public UpdateLibraryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the command to update a media library.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully updated <see cref="LibraryResponse"/>, or an error message.
    /// </returns>
    public async ValueTask<ErrorOr<LibraryResponse>> Handle(UpdateLibraryCommand request, CancellationToken cancellationToken)
    {
        // get a library repository
        ILibraryRepository libraryRepository = _unitOfWork.GetRepository<ILibraryRepository>();
        ErrorOr<LibraryEntity?> resultGetLibrary = await libraryRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (resultGetLibrary.IsError)
            return resultGetLibrary.Errors;
        else if (resultGetLibrary.Value is null)
            return DomainErrors.Library.LibraryNotFound;
        // if the user that made the request is not an Admin or is not the owner of the library, they do not have the right to update it
        if (resultGetLibrary.Value.UserId != request.CurrentUserId) // TODO: after implementing the Admin role, add that permission check here too
            return ApplicationErrors.Authorization.NotAuthorized;

        // create a domain library object
        ErrorOr<Library> createLibraryResult = Library.Create(
            LibraryId.Create(request.Id),
            request.OwnerId,
            request.Title!,
            Enum.Parse<LibraryType>(request.LibraryType!),
            request.ContentLocations!
        );
        if (createLibraryResult.IsError)
            return createLibraryResult.Errors;
        // convert the domain library entity to a repository library entity
        LibraryEntity persistenceLibrary = createLibraryResult.Value.ToRepositoryEntity();

        // update the repository entity and save changes
        ErrorOr<Updated> updateLibraryResult = await libraryRepository.UpdateAsync(persistenceLibrary, cancellationToken).ConfigureAwait(false);
        if (updateLibraryResult.IsError)
            return updateLibraryResult.Errors;
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // retrieve the updated media library from the persistence medium and return it
        ErrorOr<LibraryEntity?> retrievedLibraryResult = await libraryRepository.GetByIdAsync(createLibraryResult.Value.Id.Value, cancellationToken).ConfigureAwait(false);
        if (retrievedLibraryResult.IsError)
            return retrievedLibraryResult.Errors;
        if (retrievedLibraryResult.Value is null)
            return ApplicationErrors.Persistence.ErrorPersistingMediaLibrary;
        return retrievedLibraryResult.Value.ToResponse();
    }
}
