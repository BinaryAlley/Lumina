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

namespace Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authentication.ChangePassword;

/// <summary>
/// API endpoint for the <c>/auth/change-password</c> route.
/// </summary>
public class ChangePasswordEndpoint : BaseEndpoint<ChangePasswordRequest, IResult>
{
    private readonly ISender _sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangePasswordEndpoint"/> class.
    /// </summary>
    /// <param name="sender">Injected service for mediating commands and queries.</param>
    public ChangePasswordEndpoint(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.POST);
        Routes(ApiRoutes.Authentication.CHANGE_PASSWORD);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Changes the password of an account.
    /// </summary>
    /// <param name="request">The request containing the account for which to change the password.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        ErrorOr<ChangePasswordResponse> result = await _sender.Send(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
