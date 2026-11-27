#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.Mapping.Authentication;
using Nemera.Application.Core.UsersManagement.Authentication.Commands.RegisterUser;
using Nemera.Contracts.Requests.Authentication;
using Nemera.Contracts.Responses.Authentication;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.DependencyInjection;
using Nemera.Presentation.Api.Common.Routes.UsersManagement;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.UsersManagement.Authentication.Register;

/// <summary>
/// API endpoint for the <c>/auth/register</c> route.
/// </summary>
public class RegisterEndpoint : BaseEndpoint<RegistrationRequest, IResult>
{
    private readonly ICommandHandler<RegisterUserCommand, Result<RegistrationResponse>> _registerUserCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterEndpoint"/> class.
    /// </summary>
    /// <param name="registerUserCommandHandler">Injected service for handling register user commands.</param>
    public RegisterEndpoint(ICommandHandler<RegisterUserCommand, Result<RegistrationResponse>> registerUserCommandHandler)
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
        Options(x => x.RequireRateLimiting(PresentationApiLayerServices.AUTHENTICATION_RATE_LIMIT_POLICY));
    }

    /// <summary>
    /// Registers a new account.
    /// </summary>
    /// <param name="request">The request containing the account to be added.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(RegistrationRequest request, CancellationToken cancellationToken)
    {
        Result<RegistrationResponse> result = await _registerUserCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Created($"{BaseURL}api/v1{ApiRoutes.Users.GET_USER_BY_ID.Replace("{id}", success.Id.ToString())}", result.Value), Problem);
    }
}
