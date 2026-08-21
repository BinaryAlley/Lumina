#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Mapping.Themes;
using Lumina.Contracts.Responses.Themes;
using Lumina.Domain.Common.Primitives;
using System.Threading;
using System.Threading.Tasks;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Themes.Management.Queries.GetCurrentTheme;

/// <summary>
/// Handler for the query to get the currently active theme.
/// </summary>
public class GetCurrentThemeQueryHandler : IQueryHandler<GetCurrentThemeQuery, Result<ThemeResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCurrentThemeQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    public GetCurrentThemeQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the query to get the currently active theme.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the active <see cref="ThemeResponse"/>, or an error.
    /// </returns>
    public async Task<Result<ThemeResponse>> HandleAsync(GetCurrentThemeQuery query, CancellationToken cancellationToken)
    {
        Result<ThemeEntity?> getCurrentResult = await _unitOfWork.ThemeRepository.GetCurrentAsync(cancellationToken).ConfigureAwait(false);
        if (getCurrentResult.IsFailure)
            return getCurrentResult.Errors;

        if (getCurrentResult.Value is null)
            return DomainErrors.Themes.ThemeNotFound;

        return getCurrentResult.Value.ToResponse();
    }
}
