#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Authentication;
using Lumina.Application.Core.UsersManagement.Authentication.Commands.RegisterUser;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Responses.Authentication;
using Lumina.Presentation.Api.Common.Routes.UsersManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authentication.Register;

/// <summary>
/// API endpoint for the <c>/auth/register</c> route.
/// </summary>
public class RegisterEndpoint : BaseEndpoint<RegistrationRequest, IResult>
{
    private readonly ICommandHandler<RegisterUserCommand, ErrorOr<RegistrationResponse>> _registerUserCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterEndpoint"/> class.
    /// </summary>
    /// <param name="registerUserCommandHandler">Injected service for handling register user commands.</param>
    public RegisterEndpoint(ICommandHandler<RegisterUserCommand, ErrorOr<RegistrationResponse>> registerUserCommandHandler)
    {
        _registerUserCommandHandler = registerUserCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.POST);
        Routes(ApiRoutes.Authentication.REGISTER_ACCOUNT);
        Version(1);
        AllowAnonymous();
        DontCatchExceptions();
        Options(x => x.RequireRateLimiting("authenticationPolicy"));
    }

    /// <summary>
    /// Registers a new account.
    /// </summary>
    /// <param name="request">The request containing the account to be added.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(RegistrationRequest request, CancellationToken cancellationToken)
    {
        ErrorOr<RegistrationResponse> result = await _registerUserCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Created($"{BaseURL}api/v1{ApiRoutes.Users.GET_USER_BY_ID.Replace("{id}", success.Id.ToString())}", result.Value), Problem);
    }
}
