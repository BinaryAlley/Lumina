#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using Lumina.Application.Common.Mapping.Authentication;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Responses.Authentication;
using Lumina.Presentation.Api.Common.Routes.UsersManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authentication.RecoverPassword;

/// <summary>
/// API endpoint for the <c>/auth/recover-password</c> route.
/// </summary>
public class RecoverPasswordEndpoint : BaseEndpoint<RecoverPasswordRequest, IResult>
{
    private readonly ISender _sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecoverPasswordEndpoint"/> class.
    /// </summary>
    /// <param name="sender">Injected service for mediating commands and queries.</param>
    public RecoverPasswordEndpoint(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.POST);
        Routes(ApiRoutes.Authentication.RECOVER_PASSWORD);
        Version(1);
        AllowAnonymous();
        DontCatchExceptions();
        Options(x => x.RequireRateLimiting("authenticationPolicy"));
    }

    /// <summary>
    /// Recovers the password of an account.
    /// </summary>
    /// <param name="request">The request containing the account for which to recover the password.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(RecoverPasswordRequest request, CancellationToken cancellationToken)
    {
        ErrorOr<RecoverPasswordResponse> result = await _sender.Send(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
