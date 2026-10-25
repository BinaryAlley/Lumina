#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.Index;

/// <summary>
/// Class used for providing a textual description for the <see cref="StatisticsIndexViewEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class StatisticsIndexViewEndpointSummary : Summary<StatisticsIndexViewEndpoint>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StatisticsIndexViewEndpointSummary"/> class.
    /// </summary>
    public StatisticsIndexViewEndpointSummary()
    {
        Summary = "Renders the statistics page.";
        Description = "Renders the page that shows the statistics of the current user: the total listening time, the counted plays, the completed plays, the completion rate, and the recently played and most played tracks.";

        Response(200, "The statistics page is rendered.");
    }
}
