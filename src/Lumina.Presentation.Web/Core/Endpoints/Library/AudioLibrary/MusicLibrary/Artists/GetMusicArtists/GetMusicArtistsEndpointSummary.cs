#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Artists;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.GetMusicArtists;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetMusicArtistsEndpoint"/> endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetMusicArtistsEndpointSummary : Summary<GetMusicArtistsEndpoint, GetMusicArtistsLiteRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetMusicArtistsEndpointSummary"/> class.
    /// </summary>
    public GetMusicArtistsEndpointSummary()
    {
        Summary = "Retrieves the lightweight details of the music artists of a media library.";
        Description = "Retrieves the lightweight read models of the music artists of the media library identified by the route.";

        RequestParam(r => r.LibraryId, "The Id of the media library whose artists are retrieved. Required.");
        RequestParam(r => r.CurrentPage, "The page of results to retrieve. Optional.");
        RequestParam(r => r.PerPage, "The maximum number of artists to retrieve per page. Optional.");
        RequestParam(r => r.SearchTerm, "The search term used to filter the artists by name. Optional.");
        RequestParam(r => r.FilterAlphaKey, "The alpha key used to filter the results by the first character of their name, for the alpha picker. Optional.");
        RequestParam(r => r.ShouldIgnoreThePrefixForAlphaPicker, "Whether the leading \"The \" prefix of a name is ignored when computing the alpha key, or not. Optional.");
        RequestParam(r => r.SortBy, "The name of the field by which to sort the results. Optional.");
        RequestParam(r => r.SortOrder, "The direction in which to sort the results. Optional.");

        ExampleRequest = new GetMusicArtistsLiteRequest(
            LibraryId: Guid.NewGuid(),
            CurrentPage: 1,
            PerPage: 10,
            SearchTerm: "queen",
            FilterAlphaKey: null,
            ShouldIgnoreThePrefixForAlphaPicker: false,
            SortBy: null,
            SortOrder: null
        );

        Response(200, "The lightweight details of the music artists of the media library are retrieved.");
    }
}
