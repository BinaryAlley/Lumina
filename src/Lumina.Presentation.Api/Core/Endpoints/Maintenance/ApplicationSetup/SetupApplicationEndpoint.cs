#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using Lumina.Contracts.Requests.Authentication;
using Lumina.Contracts.Responses.Authentication;
using Lumina.Application.Common.Mapping.Authentication;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Mediator;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
using Lumina.Presentation.Api.Common.Routes.Maintenance;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Maintenance.ApplicationSetup;

/// <summary>
/// API endpoint for the <c>/initialization</c> route.
/// </summary>
public class SetupApplicationEndpoint : BaseEndpoint<RegistrationRequest, IResult>
{
    private readonly ISender _sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetupApplicationEndpoint"/> class.
    /// </summary>
    /// <param name="sender">Injected service for mediating commands and queries.</param>
    public SetupApplicationEndpoint(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.POST);
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
        ErrorOr<RegistrationResponse> result = await _sender.Send(request.ToSetupCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Created($"{BaseURL}api/v1{Api.Common.Routes.UsersManagement.ApiRoutes.Users.GET_USER_BY_ID}/{result.Value.Id}", result.Value), Problem);
    }
}
