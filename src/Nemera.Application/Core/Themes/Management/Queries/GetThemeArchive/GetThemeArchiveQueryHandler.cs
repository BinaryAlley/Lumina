#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Models.DTO.Themes;
using Lumina.Application.Common.Infrastructure.Themes;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Contracts.Responses.Themes;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Themes.Management.Queries.GetThemeArchive;

/// <summary>
/// Handler for the query to get the downloadable archive of a theme.
/// </summary>
public class GetThemeArchiveQueryHandler : IQueryHandler<GetThemeArchiveQuery, Result<ThemeArchiveResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IThemeService _themeService;
    private readonly IValidator<GetThemeArchiveQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemeArchiveQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="themeService">Injected service for the server-side storage and serving of theme packs.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetThemeArchiveQueryHandler(IUnitOfWork unitOfWork, IThemeService themeService, IValidator<GetThemeArchiveQuery> validator)
    {
        _unitOfWork = unitOfWork;
        _themeService = themeService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the query to get the downloadable archive of a theme.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the archive of the theme, or an error.
    /// </returns>
    public async Task<Result<ThemeArchiveResponse>> HandleAsync(GetThemeArchiveQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return validationResult;

        Result<ThemeEntity?> getThemeResult = await _unitOfWork.ThemeRepository.GetByThemeIdAsync(query.ThemeId!, cancellationToken).ConfigureAwait(false);
        if (getThemeResult.IsFailure)
            return getThemeResult.Errors;

        ThemeEntity? theme = getThemeResult.Value;
        if (theme is null || theme.IsDeleted)
            return DomainErrors.Themes.ThemeNotFound;

        Result<ThemeArchiveDto> archiveResult = await _themeService.BuildArchiveAsync(theme.ThemeId, cancellationToken).ConfigureAwait(false);
        if (archiveResult.IsFailure)
            return archiveResult.Errors;

        return new ThemeArchiveResponse(archiveResult.Value.Bytes, archiveResult.Value.FileName, archiveResult.Value.ContentType);
    }
}
