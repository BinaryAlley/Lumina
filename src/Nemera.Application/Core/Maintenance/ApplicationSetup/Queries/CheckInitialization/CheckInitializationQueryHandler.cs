#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.DataAccess.Entities.UsersManagement;
using Nemera.Application.Common.DataAccess.UoW;
using Nemera.Application.Common.DTO.Filtering;
using Nemera.Application.Common.DTO.Pagination;
using Nemera.Contracts.Responses.UsersManagement;
using Nemera.Domain.Common.Primitives;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Application.Core.Maintenance.ApplicationSetup.Queries.CheckInitialization;

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
        // If the repository reports an error, or there are no users, the application has not been initialized.
        Result<PaginatedResultDto<UserEntity>> selectUsersResult = await _unitOfWork.UserRepository.GetAllAsync<BaseFilterDto>(cancellationToken: cancellationToken);
        if (!selectUsersResult.IsFailure)
            return new InitializationResponse(selectUsersResult.Value.Data.Any());
        return new InitializationResponse(false);
    }
}
