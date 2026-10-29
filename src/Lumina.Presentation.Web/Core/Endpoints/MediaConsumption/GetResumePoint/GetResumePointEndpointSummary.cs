#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.MediaConsumption.GetResumePoint;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetResumePointEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetResumePointEndpointSummary : Summary<GetResumePointEndpoint, EmptyRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetResumePointEndpointSummary"/> class.
    /// </summary>
    public GetResumePointEndpointSummary()
    {
        Summary = "Retrieves the resume point of the current user for a media item.";
        Description = "Proxies the retrieval of the point at which the current user can resume a media item to the API server. The book reader calls it with the Book kind and the Id of the book it opens. A book resume point carries an opaque Location, while a music track resume point carries a TimeOffset. When the user has no resume point for the media item, or the media item no longer exists, the response succeeds without data instead of failing, so a reader can fall back to the beginning of the item. Any other upstream failure is rethrown to the application error handling, so a route or version mismatch is not mistaken for an absent resume point.";

        Params["mediaKind"] = "The kind of media to resume. Only the member names of MediaKind are accepted, such as Book or MusicTrack. Numeric values are rejected.";
        Params["mediaItemId"] = "The Id of the media item to resume.";

        Response(200, "The resume point of the current user is returned, or no data when the user has none.", "application/json",
            example: new SuccessResponse<ResumePointDto>(true, new ResumePointDto(
                Id: Guid.Parse("4d5e6f70-8192-4a3b-9c4d-5e6f708192a3"),
                LibraryId: Guid.Parse("3b3a19f3-1f5a-4d5a-9a3a-5c5a4a3a2a1a"),
                MediaItemId: Guid.Parse("7a6b5c4d-3e2f-4a1b-8c9d-0e1f2a3b4c5d"),
                MediaKind: MediaKind.Book,
                LastPosition: new MediaPositionDto(MediaPositionKind.Location, null, "epubcfi(/6/4[chapter-3]!/4/2/2)"),
                LastProgressFraction: 0.42,
                LastConsumedOnUtc: new DateTime(2026, 10, 24, 12, 12, 0, DateTimeKind.Utc)))
        );
    }
}
