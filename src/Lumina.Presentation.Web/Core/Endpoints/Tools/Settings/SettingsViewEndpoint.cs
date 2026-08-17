#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.UsersManagement;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Tools.Settings;

/// <summary>
/// API endpoint for the <c>/{culture}/tools/settings</c> route.
/// </summary>
public class SettingsViewEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.GET);
        Routes(WebRoutes.Settings.INDEX);
        DontAutoTag();
        Options(options => options.WithTags("Settings"));
    }

    /// <summary>
    /// Displays the user settings view.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        UserSettingsDto settings = new();
        return Task.FromResult(View("/Core/Views/Tools/Settings.cshtml", settings));
    }
}
