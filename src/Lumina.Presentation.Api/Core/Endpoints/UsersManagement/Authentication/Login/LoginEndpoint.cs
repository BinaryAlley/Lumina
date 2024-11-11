#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using Lumina.Application.Common.Mapping.Authentication;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Responses.Authentication;
using Lumina.Presentation.Api.Common.Routes.UsersManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Mediator;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authentication.Login;

/// <summary>
/// API endpoint for the <c>/auth/login</c> route.
/// </summary>
public class LoginEndpoint : BaseEndpoint<LoginRequest, IResult>
{
    private readonly ISender _sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginEndpoint"/> class.
    /// </summary>
    /// <param name="sender">Injected service for mediating commands and queries.</param>
    public LoginEndpoint(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.POST);
        Routes(ApiRoutes.Authentication.LOGIN_ACCOUNT);
        Version(1);
        AllowAnonymous();
        DontCatchExceptions();
    }

    /// <summary>
    /// Authenticates an account.
    /// </summary>
    /// <param name="request">The request containing the account to be authenticated.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        ErrorOr<LoginResponse> result = await _sender.Send(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
