#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Themes;
using Lumina.Application.Core.Themes.Management.Commands.DeleteTheme;
using Lumina.Contracts.Requests.Themes;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Themes;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Themes.Management.DeleteTheme;

/// <summary>
/// API endpoint for the <c>/themes/{themeId}</c> route.
/// </summary>
public class DeleteThemeEndpoint : BaseEndpoint<DeleteThemeRequest, IResult>
{
    private readonly ICommandHandler<DeleteThemeCommand, Result<Success>> _deleteThemeCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteThemeEndpoint"/> class.
    /// </summary>
    /// <param name="deleteThemeCommandHandler">Injected service for handling delete theme commands.</param>
    public DeleteThemeEndpoint(ICommandHandler<DeleteThemeCommand, Result<Success>> deleteThemeCommandHandler)
    {
        _deleteThemeCommandHandler = deleteThemeCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.DELETE);
        Routes(ApiRoutes.Themes.DELETE_THEME);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Deletes the theme identified by <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the theme to delete.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(DeleteThemeRequest request, CancellationToken cancellationToken)
    {
        Result<Success> result = await _deleteThemeCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.NoContent(), Problem);
    }
}
