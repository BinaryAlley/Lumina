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
        Description = "Renders the page that shows the consumption statistics of the selected user: the total consumed duration, the counted sessions, the completed sessions, the stopped sessions, the completion rate, the per media kind breakdown, the tag and genre breakdown, and the recently consumed and most consumed media items. The page filters by library, media kind, and date range. An administrator can select another user or all users; a regular user sees only their own statistics. The page also offers the deletion of a history row, the clearing of the history, and the removal of the resume points.";

        Response(200, "The statistics page is rendered.");
    }
}
