#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Mapping.Themes;
using Lumina.Contracts.Responses.Themes;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.Themes.Management.Queries.GetThemes;

/// <summary>
/// Handler for the query to get all installed themes.
/// </summary>
public class GetThemesQueryHandler : IQueryHandler<GetThemesQuery, Result<IReadOnlyList<ThemeResponse>>>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemesQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public GetThemesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the query to get all installed themes.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a collection of <see cref="ThemeResponse"/>, or an error.
    /// </returns>
    public async Task<Result<IReadOnlyList<ThemeResponse>>> HandleAsync(GetThemesQuery query, CancellationToken cancellationToken)
    {
        Result<IEnumerable<ThemeEntity>> getThemesResult = await _unitOfWork.ThemeRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        if (getThemesResult.IsFailure)
            return getThemesResult.Errors;

        // deleted bundled themes are returned as well, so the administration page can offer to restore them
        return Result.From<IReadOnlyList<ThemeResponse>>([.. getThemesResult.Value
            .OrderBy(theme => theme.IsDeleted)
            // the shipped bundled themes are listed before the user themes, so the defaults stay the most visible
            .ThenByDescending(theme => theme.InstallSource == ThemeInstallSource.Bundled)
            .ThenBy(theme => theme.Name, StringComparer.OrdinalIgnoreCase)
            .Select(theme => theme.ToResponse())]);
    }
}
