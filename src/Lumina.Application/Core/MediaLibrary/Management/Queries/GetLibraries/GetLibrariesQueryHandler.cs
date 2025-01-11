#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Mediator;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibraries;

/// <summary>
/// Handler for the query to authenticate an account.
/// </summary>
public class GetLibrariesQueryHandler : IRequestHandler<GetLibrariesQuery, ErrorOr<LibraryResponse[]>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibrariesQueryHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public GetLibrariesQueryHandler(
        IAuthorizationService authorizationService,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the query to authenticate an account.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfuly created <see cref="LibraryResponse"/>, or an error message.
    /// </returns>
    public async ValueTask<ErrorOr<LibraryResponse[]>> Handle(GetLibrariesQuery request, CancellationToken cancellationToken)
    {
        // get the library with the specified id from the repository
        ILibraryRepository libraryRepository = _unitOfWork.GetRepository<ILibraryRepository>();
        ErrorOr<IEnumerable<LibraryEntity>> getLibrariesResult = await libraryRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        if (getLibrariesResult.IsError)
            return getLibrariesResult.Errors;

        // admins can see all libraries
        if (await _authorizationService.IsInRoleAsync(_currentUserService.UserId!.Value, "Admin", cancellationToken).ConfigureAwait(false))
            return ErrorOrFactory.From(getLibrariesResult.Value.Select(library => library.ToResponse()).ToArray());
        else
        {
            // for regular users, only take the libraries that belong to them
            LibraryResponse[] userLibraries = getLibrariesResult.Value
                .Where(library => library.UserId == _currentUserService.UserId)
                .Select(library => library.ToResponse()).ToArray();

            return ErrorOrFactory.From(userLibraries);
        }
    }
}
