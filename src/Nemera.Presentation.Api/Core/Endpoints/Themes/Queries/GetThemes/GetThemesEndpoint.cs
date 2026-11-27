#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Application.Common.CQRS;
using Nemera.Application.Core.Themes.Management.Queries.GetThemes;
using Nemera.Contracts.Responses.Themes;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.Themes;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Themes.Queries.GetThemes;

/// <summary>
/// API endpoint for the <c>/themes</c> route.
/// </summary>
public class GetThemesEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetThemesQuery, Result<IReadOnlyList<ThemeResponse>>> _getThemesQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemesEndpoint"/> class.
    /// </summary>
    /// <param name="getThemesQueryHandler">Injected service for handling get themes queries.</param>
    public GetThemesEndpoint(IQueryHandler<GetThemesQuery, Result<IReadOnlyList<ThemeResponse>>> getThemesQueryHandler)
    {
        _getThemesQueryHandler = getThemesQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Themes.GET_THEMES);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the list of all the installed themes.
    /// </summary>
    /// <param name="_">The empty request, which carries no data.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest _, CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<ThemeResponse>> result = await _getThemesQueryHandler.HandleAsync(new GetThemesQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
