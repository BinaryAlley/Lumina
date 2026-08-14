#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.FileSystemManagement.Thumbnails;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Contracts.Responses.FileSystemManagement.Thumbnails;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Thumbnails.Queries.GetThumbnail;

/// <summary>
/// Handler for the query to get the file thumbnail.
/// </summary>
public class GetThumbnailQueryHandler : IQueryHandler<GetThumbnailQuery, Result<ThumbnailResponse>>
{
    private readonly IThumbnailService _thumbnailsService;
    private readonly IValidator<GetThumbnailQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThumbnailQueryHandler"/> class.
    /// </summary>
    /// <param name="thumbnailsService">Injected service for handling thumbnails.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetThumbnailQueryHandler(IThumbnailService thumbnailsService, IValidator<GetThumbnailQuery> validator)
    {
        _thumbnailsService = thumbnailsService;
        _validator = validator;
    }

    /// <summary>
    /// Gets the thumbnail for a file located at the specified path, with the specified quality.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{T}"/> containing either a thumbnail, or an error.</returns>
    public async Task<Result<ThumbnailResponse>> HandleAsync(GetThumbnailQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return validationResult;

        Result<Thumbnail> getThumbnailResult = await _thumbnailsService.GetThumbnailAsync(query.Path!, query.Quality, cancellationToken);
        return await ValueTask.FromResult(getThumbnailResult.Match(value => Result.From(value.ToResponse()), errors => errors));
    }
}
