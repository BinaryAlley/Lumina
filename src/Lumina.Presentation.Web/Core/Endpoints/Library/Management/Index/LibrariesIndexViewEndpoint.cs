#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Authorization;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using Lumina.Presentation.Web.Common.Enums.Authorization;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Management.Index;

/// <summary>
/// API endpoint for the <c>/{culture}/libraries/manage</c> route.
/// </summary>
public class LibrariesIndexViewEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibrariesIndexViewEndpoint"/> class.
    /// </summary>
    /// <param name="authorizationService">Injected service for authorization checks against the remote API.</param>
    public LibrariesIndexViewEndpoint(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.GET);
        Routes(WebRoutes.LibraryManagement.INDEX);
        DontAutoTag();
        Options(options => options.WithTags("Libraries"));
    }

    /// <summary>
    /// Displays the media libraries management view.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        // Changing and deleting a media library takes the permission to manage media libraries, or the administrator role, the same rule the API applies,
        // so the actions are only offered to the users the API would let through.
        bool canManageLibraries = await _authorizationService.IsInRoleAsync("Admin", cancellationToken).ConfigureAwait(false) ||
            await _authorizationService.HasPermissionAsync(AuthorizationPermission.CanCreateLibraries, cancellationToken).ConfigureAwait(false);
        return View("/Core/Views/Library/Management/Index.cshtml", new LibrariesIndexPageDto(canManageLibraries));
    }
}
