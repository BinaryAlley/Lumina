#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Repositories.Common.Actions;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.Authorization;

/// <summary>
/// Interface for the repository for authorization roles.
/// </summary>
public interface IRoleRepository : IRepository<RoleEntity>,
                                   IGetByIdRepositoryAction<RoleEntity, Guid>,
                                   IGetAllRepositoryAction<RoleEntity>,
                                   IInsertRepositoryAction<RoleEntity>,
                                   IUpdateRepositoryAction<RoleEntity>,
                                   IDeleteByIdRepositoryAction<Guid>
{
    /// <summary>
    /// Gets a role identified by <paramref name="roleType"/> from the repository, if it exists.
    /// </summary>
    /// <param name="roleType">The type of role to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a <see cref="RoleEntity"/> if found, or an error.</returns>
    Task<ErrorOr<RoleEntity?>> GetByNameAsync(string roleType, CancellationToken cancellationToken);
}
