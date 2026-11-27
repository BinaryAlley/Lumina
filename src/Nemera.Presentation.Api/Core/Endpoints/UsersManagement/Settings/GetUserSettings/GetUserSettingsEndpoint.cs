#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Application.Common.CQRS;
using Nemera.Application.Core.UsersManagement.Settings.Queries.GetUserSettings;
using Nemera.Contracts.Responses.UsersManagement.Settings;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.UsersManagement;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.UsersManagement.Settings.GetUserSettings;

/// <summary>
/// API endpoint for the <c>/users/me/settings</c> route.
/// </summary>
/// <remarks>
/// Uses "me" because it is always for the currently authenticated user.
/// </remarks>
public class GetUserSettingsEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetUserSettingsQuery, Result<UserSettingsResponse>> _getUserSettingsQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserSettingsEndpoint"/> class.
    /// </summary>
    /// <param name="getUserSettingsQueryHandler">Injected service for handling get user settings queries.</param>
    public GetUserSettingsEndpoint(IQueryHandler<GetUserSettingsQuery, Result<UserSettingsResponse>> getUserSettingsQueryHandler)
    {
        _getUserSettingsQueryHandler = getUserSettingsQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Users.GET_USER_SETTINGS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the settings of the current user.
    /// </summary>
    /// <param name="request">The empty request, which carries no data.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        Result<UserSettingsResponse> result = await _getUserSettingsQueryHandler.HandleAsync(new GetUserSettingsQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
