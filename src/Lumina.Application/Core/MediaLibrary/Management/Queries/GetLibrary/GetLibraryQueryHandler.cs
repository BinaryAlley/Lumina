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
using Mediator;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.MediaLibrary.Management.Queries.GetLibrary;

/// <summary>
/// Handler for the query to get a library by its Id.
/// </summary>
public class GetLibraryQueryHandler : IRequestHandler<GetLibraryQuery, ErrorOr<LibraryResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryQueryHandler"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public GetLibraryQueryHandler(
        IAuthorizationService authorizationService,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the query to get a library by its Id.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfuly created <see cref="LibraryResponse"/>, or an error message.
    /// </returns>
    public async ValueTask<ErrorOr<LibraryResponse>> Handle(GetLibraryQuery request, CancellationToken cancellationToken)
    {
        // get the library with the specified id from the repository
        ILibraryRepository libraryRepository = _unitOfWork.GetRepository<ILibraryRepository>();
        ErrorOr<LibraryEntity?> getLibraryResult = await libraryRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (getLibraryResult.IsError)
            return getLibraryResult.Errors;
        else if (getLibraryResult.Value is null)
            return DomainErrors.Library.LibraryNotFound;

        // if the user that requested the library is not an Admin or is not the owner of the library, they do not have the right to view it
        if (getLibraryResult.Value.UserId != _currentUserService.UserId ||
            (!await _authorizationService.IsInRoleAsync(_currentUserService.UserId!.Value, "Admin", cancellationToken).ConfigureAwait(false) &&
             !await _authorizationService.HasPermissionAsync(_currentUserService.UserId!.Value, AuthorizationPermission.CanCreateLibraries, cancellationToken)))
            return ApplicationErrors.Authorization.NotAuthorized;

        return getLibraryResult.Value.ToResponse();
    }
}
