#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Maintenance.ApplicationSetup.Queries.CheckInitialization;
using Lumina.Contracts.Responses.UsersManagement;
using Lumina.Presentation.Api.Common.Routes.Maintenance;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Maintenance.ApplicationSetup;

/// <summary>
/// API endpoint for the <c>/initialization</c> route.
/// </summary>
public class CheckInitializationEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IQueryHandler<CheckInitializationQuery, InitializationResponse> _checkInitializationQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckInitializationEndpoint"/> class.
    /// </summary>
    /// <param name="checkInitializationQueryHandler">Injected service for handling check initialization queries.</param>
    public CheckInitializationEndpoint(IQueryHandler<CheckInitializationQuery, InitializationResponse> checkInitializationQueryHandler)
    {
        _checkInitializationQueryHandler = checkInitializationQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
        Routes(ApiRoutes.Initialization.CHECK_INITIALIZATION);
        Version(1);
        AllowAnonymous();
        DontCatchExceptions();
    }

    /// <summary>
    /// Checks the initialization status of the application (if the Admin account exists).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest _, CancellationToken cancellationToken)
    {
        InitializationResponse result = await _checkInitializationQueryHandler.HandleAsync(new CheckInitializationQuery(), cancellationToken).ConfigureAwait(false);
        return TypedResults.Ok(result);
    }
}
