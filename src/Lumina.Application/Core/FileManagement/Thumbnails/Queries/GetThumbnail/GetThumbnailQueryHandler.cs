#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Contracts.Responses.FileManagement;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.Services;
using Lumina.Domain.Core.Aggregates.FileManagement.FileManagementAggregate.ValueObjects;
using Mapster;
using Mediator;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.FileManagement.Thumbnails.Queries.GetThumbnail;

/// <summary>
/// Handler for the query to get the file thumbnail.
/// </summary>
public class GetThumbnailQueryHandler : IRequestHandler<GetThumbnailQuery, ErrorOr<ThumbnailResponse>>
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly IThumbnailService _thumbnailsService;
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="GetThumbnailQueryHandler"/> class.
    /// </summary>
    /// <param name="thumbnailsService">Injected service for handling thumbnails.</param>
    public GetThumbnailQueryHandler(IThumbnailService thumbnailsService)
    {
        _thumbnailsService = thumbnailsService;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Gets the thumbnail for a file located at the specified path, with the specified quality.
    /// </summary>
    /// <param name="request">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> containing either a thumbnail, or an error.</returns>
    public async ValueTask<ErrorOr<ThumbnailResponse>> Handle(GetThumbnailQuery request, CancellationToken cancellationToken)
    {
        ErrorOr<Thumbnail> result = await _thumbnailsService.GetThumbnailAsync(request.Path, request.Quality, cancellationToken);
        return await ValueTask.FromResult(result.Match(values => ErrorOrFactory.From(result.Value.Adapt<ThumbnailResponse>()), errors => errors));
    }
    #endregion
}