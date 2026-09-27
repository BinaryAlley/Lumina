#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Repositories.Authorization;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Errors;
using Lumina.DataAccess.Common.Persistence;
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
/// Repository for authorization roles.
/// </summary>
internal sealed class RoleRepository : IRoleRepository
{
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoleRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public RoleRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Adds a new authorization role.
    /// </summary>
    /// <param name="role">The authorization role to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Created>> InsertAsync(RoleEntity role, CancellationToken cancellationToken)
    {
        bool doesRoleExist = await _luminaDbContext.Roles.AnyAsync(repositoryRole => repositoryRole.Id == role.Id || repositoryRole.RoleName == role.RoleName, cancellationToken).ConfigureAwait(false);
        if (doesRoleExist)
            return Errors.Authorization.RoleAlreadyExists;

        _luminaDbContext.Roles.Add(role);
        return Result.Created;
    }

    /// <summary>
    /// Gets all the authorization roles, or a page of them when the pagination data is provided.
    /// </summary>
    /// <typeparam name="TFilter">The type of the filter carrying the criteria used to filter the results.</typeparam>
    /// <param name="paginationData">The pagination data that includes the current page and the number of items per page to retrieve. If <see langword="null"/>, all the matching roles are returned.</param>
    /// <param name="sortBy">The name of the field by which to sort the results.</param>
    /// <param name="sortOrder">The direction in which to sort the results.</param>
    /// <param name="filterModel">The model containing the parameters used to filter the results.</param>
    /// <param name="shouldIncludeNavigationProperties">Whether the navigation properties of the entities should be loaded together with the entities themselves. Pass <see langword="false"/> to retrieve only the data stored directly on the entity rows.</param>
    /// <param name="shouldTrackEntities">Whether the retrieved entities should be tracked by the persistence medium, so that changes to them can be saved. Pass <see langword="false"/> for read-only scenarios, to avoid the tracking overhead.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a paginated result of <see cref="RoleEntity"/>, or an error.</returns>
    public async Task<Result<PaginatedResultDto<RoleEntity>>> GetAllAsync<TFilter>(PaginationDataDto? paginationData = null, string? sortBy = null, SortOrder? sortOrder = null, TFilter? filterModel = null, bool shouldIncludeNavigationProperties = true, bool shouldTrackEntities = true, CancellationToken cancellationToken = default) where TFilter : BaseFilterDto
    {
        IQueryable<RoleEntity> query = _luminaDbContext.Roles;
        // Apply no-tracking when the caller only reads, so the entities are not tracked by the context.
        if (!shouldTrackEntities)
            query = query.AsNoTracking();

        // If no pagination was requested, return all the roles.
        if (paginationData is null)
        {
            IReadOnlyList<RoleEntity> allRoles = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
            return new PaginatedResultDto<RoleEntity>
            {
                Data = allRoles,
                CurrentPage = 1,
                PerPage = allRoles.Count,
                Count = allRoles.Count,
                NumberOfPages = 1
            };
        }

        // Count the matching roles first, so the requested page can be clamped to the number of available pages.
        int count = await query.Select(role => role.Id).CountAsync(cancellationToken).ConfigureAwait(false);
        int numberOfPages = (int)Math.Ceiling((double)count / paginationData.PerPage);
        int currentPage = Math.Min(paginationData.CurrentPage, Math.Max(1, numberOfPages)); // Make sure current page doesn't exceed maximum number of pages.

        // Fetch only the requested page of roles.
        IReadOnlyList<RoleEntity> paginatedResult = await query
            .Skip((currentPage - 1) * paginationData.PerPage)
            .Take(paginationData.PerPage)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PaginatedResultDto<RoleEntity>
        {
            Data = paginatedResult,
            CurrentPage = currentPage,
            PerPage = paginationData.PerPage,
            Count = count,
            NumberOfPages = numberOfPages
        };
    }

    /// <summary>
    /// Gets a role identified by <paramref name="roleType"/> from the repository, if it exists.
    /// </summary>
    /// <param name="roleType">The type of role to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a <see cref="RoleEntity"/> if found, or an error.</returns>
    public async Task<Result<RoleEntity?>> GetByNameAsync(string roleType, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Roles
            .Include(role => role.RolePermissions)
            .ThenInclude(library => library.Permission)
            .AsSplitQuery()
            .FirstOrDefaultAsync(role => role.RoleName == roleType, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a <see cref="RoleEntity"/> identified by <paramref name="id"/> from the storage medium.
    /// </summary>
    /// <param name="id">The id of the authorization role to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a <see cref="RoleEntity"/> identified by <paramref name="id"/>, or an error.</returns>
    public async Task<Result<RoleEntity?>> GetByIdAsync(Guid id, bool shouldIncludeNavigationProperties = true, bool shouldTrackEntities = true, CancellationToken cancellationToken = default)
    {
        IQueryable<RoleEntity> query = _luminaDbContext.Roles;
        // Apply no-tracking when the caller only reads, so the entities are not tracked by the context.
        if (!shouldTrackEntities)
            query = query.AsNoTracking();
        if (shouldIncludeNavigationProperties)
            query = query
                .Include(role => role.RolePermissions)
                .ThenInclude(rolePermission => rolePermission.Permission)
                .AsSplitQuery();
        return await query.FirstOrDefaultAsync(role => role.Id == id, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates a role.
    /// </summary>
    /// <param name="data">The role to update.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> UpdateAsync(RoleEntity data, CancellationToken cancellationToken)
    {
        // Check if a role with the requested Id exists, and retrieve it.
        RoleEntity? foundRole = await _luminaDbContext.Roles
            .Include(role => role.RolePermissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .AsSplitQuery()
            .FirstOrDefaultAsync(role => role.Id == data.Id, cancellationToken).ConfigureAwait(false);
        if (foundRole is null)
            return Errors.Authorization.RoleNotFound;

        // The stored identity is never overwritten by an edit, and the audit columns are only ever written by the auditing interceptor.
        EditableValuesCopier.CopyEditableValues(_luminaDbContext, foundRole, data);

        // A permission participation is matched by its permission. Matched participations keep their identity and their audit columns, so a permission
        // that is referenced elsewhere is never deleted and re-inserted just because another field of the role was edited.
        CollectionReconciler.Reconcile(
            foundRole.RolePermissions,
            data.RolePermissions,
            existingRolePermission => existingRolePermission.PermissionId,
            incomingRolePermission => incomingRolePermission.PermissionId,
            shouldReplace: (existingRolePermission, incomingRolePermission) => false,
            createNew: incomingRolePermission => new RolePermissionEntity
            {
                RoleId = foundRole.Id,
                PermissionId = incomingRolePermission.PermissionId,
                Permission = incomingRolePermission.Permission,
                Role = foundRole
            });
        return Result.Updated;
    }

    /// <summary>
    /// Deletes a role identified by <paramref name="id"/> from the storage medium.
    /// </summary>
    /// <param name="id">The id of the role to be deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Deleted>> DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        // Check if a role with the requested Id exists, and retrieve it.
        RoleEntity? foundRole = await _luminaDbContext.Roles
            .FirstOrDefaultAsync(role => role.Id == id, cancellationToken).ConfigureAwait(false);
        if (foundRole is null)
            return Errors.Authorization.RoleNotFound;

        // Remove the role.
        _luminaDbContext.Remove(foundRole);
        return Result.Deleted;
    }
}
