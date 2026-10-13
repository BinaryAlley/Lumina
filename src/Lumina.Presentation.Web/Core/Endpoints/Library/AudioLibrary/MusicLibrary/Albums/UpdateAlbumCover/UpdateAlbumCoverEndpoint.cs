#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.UpdateAlbumCover;

/// <summary>
/// API endpoint for the <c>/{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/api-update-cover</c> route.
/// </summary>
public class UpdateAlbumCoverEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateAlbumCoverEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public UpdateAlbumCoverEndpoint(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.PUT);
        Routes(WebRoutes.Music.UPDATE_ALBUM_COVER);
        DontAutoTag();
        Options(options => options.WithTags("Music"));
        EnableAntiforgery();
    }

    /// <summary>
    /// Updates the cover image of the album with the image uploaded in the multipart form of the request.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        // The endpoint takes no typed request, because its body is a multipart file, so the album id is read from the {albumId} route value and the
        // owning library from the {libraryId} query string; all the ids are normalized to Guids before they are substituted into the upstream URL, so
        // that a crafted value can never escape its URL segment, and an unparseable id is reported by the API as a missing id. A missing file is
        // forwarded as a multipart form without any file, so that the upstream API reports it with its own validation error.
        Guid artistId = Guid.TryParse(HttpContext.Request.RouteValues["artistId"]?.ToString(), out Guid parsedArtistId) ? parsedArtistId : Guid.Empty;
        Guid albumId = Guid.TryParse(HttpContext.Request.RouteValues["albumId"]?.ToString(), out Guid parsedAlbumId) ? parsedAlbumId : Guid.Empty;
        string libraryId = Guid.TryParse(HttpContext.Request.Query["libraryId"].ToString(), out Guid parsedLibraryId) ? parsedLibraryId.ToString() : Guid.Empty.ToString();

        IFormFile? cover = null;
        if (HttpContext.Request.HasFormContentType)
        {
            try
            {
                cover = HttpContext.Request.Form.Files.Count > 0 ? HttpContext.Request.Form.Files[0] : null;
            }
            catch (InvalidDataException)
            {
                cover = null;
            }
        }

        // The cover stream must stay open while the file is streamed to the API, and is disposed when the request completes.
        Stream? coverStream = cover?.OpenReadStream();
        try
        {
            UpdateMusicCoverDto response = await _apiHttpClient.PutMultipartAsync<UpdateMusicCoverDto>(
                ApiRoutes.Music.UPDATE_ALBUM_COVER.Replace("{libraryId}", libraryId).Replace("{artistId}", artistId.ToString()).Replace("{albumId}", albumId.ToString()), coverStream, cover?.FileName, "cover", cancellationToken).ConfigureAwait(false);
            return JsonSuccess(response.CoverPath);
        }
        finally
        {
            coverStream?.Dispose();
        }
    }
}
