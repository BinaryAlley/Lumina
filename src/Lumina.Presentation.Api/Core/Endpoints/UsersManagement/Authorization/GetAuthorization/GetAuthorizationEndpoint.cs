#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using Lumina.Application.Common.Mapping.Authentication;
using Lumina.Application.Common.Mapping.Authorization;
using Lumina.Contracts.Requests.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Presentation.Api.Common.Routes.UsersManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Mediator;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authorization.GetAuthorization;

/// <summary>
/// API endpoint for the <c>/auth/get-authorization</c> route.
/// </summary>
public class GetAuthorizationEndpoint : BaseEndpoint<GetAuthorizationRequest, IResult>
{
    private readonly ISender _sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAuthorizationEndpoint"/> class.
    /// </summary>
    /// <param name="sender">Injected service for mediating commands and queries.</param>
    public GetAuthorizationEndpoint(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Authorization.GET_AUTHORIZATION);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the authentication roles and permissions of an account.
    /// </summary>
    /// <param name="request">The request containing the account for which to get the authentication roles and permissions.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetAuthorizationRequest request, CancellationToken cancellationToken)
    {
        ErrorOr<GetAuthorizationResponse> result = await _sender.Send(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
