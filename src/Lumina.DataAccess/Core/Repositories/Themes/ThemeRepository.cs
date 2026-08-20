#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Common.DataAccess.Repositories.Themes;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.Common.Errors;
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
        bool themeExists = await _luminaDbContext.Themes.AnyAsync(repositoryTheme => repositoryTheme.Id == theme.Id, cancellationToken).ConfigureAwait(false);
        if (themeExists)
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
    /// Gets all themes from the storage medium.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a collection of <see cref="ThemeEntity"/>, or an error.</returns>
    public async Task<Result<IEnumerable<ThemeEntity>>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _luminaDbContext.Themes.ToListAsync(cancellationToken).ConfigureAwait(false);
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
