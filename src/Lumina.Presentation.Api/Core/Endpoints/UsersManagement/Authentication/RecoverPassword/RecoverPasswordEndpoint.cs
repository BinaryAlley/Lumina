#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Authentication;
using Lumina.Application.Core.UsersManagement.Authentication.Commands.RecoverPassword;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Responses.Authentication;
using Lumina.Presentation.Api.Common.Routes.UsersManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
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
    private readonly ICommandHandler<RecoverPasswordCommand, ErrorOr<RecoverPasswordResponse>> _recoverPasswordCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecoverPasswordEndpoint"/> class.
    /// </summary>
    /// <param name="recoverPasswordCommandHandler">Injected service for handling recover password commands.</param>
    public RecoverPasswordEndpoint(ICommandHandler<RecoverPasswordCommand, ErrorOr<RecoverPasswordResponse>> recoverPasswordCommandHandler)
    {
        _recoverPasswordCommandHandler = recoverPasswordCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.POST);
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
        ErrorOr<RecoverPasswordResponse> result = await _recoverPasswordCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
