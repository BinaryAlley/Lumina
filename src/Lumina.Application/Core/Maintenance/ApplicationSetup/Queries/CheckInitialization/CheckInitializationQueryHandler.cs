#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Application.Common.DataAccess.Repositories.Users;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Contracts.Responses.UsersManagement;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.Maintenance.ApplicationSetup.Queries.CheckInitialization;

/// <summary>
/// Handler for the query to check the initialization of the application.
/// </summary>
public class CheckInitializationQueryHandler : IQueryHandler<CheckInitializationQuery, InitializationResponse>
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
    /// <param name="query">The query containing the request.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// <see langword="true"/> if the application is initialized (the admin account is created), <see langword="false"/> otherwise.
    /// </returns>
    public async Task<InitializationResponse> HandleAsync(CheckInitializationQuery query, CancellationToken cancellationToken)
    {
        IUserRepository userRepository = _unitOfWork.GetRepository<IUserRepository>();
        // if the repository reports an error, or there are no users, the application has not been initialized
        Result<IEnumerable<UserEntity>> selectUsersResult = await userRepository.GetAllAsync(cancellationToken);
        if (!selectUsersResult.IsFailure)
            return new InitializationResponse(selectUsersResult.Value.Any());
        return new InitializationResponse(false);
    }
}
