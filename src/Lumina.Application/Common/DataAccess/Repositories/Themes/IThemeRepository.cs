#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Common.DataAccess.Repositories.Common.Actions;
using Lumina.Application.Common.DataAccess.Repositories.Common.Base;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Common.DataAccess.Repositories.Themes;

/// <summary>
/// Interface for the repository for themes.
/// </summary>
public interface IThemeRepository : IRepository<ThemeEntity>,
                                    IGetAllRepositoryAction<ThemeEntity>,
                                    IInsertRepositoryAction<ThemeEntity>,
                                    IUpdateRepositoryAction<ThemeEntity>
{
    /// <summary>
    /// Gets a theme identified by its manifest <paramref name="themeId"/> from the storage medium.
    /// </summary>
    /// <param name="themeId">The manifest id of the theme to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either a <see cref="ThemeEntity"/> identified by <paramref name="themeId"/>, or an error.</returns>
    Task<Result<ThemeEntity?>> GetByThemeIdAsync(string themeId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the currently active theme from the storage medium.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> containing either the active <see cref="ThemeEntity"/>, or an error.</returns>
    Task<Result<ThemeEntity?>> GetCurrentAsync(CancellationToken cancellationToken);
}
