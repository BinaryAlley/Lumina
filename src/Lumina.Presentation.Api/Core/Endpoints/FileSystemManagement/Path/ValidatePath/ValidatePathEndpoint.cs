#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.Mapping.FileSystemManagement.Paths;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using Lumina.Presentation.Api.Common.Routes.FileSystemManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Mediator;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.FileSystemManagement.Path.ValidatePath;

/// <summary>
/// API endpoint for the <c>/path/validate</c> route.
/// </summary>
public class ValidatePathEndpoint : BaseEndpoint<ValidatePathRequest, IResult>
{
    private readonly ISender _sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatePathEndpoint"/> class.
    /// </summary>
    /// <param name="sender">Injected service for mediating commands and queries.</param>
    public ValidatePathEndpoint(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Path.VALIDATE);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Validates the validity of the file system path stored in <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the file system path to validate.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(ValidatePathRequest request, CancellationToken cancellationToken)
    {
        PathValidResponse result = await _sender.Send(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return TypedResults.Ok(result);
    }
}
