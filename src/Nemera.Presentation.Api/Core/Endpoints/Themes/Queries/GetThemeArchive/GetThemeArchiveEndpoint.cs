#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.Mapping.Themes;
using Nemera.Application.Core.Themes.Management.Queries.GetThemeArchive;
using Nemera.Contracts.Requests.Themes;
using Nemera.Contracts.Responses.Themes;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.Themes;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Themes.Queries.GetThemeArchive;

/// <summary>
/// API endpoint for the <c>/themes/{themeId}/archive</c> route.
/// </summary>
public class GetThemeArchiveEndpoint : BaseEndpoint<GetThemeArchiveRequest, IResult>
{
    private readonly IQueryHandler<GetThemeArchiveQuery, Result<ThemeArchiveResponse>> _getThemeArchiveQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemeArchiveEndpoint"/> class.
    /// </summary>
    /// <param name="getThemeArchiveQueryHandler">Injected service for handling get theme archive queries.</param>
    public GetThemeArchiveEndpoint(IQueryHandler<GetThemeArchiveQuery, Result<ThemeArchiveResponse>> getThemeArchiveQueryHandler)
    {
        _getThemeArchiveQueryHandler = getThemeArchiveQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Themes.GET_THEME_ARCHIVE);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the downloadable archive of the theme identified by <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the theme to archive.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetThemeArchiveRequest request, CancellationToken cancellationToken)
    {
        Result<ThemeArchiveResponse> result = await _getThemeArchiveQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.File(success.Bytes, success.ContentType, fileDownloadName: success.FileName), Problem);
    }
}
