#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Themes;
using Lumina.Application.Core.Themes.Management.Queries.GetThemeTemplate;
using Lumina.Contracts.Requests.Themes;
using Lumina.Contracts.Responses.Themes;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Themes;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Themes.Queries.GetThemeTemplate;

/// <summary>
/// API endpoint for the <c>/themes/{themeId}/templates/{*pageKey}</c> route.
/// </summary>
public class GetThemeTemplateEndpoint : BaseEndpoint<GetThemeTemplateRequest, IResult>
{
    private readonly IQueryHandler<GetThemeTemplateQuery, Result<ThemeTemplateResponse>> _getThemeTemplateQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemeTemplateEndpoint"/> class.
    /// </summary>
    /// <param name="getThemeTemplateQueryHandler">Injected service for handling get theme template queries.</param>
    public GetThemeTemplateEndpoint(IQueryHandler<GetThemeTemplateQuery, Result<ThemeTemplateResponse>> getThemeTemplateQueryHandler)
    {
        _getThemeTemplateQueryHandler = getThemeTemplateQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Themes.GET_THEME_TEMPLATE);
        Version(1);
        AllowAnonymous();
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the template of the theme selected by the page key stored in <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the theme and the page key of the template to get.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetThemeTemplateRequest request, CancellationToken cancellationToken)
    {
        Result<ThemeTemplateResponse> result = await _getThemeTemplateQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
