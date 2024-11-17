#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Commands.AddLibrary;

/// <summary>
/// Handler for the command to add a media library.
/// </summary>
public class AddLibraryCommandHandler : IRequestHandler<AddLibraryCommand, ErrorOr<LibraryResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddLibraryCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public AddLibraryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the command to add a media library.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfully created <see cref="LibraryResponse"/>, or an error message.
    /// </returns>
    public async ValueTask<ErrorOr<LibraryResponse>> Handle(AddLibraryCommand request, CancellationToken cancellationToken)
    {
        // create a domain library object
        ErrorOr<Library> createLibraryResult = Library.Create(
            request.UserId,
            request.Title!,
            Enum.Parse<LibraryType>(request.LibraryType!),
            request.ContentLocations!
        );

        if (createLibraryResult.IsError)
            return createLibraryResult.Errors;
        // get a library repository
        ILibraryRepository libraryRepository = _unitOfWork.GetRepository<ILibraryRepository>();
        // convert the domain library entity to a repository library entity
        LibraryEntity persistenceLibrary = createLibraryResult.Value.ToRepositoryEntity();
        // insert the repository entity and save changes
        ErrorOr<Created> insertLibraryResult = await libraryRepository.InsertAsync(persistenceLibrary, cancellationToken).ConfigureAwait(false);
        if (insertLibraryResult.IsError)
            return insertLibraryResult.Errors;
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        // retrieve the newly saved media library from the persistence medium and return it
        ErrorOr<LibraryEntity?> retrievedLibraryResult = await libraryRepository.GetByIdAsync(createLibraryResult.Value.Id.Value, cancellationToken).ConfigureAwait(false);
        if (retrievedLibraryResult.IsError)
            return retrievedLibraryResult.Errors;
        if (retrievedLibraryResult.Value is null)
            return Errors.Persistence.ErrorPersistingMediaLibrary;
        return retrievedLibraryResult.Value.ToResponse();
    }
}
