#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Repositories.Authorization;
using Lumina.Application.Common.Errors;
using Lumina.DataAccess.Core.UoW;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.Core.Repositories.Authorization;

/// <summary>
/// Repository for authorization permissions.
/// </summary>
internal sealed class PermissionRepository : IPermissionRepository
{
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public PermissionRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Adds a new authorization permission.
    /// </summary>
    /// <param name="permission">The authorization permission to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<ErrorOr<Created>> InsertAsync(PermissionEntity permission, CancellationToken cancellationToken)
    {
        bool permissionExists = await _luminaDbContext.Permissions.AnyAsync(repositoryPermission => repositoryPermission.Id == permission.Id, cancellationToken).ConfigureAwait(false);
        if (permissionExists)
            return Errors.Authorization.PermissionAlreadyExists;

        _luminaDbContext.Permissions.Add(permission);
        return Result.Created;
    }

    /// <summary>
    /// Gets all autorization permissions.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="PermissionEntity"/>, or an error.</returns>
    public async Task<ErrorOr<IEnumerable<PermissionEntity>>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Permissions.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets all permissions that have their Id match an Id in <paramref name="ids"/> from the storage medium.
    /// </summary>
    /// <param name="ids">The list of Ids of the permissions to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> containing either a collection of <see cref="PermissionEntity"/>, or an error.</returns>
    public async Task<ErrorOr<IEnumerable<PermissionEntity>>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Permissions.Where(permission => ids.Contains(permission.Id)).ToListAsync(cancellationToken).ConfigureAwait(false);
    }
}
