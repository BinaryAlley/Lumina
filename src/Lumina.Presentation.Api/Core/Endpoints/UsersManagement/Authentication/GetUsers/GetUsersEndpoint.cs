#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.UsersManagement.Authentication.Queries.GetUsers;
using Lumina.Contracts.Responses.UsersManagement.Users;
using Lumina.Presentation.Api.Common.Routes.UsersManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Authentication.GetUsers;

/// <summary>
/// API endpoint for the <c>/auth/users</c> route.
/// </summary>
public class GetUsersEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetUsersQuery, ErrorOr<IEnumerable<UserResponse>>> _getUsersQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUsersEndpoint"/> class.
    /// </summary>
    /// <param name="getUsersQueryHandler">Injected service for handling get users queries.</param>
    public GetUsersEndpoint(IQueryHandler<GetUsersQuery, ErrorOr<IEnumerable<UserResponse>>> getUsersQueryHandler)
    {
        _getUsersQueryHandler = getUsersQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
        Routes(ApiRoutes.Authentication.USERS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the list of users.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest _, CancellationToken cancellationToken)
    {
        ErrorOr<IEnumerable<UserResponse>> result = await _getUsersQueryHandler.HandleAsync(new GetUsersQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
