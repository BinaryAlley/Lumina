#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Repositories.MediaLibrary;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.DataAccess.Common.Persistence;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.Core.Repositories.Libraries;

/// <summary>
/// Repository for media libraries.
/// </summary>
internal sealed class LibraryRepository : ILibraryRepository
{
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public LibraryRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Adds a new library.
    /// </summary>
    /// <param name="library">The library to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Created>> InsertAsync(LibraryEntity library, CancellationToken cancellationToken)
    {
        bool doesLibraryExist = await _luminaDbContext.Libraries.AnyAsync(repositoryLibrary => repositoryLibrary.Id == library.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (doesLibraryExist)
            return Errors.Library.LibraryAlreadyExists;

        _luminaDbContext.Libraries.Add(library);
        return Result.Created;
    }

    /// <summary>
    /// Gets a <see cref="LibraryEntity"/> identified by <paramref name="id"/> from the storage medium.
    /// </summary>
    /// <param name="id">The Id of the library to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a <see cref="LibraryEntity"/> identified by <paramref name="id"/>, or an error.</returns>
    public async Task<Result<LibraryEntity?>> GetByIdAsync(Guid id, bool shouldIncludeNavigationProperties = true, bool shouldTrackEntities = true, CancellationToken cancellationToken = default)
    {
        IQueryable<LibraryEntity> query = _luminaDbContext.Libraries;
        if (!shouldTrackEntities)
            query = query.AsNoTracking();
        if (shouldIncludeNavigationProperties)
            query = query
                .Include(library => library.ContentLocations)
                .Include(library => library.PathTemplateParts)
                .AsSplitQuery();
        return await query.FirstOrDefaultAsync(library => library.Id == id, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets all media libraries that are marked as enabled, from the storage medium.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of <see cref="LibraryEntity"/>, or an error.</returns>
    public async Task<Result<IEnumerable<LibraryEntity>>> GetAllEnabledAsync(CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Libraries
            .Include(library => library.ContentLocations)
            .Include(library => library.PathTemplateParts)
            .AsSplitQuery()
            .Where(library => library.IsEnabled)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets all media libraries that are marked as enabled and unlocked, from the storage medium.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of <see cref="LibraryEntity"/>, or an error.</returns>
    public async Task<Result<IEnumerable<LibraryEntity>>> GetAllEnabledAndUnlockedAsync(CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Libraries
            .Include(library => library.ContentLocations)
            .Include(library => library.PathTemplateParts)
            .AsSplitQuery()
            .Where(library => library.IsEnabled && !library.IsLocked)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets all the media libraries, or a page of them when the pagination data is provided.
    /// </summary>
    /// <typeparam name="TFilter">The type of the filter carrying the criteria used to filter the results.</typeparam>
    /// <param name="paginationData">The pagination data that includes the current page and the number of items per page to retrieve. If <see langword="null"/>, all the matching media libraries are returned.</param>
    /// <param name="sortBy">The name of the field by which to sort the results.</param>
    /// <param name="sortOrder">The direction in which to sort the results.</param>
    /// <param name="filterModel">The model containing the parameters used to filter the results.</param>
    /// <param name="shouldIncludeNavigationProperties">Whether the navigation properties of the entities should be loaded together with the entities themselves. Pass <see langword="false"/> to retrieve only the data stored directly on the entity rows.</param>
    /// <param name="shouldTrackEntities">Whether the retrieved entities should be tracked by the persistence medium, so that changes to them can be saved. Pass <see langword="false"/> for read-only scenarios, to avoid the tracking overhead.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a paginated result of <see cref="LibraryEntity"/>, or an error.</returns>
    public async Task<Result<PaginatedResultDto<LibraryEntity>>> GetAllAsync<TFilter>(PaginationDataDto? paginationData = null, string? sortBy = null, SortOrder? sortOrder = null, TFilter? filterModel = null, bool shouldIncludeNavigationProperties = true, bool shouldTrackEntities = true, CancellationToken cancellationToken = default) where TFilter : BaseFilterDto
    {
        IQueryable<LibraryEntity> query = _luminaDbContext.Libraries;
        if (!shouldTrackEntities)
            query = query.AsNoTracking();
        if (shouldIncludeNavigationProperties)
            query = query
                .Include(library => library.ContentLocations)
                .Include(library => library.PathTemplateParts)
                .AsSplitQuery();

        // If no pagination was requested, return all the media libraries.
        if (paginationData is null)
        {
            IReadOnlyList<LibraryEntity> allLibraries = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
            return new PaginatedResultDto<LibraryEntity>
            {
                Data = allLibraries,
                CurrentPage = 1,
                PerPage = allLibraries.Count,
                Count = allLibraries.Count,
                NumberOfPages = 1
            };
        }

        int count = await query.Select(library => library.Id).CountAsync(cancellationToken).ConfigureAwait(false);
        int numberOfPages = (int)Math.Ceiling((double)count / paginationData.PerPage);
        int currentPage = Math.Min(paginationData.CurrentPage, Math.Max(1, numberOfPages)); // Make sure current page doesn't exceed maximum number of pages.

        IReadOnlyList<LibraryEntity> paginatedResult = await query
            .Skip((currentPage - 1) * paginationData.PerPage)
            .Take(paginationData.PerPage)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PaginatedResultDto<LibraryEntity>
        {
            Data = paginatedResult,
            CurrentPage = currentPage,
            PerPage = paginationData.PerPage,
            Count = count,
            NumberOfPages = numberOfPages
        };
    }

    /// <summary>
    /// Updates a media library, replacing only the editable data that actually changed, while preserving its identity, its audit columns, and the identity of its content locations.
    /// </summary>
    /// <param name="data">The media library whose editable data is applied to the stored library.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> UpdateAsync(LibraryEntity data, CancellationToken cancellationToken)
    {
        LibraryEntity? foundLibrary = await _luminaDbContext.Libraries
            .Include(library => library.ContentLocations)
            .Include(library => library.PathTemplateParts)
            .AsSplitQuery()
            .FirstOrDefaultAsync(library => library.Id == data.Id, cancellationToken).ConfigureAwait(false);
        if (foundLibrary is null)
            return Errors.Library.LibraryNotFound;

        // The stored identity is never overwritten by an edit, and the audit columns are only ever written by the auditing interceptor.
        EditableValuesCopier.CopyEditableValues(_luminaDbContext, foundLibrary, data);

        // A content location is matched by its path. Matched locations keep their identity and the locations that did not change are left exactly as they are stored,
        // so that editing a library never deletes and re-inserts all of its content locations.
        CollectionReconciler.Reconcile(
            foundLibrary.ContentLocations,
            data.ContentLocations,
            existingContentLocation => existingContentLocation.Path,
            incomingContentLocation => incomingContentLocation.Path,
            shouldReplace: (existingContentLocation, incomingContentLocation) => false,
            createNew: incomingContentLocation => incomingContentLocation);

        // A path template part is matched by its position in the template, and is replaced only when its editable values actually changed.
        CollectionReconciler.Reconcile(
            foundLibrary.PathTemplateParts,
            data.PathTemplateParts,
            existingPart => existingPart.Position,
            incomingPart => incomingPart.Position,
            shouldReplace: (existingPart, incomingPart) => existingPart.Kind != incomingPart.Kind
                || existingPart.Representation != incomingPart.Representation
                || existingPart.IsOptional != incomingPart.IsOptional,
            createNew: incomingPart => incomingPart);

        return Result.Updated;
    }

    /// <summary>
    /// Deletes a library by its Id.
    /// </summary>
    /// <param name="id">The Id of the library to delete.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Deleted>> DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        LibraryEntity? library = await _luminaDbContext.Libraries
            .Include(library => library.ContentLocations)
            .Include(library => library.PathTemplateParts)
            .AsSplitQuery()
            .FirstOrDefaultAsync(library => library.Id == id, cancellationToken)
            .ConfigureAwait(false);

        if (library is null)
            return Errors.Library.LibraryNotFound;

        _luminaDbContext.Libraries.Remove(library);
        return Result.Deleted;
    }
}
