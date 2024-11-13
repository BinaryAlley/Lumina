#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Contracts.Responses.UsersManagement;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Mediator;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.Maintenance.ApplicationSetup.Queries.CheckInitialization;

/// <summary>
/// Handler for the query to check the initialization of the application.
/// </summary>
public class CheckInitializationQueryHandler : IRequestHandler<CheckInitializationQuery, InitializationResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckInitializationQueryHandler"/> class.
    /// </summary>
    public CheckInitializationQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Checks the initialization status of the application.
    /// </summary>
    /// <param name="request">The query containing the request.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// <see langword="true"/> if the application is initialized (the admin account is created), <see langword="false"/> otherwise.
    /// </returns>
    public async ValueTask<InitializationResponse> Handle(CheckInitializationQuery request, CancellationToken cancellationToken)
    {
        IUserRepository userRepository = _unitOfWork.GetRepository<IUserRepository>();
        // if the repository reports an error, or there are no users, the application has not been initialized
        ErrorOr<IEnumerable<UserEntity>> resultSelectUser = await userRepository.GetAllAsync(cancellationToken);
        if (!resultSelectUser.IsError)
            return new InitializationResponse(resultSelectUser.Value.Any());
        return new InitializationResponse(false);
    }
}
