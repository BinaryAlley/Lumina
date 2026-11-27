#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.Mapping.Authentication;
using Nemera.Application.Core.Maintenance.ApplicationSetup.Commands.SetupApplication;
using Nemera.Contracts.Requests.Authentication;
using Nemera.Contracts.Responses.Authentication;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.Maintenance;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Maintenance.ApplicationSetup;

/// <summary>
/// API endpoint for the <c>/initialization</c> route.
/// </summary>
public class SetupApplicationEndpoint : BaseEndpoint<RegistrationRequest, IResult>
{
    private readonly ICommandHandler<SetupApplicationCommand, Result<RegistrationResponse>> _setupApplicationCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetupApplicationEndpoint"/> class.
    /// </summary>
    /// <param name="setupApplicationCommandHandler">Injected service for handling setup application commands.</param>
    public SetupApplicationEndpoint(ICommandHandler<SetupApplicationCommand, Result<RegistrationResponse>> setupApplicationCommandHandler)
    {
        _setupApplicationCommandHandler = setupApplicationCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.POST);
        Routes(ApiRoutes.Initialization.SETUP_APPLICATION);
        Version(1);
        AllowAnonymous();
        DontCatchExceptions();
    }

    /// <summary>
    /// Performs the initial application setup, including creating the Admin account.
    /// </summary>
    /// <param name="request">The request containing the Admin user to be added.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(RegistrationRequest request, CancellationToken cancellationToken)
    {
        Result<RegistrationResponse> result = await _setupApplicationCommandHandler.HandleAsync(request.ToSetupCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Created($"{BaseURL}api/v1{Api.Common.Routes.UsersManagement.ApiRoutes.Users.GET_USER_BY_ID.Replace("{id}", success.Id.ToString())}", result.Value), Problem);
    }
}
