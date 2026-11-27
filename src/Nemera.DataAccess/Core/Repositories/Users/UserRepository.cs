#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Primitives;
using Nemera.Application.Common.DataAccess.Entities.Authorization;
using Nemera.Application.Common.DataAccess.Entities.UsersManagement;
using Nemera.Application.Common.DataAccess.Repositories.Users;
using Nemera.Application.Common.DTO.Filtering;
using Nemera.Application.Common.DTO.Pagination;
using Nemera.DataAccess.Common.Persistence;
using Nemera.DataAccess.Core.UoW;
using Nemera.Domain.Common.Errors;
using Nemera.Domain.SharedKernel.Common.Enums.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.DataAccess.Core.Repositories.Users;

/// <summary>
/// Repository for users.
/// </summary>
internal sealed class UserRepository : IUserRepository
{
    private readonly NemeraDbContext _nemeraDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepository"/> class.
    /// </summary>
    /// <param name="nemeraDbContext">Injected Entity Framework DbContext.</param>
    public UserRepository(NemeraDbContext nemeraDbContext)
    {
        _nemeraDbContext = nemeraDbContext;
    }

    /// <summary>
    /// Adds a new user.
    /// </summary>
    /// <param name="user">The user to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Created>> InsertAsync(UserEntity user, CancellationToken cancellationToken)
    {
        bool doesUserExist = await _nemeraDbContext.Users.AnyAsync(repositoryUser => repositoryUser.Id == user.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (doesUserExist)
            return Errors.Users.UserAlreadyExists;

        _nemeraDbContext.Users.Add(user);
        return Result.Created;
    }

    /// <summary>
    /// Gets all the users, or a page of them when the pagination data is provided.
    /// </summary>
    /// <typeparam name="TFilter">The type of the filter carrying the criteria used to filter the results.</typeparam>
    /// <param name="paginationData">The pagination data that includes the current page and the number of items per page to retrieve. If <see langword="null"/>, all the matching users are returned.</param>
    /// <param name="sortBy">The name of the field by which to sort the results.</param>
    /// <param name="sortOrder">The direction in which to sort the results.</param>
    /// <param name="filterModel">The model containing the parameters used to filter the results.</param>
    /// <param name="shouldIncludeNavigationProperties">Whether the navigation properties of the entities should be loaded together with the entities themselves. Pass <see langword="false"/> to retrieve only the data stored directly on the entity rows.</param>
    /// <param name="shouldTrackEntities">Whether the retrieved entities should be tracked by the persistence medium, so that changes to them can be saved. Pass <see langword="false"/> for read-only scenarios, to avoid the tracking overhead.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a paginated result of <see cref="UserEntity"/>, or an error.</returns>
    public async Task<Result<PaginatedResultDto<UserEntity>>> GetAllAsync<TFilter>(PaginationDataDto? paginationData = null, string? sortBy = null, SortOrder? sortOrder = null, TFilter? filterModel = null, bool shouldIncludeNavigationProperties = true, bool shouldTrackEntities = true, CancellationToken cancellationToken = default) where TFilter : BaseFilterDto
    {
        IQueryable<UserEntity> query = _nemeraDbContext.Users;
        if (!shouldTrackEntities)
            query = query.AsNoTracking();

        // If no pagination was requested, return all the users.
        if (paginationData is null)
        {
            IReadOnlyList<UserEntity> allUsers = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
            return new PaginatedResultDto<UserEntity>
            {
                Data = allUsers,
                CurrentPage = 1,
                PerPage = allUsers.Count,
                Count = allUsers.Count,
                NumberOfPages = 1
            };
        }

        int count = await query.Select(user => user.Id).CountAsync(cancellationToken).ConfigureAwait(false);
        int numberOfPages = (int)Math.Ceiling((double)count / paginationData.PerPage);
        int currentPage = Math.Min(paginationData.CurrentPage, Math.Max(1, numberOfPages)); // Make sure current page doesn't exceed maximum number of pages.

        IReadOnlyList<UserEntity> paginatedResult = await query
            .Skip((currentPage - 1) * paginationData.PerPage)
            .Take(paginationData.PerPage)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PaginatedResultDto<UserEntity>
        {
            Data = paginatedResult,
            CurrentPage = currentPage,
            PerPage = paginationData.PerPage,
            Count = count,
            NumberOfPages = numberOfPages
        };
    }

    /// <summary>
    /// Gets a user identified by <paramref name="username"/> from the repository, if it exists.
    /// </summary>
    /// <param name="username">The username of the user to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a <see cref="UserEntity"/> if found, or an error.</returns>
    public async Task<Result<UserEntity?>> GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        return await _nemeraDbContext.Users
            .Include(user => user.Libraries)
                .ThenInclude(library => library.ContentLocations)
            .Include(user => user.UserPermissions)
                .ThenInclude(userPermission => userPermission.Permission)
            .Include(user => user.UserRole!.Role.RolePermissions)
                .ThenInclude(rolePermission => rolePermission.Permission)
            .AsSplitQuery()
            .FirstOrDefaultAsync(user => user.Username == username, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Updates an user.
    /// </summary>
    /// <param name="data">Ther user to update.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> UpdateAsync(UserEntity data, CancellationToken cancellationToken)
    {
        UserEntity? foundUser = await _nemeraDbContext.Users
            .Include(user => user.Libraries)
                .ThenInclude(library => library.ContentLocations)
            .Include(user => user.UserPermissions)
                .ThenInclude(userPermission => userPermission.Permission)
            .Include(user => user.UserRole!.Role.RolePermissions)
                .ThenInclude(rolePermission => rolePermission.Permission)
            .AsSplitQuery()
            .FirstOrDefaultAsync(user => user.Username == data.Username, cancellationToken)
            .ConfigureAwait(false);
        if (foundUser is null)
            return Errors.Users.UserDoesNotExist;

        // The stored identity is never overwritten by an edit, and the audit columns are only ever written by the auditing interceptor.
        EditableValuesCopier.CopyEditableValues(_nemeraDbContext, foundUser, data);

        // A permission participation is matched by its permission. Matched participations keep their identity and their audit columns, so a permission
        // that is referenced elsewhere is never deleted and re-inserted just because another field of the user was edited.
        CollectionReconciler.Reconcile(
            foundUser.UserPermissions,
            data.UserPermissions,
            existingUserPermission => existingUserPermission.PermissionId,
            incomingUserPermission => incomingUserPermission.PermissionId,
            shouldReplace: (existingUserPermission, incomingUserPermission) => false,
            createNew: incomingUserPermission => new UserPermissionEntity
            {
                UserId = foundUser.Id,
                PermissionId = incomingUserPermission.PermissionId,
                Permission = incomingUserPermission.Permission,
                User = foundUser
            });

        // Update user role.
        if (foundUser.UserRole != null)
            _nemeraDbContext.UserRoles.Remove(foundUser.UserRole);
        if (data.UserRole is not null)
            _nemeraDbContext.UserRoles.Add(new UserRoleEntity
            {
                UserId = foundUser.Id,
                RoleId = data.UserRole.RoleId,
                Role = data.UserRole.Role,
                User = foundUser
            });
        return Result.Updated;
    }

    /// <summary>
    /// Gets a <see cref="UserEntity"/> identified by <paramref name="id"/> from the storage medium.
    /// </summary>
    /// <param name="id">The id of the user to get.</param>
    /// <param name="shouldIncludeNavigationProperties">Whether the navigation properties of the user should be loaded together with the user itself.</param>
    /// <param name="shouldTrackEntities">Whether the retrieved user should be tracked by the persistence medium, so that changes to it can be saved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a <see cref="UserEntity"/> identified by <paramref name="id"/>, or an error.</returns>
    public async Task<Result<UserEntity?>> GetByIdAsync(Guid id, bool shouldIncludeNavigationProperties = true, bool shouldTrackEntities = true, CancellationToken cancellationToken = default)
    {
        IQueryable<UserEntity> query = _nemeraDbContext.Users;
        if (!shouldTrackEntities)
            query = query.AsNoTracking();
        if (shouldIncludeNavigationProperties)
            query = query
                .Include(user => user.Libraries)
                    .ThenInclude(library => library.ContentLocations)
                .Include(user => user.UserPermissions)
                    .ThenInclude(userPermission => userPermission.Permission)
                .Include(user => user.UserRole!.Role.RolePermissions)
                    .ThenInclude(rolePermission => rolePermission.Permission)
                .AsSplitQuery();
        return await query.FirstOrDefaultAsync(user => user.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }
}
