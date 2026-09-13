#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Common.DataAccess.Repositories.Themes;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.DTO.Pagination;
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

namespace Lumina.DataAccess.Core.Repositories.Themes;

/// <summary>
/// Repository for themes.
/// </summary>
internal sealed class ThemeRepository : IThemeRepository
{
    private readonly LuminaDbContext _luminaDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeRepository"/> class.
    /// </summary>
    /// <param name="luminaDbContext">Injected Entity Framework DbContext.</param>
    public ThemeRepository(LuminaDbContext luminaDbContext)
    {
        _luminaDbContext = luminaDbContext;
    }

    /// <summary>
    /// Adds a new theme.
    /// </summary>
    /// <param name="theme">The theme to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Created>> InsertAsync(ThemeEntity theme, CancellationToken cancellationToken)
    {
        bool doesThemeExist = await _luminaDbContext.Themes.AnyAsync(repositoryTheme => repositoryTheme.Id == theme.Id, cancellationToken).ConfigureAwait(false);
        if (doesThemeExist)
            return Errors.Themes.ThemeNotFound;

        _luminaDbContext.Themes.Add(theme);
        return Result.Created;
    }

    /// <summary>
    /// Updates a theme.
    /// </summary>
    /// <param name="data">The theme to update.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Updated>> UpdateAsync(ThemeEntity data, CancellationToken cancellationToken)
    {
        ThemeEntity? foundTheme = await _luminaDbContext.Themes
            .FirstOrDefaultAsync(theme => theme.Id == data.Id, cancellationToken).ConfigureAwait(false);
        if (foundTheme is null)
            return Errors.Themes.ThemeNotFound;

        _luminaDbContext.Entry(foundTheme).CurrentValues.SetValues(data);
        return Result.Updated;
    }

    /// <summary>
    /// Gets all the themes from the storage medium, or a page of them when the pagination data is provided.
    /// </summary>
    /// <typeparam name="TFilter">The type of the filter carrying the criteria used to filter the results.</typeparam>
    /// <param name="paginationData">The pagination data that includes the current page and the number of items per page to retrieve. If <see langword="null"/>, all the matching themes are returned.</param>
    /// <param name="sortBy">The name of the field by which to sort the results.</param>
    /// <param name="sortOrder">The direction in which to sort the results.</param>
    /// <param name="filterModel">The model containing the parameters used to filter the results.</param>
    /// <param name="shouldIncludeNavigationProperties">Whether the navigation properties of the entities should be loaded together with the entities themselves. Pass <see langword="false"/> to retrieve only the data stored directly on the entity rows.</param>
    /// <param name="shouldTrackEntities">Whether the retrieved entities should be tracked by the persistence medium, so that changes to them can be saved. Pass <see langword="false"/> for read-only scenarios, to avoid the tracking overhead.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a paginated result of <see cref="ThemeEntity"/>, or an error.</returns>
    public async Task<Result<PaginatedResultDto<ThemeEntity>>> GetAllAsync<TFilter>(PaginationDataDto? paginationData = null, string? sortBy = null, SortOrder? sortOrder = null, TFilter? filterModel = null, bool shouldIncludeNavigationProperties = true, bool shouldTrackEntities = true, CancellationToken cancellationToken = default) where TFilter : BaseFilterDto
    {
        IQueryable<ThemeEntity> query = _luminaDbContext.Themes;
        if (!shouldTrackEntities)
            query = query.AsNoTracking();

        // If no pagination was requested, return all the themes.
        if (paginationData is null)
        {
            IReadOnlyList<ThemeEntity> allThemes = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
            return new PaginatedResultDto<ThemeEntity>
            {
                Data = allThemes,
                CurrentPage = 1,
                PerPage = allThemes.Count,
                Count = allThemes.Count,
                NumberOfPages = 1
            };
        }

        int count = await query.Select(theme => theme.Id).CountAsync(cancellationToken).ConfigureAwait(false);
        int numberOfPages = (int)Math.Ceiling((double)count / paginationData.PerPage);
        int currentPage = Math.Min(paginationData.CurrentPage, Math.Max(1, numberOfPages)); // Make sure current page doesn't exceed maximum number of pages.

        IReadOnlyList<ThemeEntity> paginatedResult = await query
            .Skip((currentPage - 1) * paginationData.PerPage)
            .Take(paginationData.PerPage)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PaginatedResultDto<ThemeEntity>
        {
            Data = paginatedResult,
            CurrentPage = currentPage,
            PerPage = paginationData.PerPage,
            Count = count,
            NumberOfPages = numberOfPages
        };
    }

    /// <summary>
    /// Gets a theme identified by its manifest <paramref name="themeId"/> from the storage medium.
    /// </summary>
    /// <param name="themeId">The manifest id of the theme to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a <see cref="ThemeEntity"/> identified by <paramref name="themeId"/>, or an error.</returns>
    public async Task<Result<ThemeEntity?>> GetByThemeIdAsync(string themeId, CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Themes
            .FirstOrDefaultAsync(theme => theme.ThemeId == themeId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes a theme identified by <paramref name="id"/> from the storage medium.
    /// </summary>
    /// <param name="id">The id of the theme to delete.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    public async Task<Result<Deleted>> DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        ThemeEntity? foundTheme = await _luminaDbContext.Themes
            .FirstOrDefaultAsync(theme => theme.Id == id, cancellationToken).ConfigureAwait(false);
        if (foundTheme is null)
            return Errors.Themes.ThemeNotFound;

        _luminaDbContext.Themes.Remove(foundTheme);
        return Result.Deleted;
    }

    /// <summary>
    /// Gets the currently active theme from the storage medium.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the active <see cref="ThemeEntity"/>, or an error.</returns>
    public async Task<Result<ThemeEntity?>> GetCurrentAsync(CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Themes
            .FirstOrDefaultAsync(theme => theme.IsCurrent == true, cancellationToken).ConfigureAwait(false);
    }
}
