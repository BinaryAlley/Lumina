#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Authentication;
using Lumina.Application.Core.UsersManagement.Authentication.Queries.LoginUser;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Responses.Authentication;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.UsersManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
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
    private readonly IQueryHandler<LoginUserQuery, Result<LoginResponse>> _loginQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginEndpoint"/> class.
    /// </summary>
    /// <param name="loginQueryHandler">Injected service for handling login queries.</param>
    public LoginEndpoint(IQueryHandler<LoginUserQuery, Result<LoginResponse>> loginQueryHandler)
    {
        _loginQueryHandler = loginQueryHandler;
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
        Options(x => x.RequireRateLimiting("authenticationPolicy"));
    }

    /// <summary>
    /// Authenticates an account.
    /// </summary>
    /// <param name="request">The request containing the account to be authenticated.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        Result<LoginResponse> result = await _loginQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
