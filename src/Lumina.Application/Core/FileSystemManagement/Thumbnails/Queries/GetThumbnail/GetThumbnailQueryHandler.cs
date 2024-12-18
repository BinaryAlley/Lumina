#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.Mapping.FileSystemManagement.Thumbnails;
using Lumina.Contracts.Responses.FileSystemManagement.Thumbnails;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using Mediator;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Thumbnails.Queries.GetThumbnail;

/// <summary>
/// Handler for the query to get the file thumbnail.
/// </summary>
public class GetThumbnailQueryHandler : IRequestHandler<GetThumbnailQuery, ErrorOr<ThumbnailResponse>>
{
    private readonly IThumbnailService _thumbnailsService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThumbnailQueryHandler"/> class.
    /// </summary>
    /// <param name="thumbnailsService">Injected service for handling thumbnails.</param>
    public GetThumbnailQueryHandler(IThumbnailService thumbnailsService)
    {
        _thumbnailsService = thumbnailsService;
    }

    /// <summary>
    /// Gets the thumbnail for a file located at the specified path, with the specified quality.
    /// </summary>
    /// <param name="request">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> containing either a thumbnail, or an error.</returns>
    public async ValueTask<ErrorOr<ThumbnailResponse>> Handle(GetThumbnailQuery request, CancellationToken cancellationToken)
    {
        ErrorOr<Thumbnail> getThumbnailResult = await _thumbnailsService.GetThumbnailAsync(request.Path!, request.Quality, cancellationToken);
        return await ValueTask.FromResult(getThumbnailResult.Match(value => ErrorOrFactory.From(value.ToResponse()), errors => errors));
    }
}
