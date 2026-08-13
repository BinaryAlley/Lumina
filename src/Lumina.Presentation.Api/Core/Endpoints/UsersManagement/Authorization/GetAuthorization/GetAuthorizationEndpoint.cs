#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Authentication;
using Lumina.Application.Common.Mapping.Authorization;
using Lumina.Application.Core.UsersManagement.Authorization.Queries.GetAuthorization;
using Lumina.Contracts.Requests.Authorization;
using Lumina.Contracts.Responses.Authorization;
using Lumina.Presentation.Api.Common.Routes.UsersManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
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
    private readonly IQueryHandler<GetAuthorizationQuery, ErrorOr<AuthorizationResponse>> _getAuthorizationQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAuthorizationEndpoint"/> class.
    /// </summary>
    /// <param name="getAuthorizationQueryHandler">Injected service for handling get authorization queries.</param>
    public GetAuthorizationEndpoint(IQueryHandler<GetAuthorizationQuery, ErrorOr<AuthorizationResponse>> getAuthorizationQueryHandler)
    {
        _getAuthorizationQueryHandler = getAuthorizationQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
        Routes(ApiRoutes.Authorization.GET_AUTHORIZATION);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the authorization roles and permissions of an account.
    /// </summary>
    /// <param name="request">The request containing the account for which to get the authorization roles and permissions.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetAuthorizationRequest request, CancellationToken cancellationToken)
    {
        ErrorOr<AuthorizationResponse> result = await _getAuthorizationQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
