#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Repositories.Authorization;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Errors;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
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
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Created>> InsertAsync(PermissionEntity permission, CancellationToken cancellationToken)
    {
        bool doesPermissionExist = await _luminaDbContext.Permissions.AnyAsync(repositoryPermission => repositoryPermission.Id == permission.Id, cancellationToken).ConfigureAwait(false);
        if (doesPermissionExist)
            return Errors.Authorization.PermissionAlreadyExists;

        _luminaDbContext.Permissions.Add(permission);
        return Result.Created;
    }

    /// <summary>
    /// Gets all the authorization permissions, or a page of them when the pagination data is provided.
    /// </summary>
    /// <typeparam name="TFilter">The type of the filter carrying the criteria used to filter the results.</typeparam>
    /// <param name="paginationData">The pagination data that includes the current page and the number of items per page to retrieve. If <see langword="null"/>, all the matching permissions are returned.</param>
    /// <param name="sortBy">The name of the field by which to sort the results.</param>
    /// <param name="sortOrder">The direction in which to sort the results.</param>
    /// <param name="filterModel">The model containing the parameters used to filter the results.</param>
    /// <param name="shouldIncludeNavigationProperties">Whether the navigation properties of the entities should be loaded together with the entities themselves. Pass <see langword="false"/> to retrieve only the data stored directly on the entity rows.</param>
    /// <param name="shouldTrackEntities">Whether the retrieved entities should be tracked by the persistence medium, so that changes to them can be saved. Pass <see langword="false"/> for read-only scenarios, to avoid the tracking overhead.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a paginated result of <see cref="PermissionEntity"/>, or an error.</returns>
    public async Task<Result<PaginatedResultDto<PermissionEntity>>> GetAllAsync<TFilter>(PaginationDataDto? paginationData = null, string? sortBy = null, SortOrder? sortOrder = null, TFilter? filterModel = null, bool shouldIncludeNavigationProperties = true, bool shouldTrackEntities = true, CancellationToken cancellationToken = default) where TFilter : BaseFilterDto
    {
        IQueryable<PermissionEntity> query = _luminaDbContext.Permissions;
        if (!shouldTrackEntities)
            query = query.AsNoTracking();

        // If no pagination was requested, return all the permissions.
        if (paginationData is null)
        {
            IReadOnlyList<PermissionEntity> allPermissions = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
            return new PaginatedResultDto<PermissionEntity>
            {
                Data = allPermissions,
                CurrentPage = 1,
                PerPage = allPermissions.Count,
                Count = allPermissions.Count,
                NumberOfPages = 1
            };
        }

        int count = await query.Select(permission => permission.Id).CountAsync(cancellationToken).ConfigureAwait(false);
        int numberOfPages = (int)Math.Ceiling((double)count / paginationData.PerPage);
        int currentPage = Math.Min(paginationData.CurrentPage, Math.Max(1, numberOfPages)); // Make sure current page doesn't exceed maximum number of pages.

        IReadOnlyList<PermissionEntity> paginatedResult = await query
            .Skip((currentPage - 1) * paginationData.PerPage)
            .Take(paginationData.PerPage)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PaginatedResultDto<PermissionEntity>
        {
            Data = paginatedResult,
            CurrentPage = currentPage,
            PerPage = paginationData.PerPage,
            Count = count,
            NumberOfPages = numberOfPages
        };
    }

    /// <summary>
    /// Gets all permissions that have their Id match an Id in <paramref name="ids"/> from the storage medium.
    /// </summary>
    /// <param name="ids">The list of Ids of the permissions to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of <see cref="PermissionEntity"/>, or an error.</returns>
    public async Task<Result<IEnumerable<PermissionEntity>>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Permissions.Where(permission => ids.Contains(permission.Id)).ToListAsync(cancellationToken).ConfigureAwait(false);
    }
}
