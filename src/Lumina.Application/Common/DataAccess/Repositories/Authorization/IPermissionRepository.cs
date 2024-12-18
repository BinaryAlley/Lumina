#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Repositories.Common.Actions;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.Authorization;

/// <summary>
/// Interface for the repository for authorization permissions.
/// </summary>
public interface IPermissionRepository : IRepository<PermissionEntity>,
                                         IInsertRepositoryAction<PermissionEntity>,
                                         IGetAllRepositoryAction<PermissionEntity>
{
    /// <summary>
    /// Gets all permissions that have their Id match an Id in <paramref name="ids"/> from the storage medium.
    /// </summary>
    /// <param name="ids">The list of Ids of the permissions to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="PermissionEntity"/>, or an error.</returns>
    Task<ErrorOr<IEnumerable<PermissionEntity>>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);
}
